using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TileManager;
using System.Linq;

public class PlayerAI : MonoBehaviour
{

    public GameObject pacman;
    public GameObject[] ghosts;
    public TileManager tileManager;

    public bool activated;

    // FSM state
    private PlayerState currentState;

    public enum AI_MODE { FSM, BT, UTIL }

    public AI_MODE aiMode = AI_MODE.FSM;

    // Behaviour Tree
    public BehaviourTree tree;
    BTNode.Status treeStatus = BTNode.Status.RUNNING;
    Corridor targetCorridor = null;
    Vector2 _BTmoveDir = Vector2.zero;

    // Utility AI
    public Action[] actionsAvailable;
    public UtilityAI utilityAI;
    public Vector2 utilAImoveDir = Vector2.zero;
    public float threatValue = 0f;
    public float poweredUpValue = 0f;

    #region Singleton

    private static PlayerAI _instance = null;

    public static PlayerAI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<PlayerAI>();
                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            if (this != _instance)
                Destroy(this.gameObject);
        }
    }

    public static void DestroySelf()
    {

        Destroy(GameObject.Find("PlayerAI"));
    }

    #endregion

    void Start()
    {
        activated = false;
        // FSM init
        currentState = new SeekPelletsState();

        // BT init
        BuildBTree();

        //tree.PrintTree();

        // Utility AI init
        utilityAI = GetComponent<UtilityAI>();

    }

    void Update()
    {
        if (GameManager.lives <= 0 || !activated)
            return;
        if (aiMode == AI_MODE.FSM)
        {
            currentState = currentState.Process();
        }
        else if (aiMode == AI_MODE.BT)
        {
            /*if (treeStatus != BTNode.Status.SUCCESS)
                treeStatus = tree.Process();*/
            treeStatus = tree.Process();
        }
        else if (aiMode == AI_MODE.UTIL)
        {
            if (utilityAI.finishedDeciding)
            {
                utilityAI.finishedDeciding = false;
                utilityAI.bestAction.Execute(this);
            }

            // update stats
            UpdateConsiderationValues();

        }
        else
        {
            Debug.LogError("AI Mode undefined!");
        }
    }

    // builds/initialise the Behaviour Tree
    void BuildBTree()
    {
        // BT init
        tree = new BehaviourTree();

        BTSequence chaseGhosts = new BTSequence("Chase ghosts");
        BTSequence runFromGhosts = new BTSequence("Run from ghosts");
        BTSequence seekPellets = new BTSequence("Seek pellets");

        BTLeaf isGhostsNearby = new BTLeaf("Is ghost nearby?", BTIsGhostsNearby);
        BTLeaf run = new BTLeaf("Run", BTRun, true);

        runFromGhosts.AddChild(isGhostsNearby);
        runFromGhosts.AddChild(run);

        BTLeaf isPoweredUp = new BTLeaf("Is powered up?", BTIsPoweredUp);
        BTInverter inverter1 = new BTInverter("Inverter 1");
        BTLeaf isPoweringDown = new BTLeaf("Is powering down?", BTIsPoweringDown);
        BTLeaf pursueGhosts = new BTLeaf("Pursue ghosts", BTPursueGhosts, true);

        inverter1.AddChild(isPoweringDown);
        chaseGhosts.AddChild(isPoweredUp);
        chaseGhosts.AddChild(inverter1);
        chaseGhosts.AddChild(pursueGhosts);

        BTLeaf scanForBestCorridor = new BTLeaf("Scan for best corridor", BTScanForBestCorridor);
        BTLeaf moveToCorridor = new BTLeaf("Move to corridor", BTMoveToCorridor);
        BTLeaf eatPellets = new BTLeaf("Eat pellets", BTEatPellets, true);

        seekPellets.AddChild(scanForBestCorridor);
        seekPellets.AddChild(moveToCorridor);
        seekPellets.AddChild(eatPellets);

        tree.AddChild(runFromGhosts);
        tree.AddChild(chaseGhosts);
        tree.AddChild(seekPellets);
    }

    public Vector2 GetAIDirection()
    {
        if (aiMode == AI_MODE.FSM)
            return currentState.GetDirection();
        else if (aiMode == AI_MODE.BT)
            return _BTmoveDir;
        else if (aiMode == AI_MODE.UTIL)
            return utilAImoveDir;
        else
            Debug.LogError("AI Mode undefined!");

        return Vector2.zero;
    }

    public string GetCurrentFSMState()
    {
        if (currentState == null)
            return "-";

        return currentState.name.ToString();
    }

    public bool PoweringDown()
    {
        GameManager _gm = GameManager.Instance;
        if (Time.time >= (_gm._timeToCalm- _gm.scareLength)+ _gm.scareLength*0.66f)
            return true;
        else
            return false;
    }

    // returns the number of scared ghosts
    public int NumOfScaredGhosts()
    {
        List<GhostMove> ghostMoves = new List<GhostMove>();
        ghostMoves.AddRange(ghosts.Select(go => go.GetComponent<GhostMove>()));

        int count = 0;
        foreach (var ghost in ghostMoves)
        {
            if (ghost.state == GhostMove.State.Run)
                count++;
        }

        return count;
    }

    #region Behaviour Tree Processes

    BTNode.Status BTIsPoweredUp()
    {
        if (GameManager.scared)
            return BTNode.Status.SUCCESS;
        else
            return BTNode.Status.FAILURE;
    }

    BTNode.Status BTIsPoweringDown()
    {
        if (PoweringDown())
            return BTNode.Status.SUCCESS;
        else
            return BTNode.Status.FAILURE;
    }

    BTNode.Status BTPursueGhosts()
    {
        // all ghosts were eaten
        if (GetClosestGhost(true) == null)
            return BTNode.Status.FAILURE;

        // chase ghost
        Tuple<Node, Stack<Vector2>> t = PathfindTargetFullInfo(GetClosestGhost(true));
        VisualizationManager.DisplayPathfindByNode(t.Item1, Color.green);

        if (t.Item2.Count > 0)
            _BTmoveDir = t.Item2.Peek();

        return BTNode.Status.SUCCESS;
    }

    BTNode.Status BTIsGhostsNearby()
    {
        if (GhostInSight())
            return BTNode.Status.SUCCESS;
        else
            return BTNode.Status.FAILURE;
    }

    BTNode.Status BTRun()
    {
        List<Tuple<Node, Stack<Vector2>>> dangerPaths = new List<Tuple<Node, Stack<Vector2>>>();

        foreach (var ghost in ghosts)
            dangerPaths.Add(PathfindTargetFullInfo(ghost));

        dangerPaths.Sort(SortByDistance);

        List<Vector2> toAvoid = new List<Vector2>();
        foreach (var path in dangerPaths)
        {
            // ghost still in cage
            if (path.Item2.Count == 0)
                continue;

            VisualizationManager.DisplayPathfindByNode(path.Item1, Color.red);

            if (!toAvoid.Contains(path.Item2.Peek()))
                toAvoid.Add(path.Item2.Peek());
        }

        foreach (var direction in PossibleDirections())
        {
            if (!toAvoid.Contains(direction))
            {
                _BTmoveDir = direction;
                return BTNode.Status.SUCCESS;
            }
            else
                continue;
        }

        _BTmoveDir = toAvoid[toAvoid.Count - 1];

        return BTNode.Status.SUCCESS;
    }

    BTNode.Status ScanForBestCorridor(float scanRadius = 5f)
    {
        // scans for corridors in a radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll((Vector2)pacman.transform.position, scanRadius);
        List<Corridor> corridors = colliders.Select(f => f.GetComponent<Corridor>()).ToList();

        Corridor bestCorridor = null;

        foreach (Corridor corridor in corridors)
        {
            // collider that is not a corridor
            if (corridor == null)
                continue;

            // skips corridors with ghosts
            // might not be efficient as ghosts will move through corridors quickly
            // as the corridors are small
            if (corridor.ghost_count > 0)
                continue;
            // skips corridors with no pellets
            // no incentive to target such corridors
            if (corridor.pellet_count <= 0)
                continue;
            // set first best option
            if (bestCorridor == null)
            {
                bestCorridor = corridor;
            }
            // compare best corridor with current corridor
            else if (corridor.pellet_count > bestCorridor.pellet_count)
            {
                bestCorridor = corridor;
            }
        }

        if (bestCorridor == null)
        {
            // max radius hit
            // either ghost occupy the last few corridors with pellets,
            // or there is a problem where all pellets are eaten but game still runs
            if (scanRadius >= 40f)
                return BTNode.Status.FAILURE;

            // recursively scan with increased radius
            return ScanForBestCorridor(scanRadius + 2f);

        }

        targetCorridor = bestCorridor;

        return BTNode.Status.SUCCESS;
    }

    BTNode.Status BTScanForBestCorridor()
    {
        return ScanForBestCorridor();
    }

    BTNode.Status BTMoveToCorridor()
    {
        if (targetCorridor == null)
            return BTNode.Status.FAILURE;
        // corridor is for some reason empty,
        // even though there should be pellets inside
        GameObject pellet = targetCorridor.UpdatePellet();
        if (pellet == null)
            return BTNode.Status.SUCCESS;

        Tuple<PlayerAI.Node, Stack<Vector2>> t = PlayerAI.Instance.PathfindTargetFullInfo(pellet);
        VisualizationManager.DisplayPathfindByNode(t.Item1, Color.cyan);

        if (t.Item2.Count > 0)
            _BTmoveDir = t.Item2.Peek();

        if (targetCorridor.ContainsPacMan())
            return BTNode.Status.SUCCESS;

        // by right should return RUNNING status,
        // but if is in RUNNING status,
        // all other checks are disabled (eg ghost can ambush)
        return BTNode.Status.FAILURE;
    }

    BTNode.Status BTEatPellets()
    {
        if (targetCorridor == null)
            return BTNode.Status.FAILURE;
        // successfully cleared corridor
        GameObject pellet = targetCorridor.UpdatePellet();
        if (pellet == null)
            return BTNode.Status.SUCCESS;

        //_BTmoveDir = PathfindTargetDirection(pellet);

        Tuple<PlayerAI.Node, Stack<Vector2>> t = PlayerAI.Instance.PathfindTargetFullInfo(pellet);
        VisualizationManager.DisplayPathfindByNode(t.Item1, Color.cyan);

        if (t.Item2.Count > 0)
            _BTmoveDir = t.Item2.Peek();

        // by right should return RUNNING status,
        // but if is in RUNNING status,
        // all other checks are disabled (eg ghost can ambush)
        return BTNode.Status.FAILURE;
    }

    #endregion

    #region Utility Based AI
    public void OnFinishedAction()
    {
        utilityAI.DecideBestAction(actionsAvailable);
    }

    void UpdateConsiderationValues()
    {
        // TODO:

        // calculate threat value based on ghosts position and state
        // eg.
        //ghost nearby = the nearer the higher
        //ghost scared = 0
        //ghost flashing = *0.75
        //ghost facing player = *1.5
        //ghost facing away = *0.5
        if (GhostInSight())
            threatValue = 1;
        else
            threatValue = 0;

        // powered up value
        poweredUpValue = NumOfScaredGhosts();
        if (PoweringDown())
            poweredUpValue *= 0.5f;
        poweredUpValue /= 4;
    }

    #endregion

    #region Pathfinding

    public class Node
    {
        public Tile tile;
        public float f;
        public float g;
        public float h;
        public Node parent;

        public Node(Tile tile = null, Node parent = null, float f = 0.0f, float g = 0.0f, float h = 0.0f)
        {
            this.tile = tile;
            this.parent = parent;
            this.f = f;
            this.g = g;
            this.h = h;
        }
    }

    // Sorts tuples by distance
    public static int SortByDistance(Tuple<PlayerAI.Node, Stack<Vector2>> stack1, Tuple<PlayerAI.Node, Stack<Vector2>> stack2)
    {
        return stack1.Item2.Count.CompareTo(stack2.Item2.Count);
    }

    // Returns the closest ghost
    public GameObject GetClosestGhost(bool scaredGhost = false)
    {
        GameObject closestGhost = null;
        float shortest_dist = Mathf.Infinity;
        foreach (var ghost in ghosts)
        {
            if(scaredGhost && ghost.GetComponent<GhostMove>().state != GhostMove.State.Run)
                continue;
            if (!scaredGhost && ghost.GetComponent<GhostMove>().state == GhostMove.State.Run)
                continue;

            float dist = Vector3.Distance(ghost.transform.position, pacman.transform.position);
            if (dist < shortest_dist)
            {
                closestGhost = ghost;
                shortest_dist = dist;
            }
        }

        return closestGhost;
    }

    // Returns true if ghost is in sight or nearby
    // personalSpace is the range/steps of detection
    public bool GhostInSight(int personalSpace = 8)
    {
        foreach (GameObject ghost in ghosts)
        {
            if (ghost.GetComponent<GhostMove>().state == GhostMove.State.Run)
                continue;
            Tuple<Node, Stack<Vector2>> t = PathfindTargetFullInfo(ghost);
            if (t.Item2.Count > 0 && t.Item2.Count <= personalSpace)
                return true;
        }

        return false;
    }

    // List of possible directions from current position
    public List<Vector2> PossibleDirections()
    {
        Vector3 currentPos = new Vector3(pacman.transform.position.x + 0.499f, pacman.transform.position.y + 0.499f);
        Tile currentTile = tileManager.tiles[tileManager.Index((int)currentPos.x, (int)currentPos.y)];

        List<Vector2> directions = new List<Vector2>();

        if (currentTile.up != null && !currentTile.up.occupied) directions.Add(Vector2.up);
        if (currentTile.down != null && !currentTile.down.occupied) directions.Add(Vector2.down);
        if (currentTile.left != null && !currentTile.left.occupied) directions.Add(Vector2.left);
        if (currentTile.right != null && !currentTile.right.occupied) directions.Add(Vector2.right);

        return directions;
    }

    // Pathfind from pacman position to target
    // uses A Star search
    // returns the first direction to target
    public Vector2 PathfindTargetDirection(GameObject target)
    {
        Tuple<Node, Stack<Vector2>> pf = PathfindTargetFullInfo(target);
        // if pacman reached target
        if (pf.Item2.Count == 0)
            return Vector2.zero;
        return pf.Item2.Peek();
    }

    // Pathfind from pacman position to target
    // uses A Star search
    // returns the final node and a stack of directions
    public Tuple<Node, Stack<Vector2>> PathfindTargetFullInfo(GameObject target)
    {
        Vector3 currentPos = new Vector3(pacman.transform.position.x + 0.499f, pacman.transform.position.y + 0.499f);
        Tile currentTile = tileManager.tiles[tileManager.Index((int)currentPos.x, (int)currentPos.y)];

        Vector3 targetPos = new Vector3(target.transform.position.x + 0.499f, target.transform.position.y + 0.499f);
        Tile targetTile = tileManager.tiles[tileManager.Index((int)targetPos.x, (int)targetPos.y)];


        List<Node> openList = new List<Node>();
        List<Node> closedList = new List<Node>();
        openList.Add(new Node(currentTile));

        Stack<Vector2> directions = new Stack<Vector2>();

        // for displaying the path
        Node path = null;

        while (openList.Count > 0)
        {
            Node current = openList[0];

            foreach (Node node in openList)
            {
                if (node.f < current.f)
                    current = node;
            }

            if (current.tile.Equals(targetTile))
            {
                path = current;

                // retrace path and return direction
                while (current.parent != null)
                {
                    directions.Push(current.parent.tile.GetTileDirection(current.tile));
                    current = current.parent;
                }

                // return the directions here
                return Tuple.Create(path, directions);
            }

            openList.Remove(current);
            closedList.Add(current);

            // create list of tile branches
            // ignoring walls
            List<Tile> branches = new List<Tile>();
            if (current.tile.up != null && !current.tile.up.occupied) branches.Add(current.tile.up);
            if (current.tile.down != null && !current.tile.down.occupied) branches.Add(current.tile.down);
            if (current.tile.left != null && !current.tile.left.occupied) branches.Add(current.tile.left);
            if (current.tile.right != null && !current.tile.right.occupied) branches.Add(current.tile.right);

            foreach (var child in branches)
            {
                float g_score = current.g + tileManager.distance(current.tile, child);
                float h_score = tileManager.distance(child, targetTile);

                // check if child is alrdy in the closedList
                bool isIn = false;
                foreach (Node node in closedList)
                {
                    if (node.tile.Equals(child))
                    {
                        // if current path has better f score, update values in closed list
                        if (g_score + h_score < node.f)
                        {
                            node.parent = current;
                            node.f = g_score + h_score;
                            node.g = g_score;
                            node.h = h_score;
                        }
                        isIn = true;
                        break;
                    }
                }

                // TODO: child.Value may be lost outside of scope, need double check !!
                if (!isIn)
                    openList.Add(new Node(child, current, g_score + h_score, g_score, h_score));
            }

        }

        return Tuple.Create(path, directions);
    }

    // Pathfind from start position to target with startDir
    // can only change direction at junctions
    // uses A Star search
    // returns the final node and a stack of directions
    // TODO:
    public Tuple<Node, Stack<Vector2>> GhostPathfindFullInfo(GameObject start, GameObject end, Vector3 startDir)
    {
        Vector3 currentPos = new Vector3(start.transform.position.x + 0.499f, start.transform.position.y + 0.499f);
        Tile currentTile = tileManager.tiles[tileManager.Index((int)currentPos.x, (int)currentPos.y)];

        Vector3 targetPos = new Vector3(end.transform.position.x + 0.499f, end.transform.position.y + 0.499f);
        Tile targetTile = tileManager.tiles[tileManager.Index((int)targetPos.x, (int)targetPos.y)];

        List<Node> openList = new List<Node>();
        List<Node> closedList = new List<Node>();
        openList.Add(new Node(currentTile));

        Stack<Vector2> directions = new Stack<Vector2>();

        // for displaying the path
        Node path = null;

        while (openList.Count > 0)
        {
            Node current = openList[0];

            foreach (Node node in openList)
            {
                if (node.f < current.f)
                    current = node;
            }

            if (current.tile.Equals(targetTile))
            {
                path = current;

                // retrace path and return direction
                while (current.parent != null)
                {
                    directions.Push(current.parent.tile.GetTileDirection(current.tile));
                    current = current.parent;
                }

                // return the directions here
                return Tuple.Create(path, directions);
            }

            openList.Remove(current);
            closedList.Add(current);

            // check if current tile is not an intersection
            if (!current.tile.isIntersection)
            {
                // get prev dir
                Vector2 dir;
                if (current.parent != null)
                    dir = current.parent.tile.GetTileDirection(current.tile);
                else
                    dir = (Vector2)startDir;

                // move in prev dir along corridor

            }

            // create list of tile branches
            // ignoring walls
            List<Tile> branches = new List<Tile>();
            if (current.tile.up != null && !current.tile.up.occupied) branches.Add(current.tile.up);
            if (current.tile.down != null && !current.tile.down.occupied) branches.Add(current.tile.down);
            if (current.tile.left != null && !current.tile.left.occupied) branches.Add(current.tile.left);
            if (current.tile.right != null && !current.tile.right.occupied) branches.Add(current.tile.right);

            foreach (var child in branches)
            {
                float g_score = current.g + tileManager.distance(current.tile, child);
                float h_score = tileManager.distance(child, targetTile);

                // check if child is alrdy in the closedList
                bool isIn = false;
                foreach (Node node in closedList)
                {
                    if (node.tile.Equals(child))
                    {
                        // if current path has better f score, update values in closed list
                        if (g_score + h_score < node.f)
                        {
                            node.parent = current;
                            node.f = g_score + h_score;
                            node.g = g_score;
                            node.h = h_score;
                        }
                        isIn = true;
                        break;
                    }
                }

                if (!isIn)
                    openList.Add(new Node(child, current, g_score + h_score, g_score, h_score));
            }

        }

        return Tuple.Create(path, directions);
    }

    // for debugging
    public void DebugPathfinding(Stack<Vector2> directions)
    {
        Debug.Log("Steps");

        while (directions.Count > 0)
        {
            Debug.Log(" |\tDirection:\t" + directions.Pop().ToString());
        }
    }

    #endregion

}

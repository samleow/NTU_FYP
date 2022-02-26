using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TileManager;

public class PlayerAI : MonoBehaviour
{

    public GameObject pacman;
    public GameObject[] ghosts;
    public TileManager tileManager;

    public bool activated;

    // FSM state
    private PlayerState currentState;

    public enum AI_MODE { FSM, BT }

    public AI_MODE aiMode = AI_MODE.FSM;

    // Behaviour Tree
    BehaviourTree tree;
    BTNode.Status treeStatus = BTNode.Status.RUNNING;

    Vector2 _BTmoveDir = Vector2.zero;

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

    #endregion

    void Start()
    {
        activated = false;
        // FSM init
        currentState = new SeekPelletsState();
        // BT init
        tree = new BehaviourTree();

        BTSequence chaseGhosts = new BTSequence("Chase ghosts");
        BTSequence runFromGhosts = new BTSequence("Run from ghosts");
        BTSequence seekPellets = new BTSequence("Seek pellets");

        BTLeaf isPoweredUp = new BTLeaf("Is powered up?", BTIsPoweredUp);
        BTInverter inverter1 = new BTInverter("Inverter 1");
        BTLeaf isPoweringDown = new BTLeaf("Is powering down?", BTIsPoweringDown);
        BTLeaf pursueGhosts = new BTLeaf("Pursue ghosts", BTPursueGhosts);

        inverter1.AddChild(isPoweringDown);
        chaseGhosts.AddChild(isPoweredUp);
        chaseGhosts.AddChild(inverter1);
        chaseGhosts.AddChild(pursueGhosts);

        BTLeaf isGhostsNearby = new BTLeaf("Is ghosts nearby?", BTIsGhostsNearby);
        BTLeaf run = new BTLeaf("Run", BTRun);

        runFromGhosts.AddChild(isGhostsNearby);
        runFromGhosts.AddChild(run);

        BTLeaf scanForBestCorridor = new BTLeaf("Scan for best corridor", BTScanForBestCorridor);
        BTLeaf moveToCorridor = new BTLeaf("Move to corridor", BTMoveToCorridor);
        BTLeaf eatPellets = new BTLeaf("Eat pellets", BTEatPellets);

        seekPellets.AddChild(scanForBestCorridor);
        seekPellets.AddChild(moveToCorridor);
        seekPellets.AddChild(eatPellets);

        tree.AddChild(chaseGhosts);
        tree.AddChild(runFromGhosts);
        tree.AddChild(seekPellets);

        tree.PrintTree();

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
        else
        {
            Debug.LogError("AI Mode undefined!");
        }
    }

    public Vector2 GetAIDirection()
    {
        if (aiMode == AI_MODE.FSM)
        {
            return currentState.GetDirection();
        }
        else if (aiMode == AI_MODE.BT)
        {
            return _BTmoveDir;
        }
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

    BTNode.Status BTScanForBestCorridor()
    {

        return BTNode.Status.SUCCESS;
    }

    BTNode.Status BTMoveToCorridor()
    {

        return BTNode.Status.SUCCESS;
    }

    BTNode.Status BTEatPellets()
    {

        return BTNode.Status.SUCCESS;
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

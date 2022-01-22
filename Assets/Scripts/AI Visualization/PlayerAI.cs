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

    private PlayerState currentState;

    public enum AI_MODE { FSM, BT }

    public AI_MODE aiMode = AI_MODE.FSM;

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
        currentState = new SeekPelletsState();
        activated = false;
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

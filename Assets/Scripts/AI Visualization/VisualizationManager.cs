using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class VisualizationManager : MonoBehaviour
{
    private PlayerController PC;

    public Text AIModeText;
    public Text AIStateText;
    public Toggle toggleAI;
    public Slider toggleAIMode;
    public Slider speedSlider;

    public RectTransform fsmDiagram;
    public RectTransform btDiagram;
    public RectTransform utilDiagram;
    public Sprite nodeSprite;
    public Font nodeFont;
    public GameObject btRoot;

    private bool fsmGenerated = false;
    private bool utilGenerated = false;
    // not used as for now, BT is "hardcoded" in Editor
    //private bool btGenerated = false;

    // dictionaries of generated nodes
    private Dictionary<string, Image> fsmNodes = new Dictionary<string, Image>();
    private Dictionary<string, Image> btNodes = new Dictionary<string, Image>();
    private List<Link> fsmLinks = new List<Link>();
    private Image fsmPrevNode;

    public static Material lineMaterial;

    // Start is called before the first frame update
    void Start()
    {
        PC = GameObject.Find("pacman").GetComponent<PlayerController>();

        lineMaterial = Resources.Load("LineMaterial", typeof(Material)) as Material;

        ChangeSpeed();
        // player in control at start
        toggleAI.isOn = false;
        toggleAIMode.interactable = false;
        PlayerAI.Instance.activated = false;
    }


    void FixedUpdate()
    {
        if (PC.playerControl)
        {
            AIModeText.text = "-";
            AIStateText.text = "-";
            return;
        }

        if (PlayerAI.Instance.aiMode == PlayerAI.AI_MODE.FSM)
        {
            AIModeText.text = "Finite State Machine";
            AIStateText.text = PlayerAI.Instance.GetCurrentFSMState();
            DisplayFSM();
        }
        else if (PlayerAI.Instance.aiMode == PlayerAI.AI_MODE.BT)
        {
            AIModeText.text = "Behavior Tree";
            if (PlayerAI.Instance.tree.currentLeaf != null && PlayerAI.Instance.tree.currentLeaf.coreProcess)
                AIStateText.text = PlayerAI.Instance.tree.currentLeaf.name;
            DisplayBT();
        }
        else if (PlayerAI.Instance.aiMode == PlayerAI.AI_MODE.UTIL)
        {
            AIModeText.text = "Utility-based AI";
            AIStateText.text = PlayerAI.Instance.utilityAI.bestAction.Name;
            DisplayUtilAI();
        }
        else
            Debug.LogError("AI Mode undefined!");
    }

    #region Buttons

    // Toggle between player manual control or AI control
    public void TogglePlayerControl()
    {
        if (!PC.playerControl)
        {
            toggleAI.isOn = false;
            toggleAIMode.interactable = false;
            PlayerAI.Instance.activated = false;
            PC.playerControl = true;
        }
        else
        {
            toggleAI.isOn = true;
            toggleAIMode.interactable = true;
            PlayerAI.Instance.activated = true;
            PC.playerControl = false;
        }
    }

    // Toggle between different AI modes
    // FSM or BT
    public void ToggleAIModes()
    {
        if (toggleAIMode.value == 0)
            PlayerAI.Instance.aiMode = PlayerAI.AI_MODE.FSM;
        else if (toggleAIMode.value == 1)
            PlayerAI.Instance.aiMode = PlayerAI.AI_MODE.BT;
        else if (toggleAIMode.value == 2)
            PlayerAI.Instance.aiMode = PlayerAI.AI_MODE.UTIL;
        else
            Debug.LogError("Slider error!");
    }

    public void ChangeSpeed()
    {
        switch (speedSlider.value)
        {
            case 0:
                Time.timeScale = 0f;
                break;
            case 1:
                Time.timeScale = 0.5f;
                break;
            case 2:
                Time.timeScale = 1f;
                break;
            case 3:
                Time.timeScale = 2f;
                break;
            case 4:
                Time.timeScale = 3f;
                break;
            default:
                Time.timeScale = 1f;
                break;
        }
    }

    #endregion

    #region Path Rendering

    public static void DisplayPathfindByNode(PlayerAI.Node node, Color color)
    {
        if (node == null)
            return;
        
        //PlayerAI.Node cursor = node;
        while (node.parent != null)
        {
            DrawLine(node.tile.GetTilePosition(), node.parent.tile.GetTilePosition(), color);
            node = node.parent;
        }
    }

    public static LineRenderer DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.1f, Transform parent = null)
    {
        GameObject myLine = new GameObject();
        if (parent)
            myLine.transform.SetParent(parent);
        myLine.transform.position = start;
        LineRenderer lr = myLine.AddComponent<LineRenderer>();
        lr.sortingOrder = 2;
        lr.material = lineMaterial;
        lr.startColor = lr.endColor = color;
        lr.startWidth = lr.endWidth = 0.1f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        if(duration >= 0f)
            GameObject.Destroy(myLine, duration);
        return lr;
    }

    #endregion

    #region Diagrams Visualization

    class Link
    {
        public LinkRenderer linkRenderer;
        public List<Image> nodes = new List<Image>();
        public Link() { }
        public Link(LinkRenderer linkRenderer) { this.linkRenderer = linkRenderer; }
    }

    void DisplayFSM()
    {
        // if diagram not generated yet
        //      generate diagram
        if (!fsmGenerated)
        {
            // List of positions
            // Need to find some way to auto generate spaced out positions
            Stack<Vector2> pos = new Stack<Vector2>();
            pos.Push(new Vector2(130, 100));
            pos.Push(new Vector2(400, 50));
            pos.Push(new Vector2(300, 250));

            // generate nodes
            string[] states = System.Enum.GetNames(typeof(PlayerState.STATE));
            foreach (var state in states)
            {
                fsmNodes.Add(state, CreateNode(state, fsmDiagram, pos.Pop(), new Vector2(120, 40)));
            }

            // generate links/lines between nodes
            Image[] nodesArray = fsmNodes.Values.ToArray<Image>();
            Link link = new Link(DrawLink(nodesArray[0].GetComponent<RectTransform>().position,
                nodesArray[nodesArray.Length - 1].GetComponent<RectTransform>().position,
                fsmDiagram));
            link.nodes.Add(nodesArray[0]);
            link.nodes.Add(nodesArray[nodesArray.Length - 1]);
            fsmLinks.Add(link);
            for (int i = 1; i < nodesArray.Length; i++)
            {
                link = new Link(DrawLink(nodesArray[i - 1].GetComponent<RectTransform>().position,
                    nodesArray[i].GetComponent<RectTransform>().position,
                    fsmDiagram));
                link.nodes.Add(nodesArray[i - 1]);
                link.nodes.Add(nodesArray[i]);
                fsmLinks.Add(link);
            }

            fsmGenerated = true;
        }

        // if diagram not displayed yet
        //      display diagram
        if (!fsmDiagram.gameObject.activeInHierarchy)
        {
            fsmDiagram.gameObject.SetActive(true);
            btDiagram.gameObject.SetActive(false);
            utilDiagram.gameObject.SetActive(false);
        }

        // update highlighted node
        //      based on current state
        foreach (var node in fsmNodes)
        {

            if (node.Value.color == Color.green)
                fsmPrevNode = node.Value;

            if (node.Key == AIStateText.text)
                node.Value.color = Color.green;
            else
                node.Value.color = Color.white;
        }

        foreach (var link in fsmLinks)
        {
            bool nodeHighlighted = false;
            foreach (var node in link.nodes)
            {
                if (node.color == Color.green)
                {
                    if (nodeHighlighted)
                    {
                        StartCoroutine(HighlightLink(link.linkRenderer));
                    }
                    nodeHighlighted = true;
                }
                else if (node == fsmPrevNode)
                {
                    if (nodeHighlighted)
                    {
                        StartCoroutine(HighlightLink(link.linkRenderer));
                    }
                    nodeHighlighted = true;
                }
            }

        }
    }

    void DisplayBT()
    {
        // if diagram not generated yet
        //      generate diagram
        //  BTree abit hard to generate
        //  will "hardcode" in Unity editor first

        // if diagram not displayed yet
        //      display diagram
        if (!btDiagram.gameObject.activeInHierarchy)
        {
            btDiagram.gameObject.SetActive(true);
            fsmDiagram.gameObject.SetActive(false);
            utilDiagram.gameObject.SetActive(false);
        }

        // update highlighted node
        //      based on current state
        Image[] nodes = btRoot.GetComponentsInChildren<Image>();
        foreach (var n in nodes)
        {
            n.color = Color.white;
        }

        if (PlayerAI.Instance.tree.currentLeaf == null)
            return;

        Image leafNode = nodes.FirstOrDefault(c => c.gameObject.name == PlayerAI.Instance.tree.currentLeaf.name);

        if (leafNode)
        {
            HighlightBT(leafNode.GetComponent<Image>());
        }
    }

    void DisplayUtilAI()
    {
        // if diagram not displayed yet
        //      display diagram
        if (!utilDiagram.gameObject.activeInHierarchy)
        {
            utilDiagram.gameObject.SetActive(true);
            fsmDiagram.gameObject.SetActive(false);
            btDiagram.gameObject.SetActive(false);
        }

        // plot graphs
        if (!utilGenerated)
        {
            utilDiagram.GetComponent<UtilityVisualization>().PlotGraphs();
            utilGenerated = true;
        }

        utilDiagram.GetComponent<UtilityVisualization>().HighlightGraph();
    }

    void HighlightBT(Image node)
    {
        node.color = Color.green;

        if (node.gameObject != btRoot)
        {
            HighlightBT(node.gameObject.transform.parent.GetComponent<Image>());
        }
    }

    IEnumerator HighlightLink(LinkRenderer lr)
    {
        lr.color = Color.green;
        yield return new WaitForSeconds(0.2f);
        lr.color = Color.white;
    }

    LinkRenderer DrawLink(Vector2 start, Vector2 end, Transform parent)
    {
        GameObject go = new GameObject("link", typeof(LinkRenderer));
        go.AddComponent<CanvasRenderer>();
        go.transform.SetParent(parent.Find("links"));
        LinkRenderer lr = go.GetComponent<LinkRenderer>();
        lr.color = Color.white;
        RectTransform rtransform = go.GetComponent<RectTransform>();
        rtransform.position = start;
        lr.end = end;
        lr.thickness = 3f;
        return go.GetComponent<LinkRenderer>();
    }

    // Bounds of diagram space (550, 300)
    Image CreateNode(string name, RectTransform parent, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        // Node body and image
        GameObject go = new GameObject(name,typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().sprite = nodeSprite;
        go.GetComponent<Image>().type = Image.Type.Sliced;
        RectTransform rtransform = go.GetComponent<RectTransform>();
        rtransform.anchoredPosition = anchoredPosition;
        rtransform.sizeDelta = sizeDelta;
        rtransform.anchorMin = Vector2.zero;
        rtransform.anchorMax = Vector2.zero;

        // Node text
        GameObject goText = new GameObject(name + " text", typeof(Text));
        goText.transform.SetParent(go.transform, false);
        goText.GetComponent<Text>().text = name;
        goText.GetComponent<Text>().font = nodeFont;
        goText.GetComponent<Text>().fontSize = 10;
        goText.GetComponent<Text>().color = Color.black;
        goText.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        RectTransform rtransformText = goText.GetComponent<RectTransform>();
        rtransformText.anchorMin = Vector2.zero;
        rtransformText.anchorMax = Vector2.one;
        rtransformText.offsetMin = Vector2.zero;
        rtransformText.offsetMax = Vector2.zero;

        return go.GetComponent<Image>();
    }

    #endregion

}

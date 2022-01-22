using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualizationManager : MonoBehaviour
{
    private PlayerController PC;

    public Text AIModeText;
    public Text AIStateText;
    public Toggle toggleAI;
    public Slider toggleAIMode;
    public Slider speedSlider;

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

    // Update is called once per frame
    void Update()
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
        }
        else if (PlayerAI.Instance.aiMode == PlayerAI.AI_MODE.BT)
        {
            AIModeText.text = "Behavior Tree";
            AIStateText.text = "-";
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
        if (toggleAIMode.value == 1)
            PlayerAI.Instance.aiMode = PlayerAI.AI_MODE.BT;
        else if (toggleAIMode.value == 0)
            PlayerAI.Instance.aiMode = PlayerAI.AI_MODE.FSM;
        else
            Debug.LogError("Slider error!");
    }

    public void ChangeSpeed()
    {
        switch (speedSlider.value)
        {
            case 0:
                Time.timeScale = 0.5f;
                break;
            case 1:
                Time.timeScale = 1f;
                break;
            case 2:
                Time.timeScale = 2f;
                break;
            default:
                Time.timeScale = 1f;
                break;
        }
    }

    #endregion

    public static void DisplayPathfindByNode(PlayerAI.Node node, Color color)
    {
        //PlayerAI.Node cursor = node;
        while (node.parent != null)
        {
            //Debug.DrawLine(node.tile.GetTilePosition(), node.parent.tile.GetTilePosition(), color);
            DrawLine(node.tile.GetTilePosition(), node.parent.tile.GetTilePosition(), color);
            node = node.parent;
        }
    }

    public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.1f)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        LineRenderer lr = myLine.AddComponent<LineRenderer>();
        lr.sortingOrder = 2;
        //lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        lr.material = lineMaterial;
        lr.startColor = lr.endColor = color;
        lr.startWidth = lr.endWidth = 0.1f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        GameObject.Destroy(myLine, duration);
    }

}

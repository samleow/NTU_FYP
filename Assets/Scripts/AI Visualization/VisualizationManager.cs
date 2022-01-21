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

    // Start is called before the first frame update
    void Start()
    {
        PC = GameObject.Find("pacman").GetComponent<PlayerController>();
        
        // player in control at start
        toggleAI.isOn = false;
        toggleAIMode.interactable = false;
        PlayerAI.Instance.enabled = false;
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
            PlayerAI.Instance.enabled = false;
            PC.playerControl = true;
        }
        else
        {
            toggleAI.isOn = true;
            toggleAIMode.interactable = true;
            PlayerAI.Instance.enabled = true;
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

#endregion

}

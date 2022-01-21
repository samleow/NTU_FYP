using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAI : MonoBehaviour
{

    public GameObject pacman;
    public GameObject blinky;
    public GameObject pinky;
    public GameObject inky;
    public GameObject clyde;

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

    // Start is called before the first frame update
    void Start()
    {
        currentState = new SeekPelletsState();
    }

    void Update()
    {
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
            return currentState.GetDirection();
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

}

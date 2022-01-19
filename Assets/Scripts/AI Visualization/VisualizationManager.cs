using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizationManager : MonoBehaviour
{
    private PlayerController PC;

    // Start is called before the first frame update
    void Start()
    {
        PC = GameObject.Find("pacman").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


#region Buttons

    public void TogglePlayerControl()
    {
        PC.playerControl = !PC.playerControl;
    }

#endregion

}

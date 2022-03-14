using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[CreateAssetMenu(fileName = "PelletSeeking", menuName = "UtilityAI/Actions/PelletSeeking")]
public class PelletSeeking : Action
{
    public override void Execute(PlayerAI playerAI)
    {
        // move towards nearest pellet
        GameObject[] pellets = GameObject.FindGameObjectsWithTag("pacdot");
        GameObject closestPellet = pellets.OrderBy(t => (t.transform.position - playerAI.pacman.transform.position).sqrMagnitude)
                           .FirstOrDefault();

        Tuple<PlayerAI.Node, Stack<Vector2>> t = PlayerAI.Instance.PathfindTargetFullInfo(closestPellet);
        VisualizationManager.DisplayPathfindByNode(t.Item1, Color.cyan);

        if (t.Item2.Count > 0)
            playerAI.utilAImoveDir = t.Item2.Peek();

        playerAI.OnFinishedAction();
    }
}

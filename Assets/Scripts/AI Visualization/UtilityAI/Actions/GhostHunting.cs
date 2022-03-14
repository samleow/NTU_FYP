using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GhostHunting", menuName = "UtilityAI/Actions/GhostHunting")]
public class GhostHunting : Action
{
    public override void Execute(PlayerAI playerAI)
    {
        Tuple<PlayerAI.Node, Stack<Vector2>> t = PlayerAI.Instance.PathfindTargetFullInfo(playerAI.GetClosestGhost(true));
        VisualizationManager.DisplayPathfindByNode(t.Item1, Color.green);

        if (t.Item2.Count > 0)
            playerAI.utilAImoveDir = t.Item2.Peek();

        playerAI.OnFinishedAction();
    }
}

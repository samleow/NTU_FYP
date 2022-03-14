using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GhostEvading", menuName = "UtilityAI/Actions/GhostEvading")]
public class GhostEvading : Action
{
    public override void Execute(PlayerAI playerAI)
    {
        List<Tuple<PlayerAI.Node, Stack<Vector2>>> dangerPaths = new List<Tuple<PlayerAI.Node, Stack<Vector2>>>();

        foreach (var ghost in playerAI.ghosts)
            dangerPaths.Add(PlayerAI.Instance.PathfindTargetFullInfo(ghost));

        dangerPaths.Sort(PlayerAI.SortByDistance);

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

        foreach (var direction in PlayerAI.Instance.PossibleDirections())
        {
            if (!toAvoid.Contains(direction))
            {
                playerAI.utilAImoveDir = direction;
                playerAI.OnFinishedAction();
                return;
            }
            else
                continue;
        }

        playerAI.utilAImoveDir = toAvoid[toAvoid.Count - 1];

        playerAI.OnFinishedAction();
    }
}

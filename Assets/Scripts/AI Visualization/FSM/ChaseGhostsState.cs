using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseGhostsState : PlayerState
{
    public ChaseGhostsState() : base()
    {
        name = STATE.CHASE_GHOSTS;
    }

    #region Events

    public override void Enter()
    {

        base.Enter();
    }

    public override void Update()
    {
        if (GhostsFlashing() || PlayerAI.Instance.GhostInSight())
        {
            nextState = new EvadeGhostsState();
            stage = EVENT.EXIT;
        }
        else
        {
            // chase ghost
            Tuple<PlayerAI.Node, Stack<Vector2>> t = PlayerAI.Instance.PathfindTargetFullInfo(PlayerAI.Instance.GetClosestGhost(true));
            VisualizationManager.DisplayPathfindByNode(t.Item1, Color.green);

            if (t.Item2.Count > 0)
                moveDirection = t.Item2.Peek();

            //base.Update();
        }
    }

    public override void Exit()
    {


        base.Exit();
    }

    #endregion

    // boost is running out
    private bool GhostsFlashing()
    {
        return PlayerAI.Instance.PoweringDown();
    }

}

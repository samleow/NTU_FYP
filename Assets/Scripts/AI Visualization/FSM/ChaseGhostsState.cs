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
        if (GhostsFlashing())
        {
            nextState = new EvadeGhostsState();
            stage = EVENT.EXIT;
        }

        //base.Update();
    }

    public override void Exit()
    {


        base.Exit();
    }

    #endregion

    // boost is running out
    private bool GhostsFlashing()
    {
        return true;
    }

}

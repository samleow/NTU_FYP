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
        if (PoweringDown())
        {
            if (GettingAmbushed())
            {
                nextState = new EvadeGhostsState();
                stage = EVENT.EXIT;
            }
            else
            {
                nextState = new SeekPelletsState();
                stage = EVENT.EXIT;
            }
        }

        base.Update();
    }

    public override void Exit()
    {


        base.Exit();
    }

    #endregion

    // boost is running out
    private bool PoweringDown()
    {
        return true;
    }

    // if 3 or more ghosts are nearby
    private bool GettingAmbushed()
    {
        return true;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvadeGhostsState : PlayerState
{
    public EvadeGhostsState() : base()
    {
        name = STATE.EVADE_GHOSTS;
    }

    #region Events

    public override void Enter()
    {


        base.Enter();
    }

    public override void Update()
    {
        if (PowerPillEaten())
        {
            nextState = new ChaseGhostsState();
            stage = EVENT.EXIT;
        }
        else if (NoVisibleGhost())
        {
            nextState = new SeekPelletsState();
            stage = EVENT.EXIT;
        }

        //base.Update();
    }

    public override void Exit()
    {


        base.Exit();
    }

    #endregion

    // if pacman ate a power pill
    private bool PowerPillEaten()
    {
        return true;
    }

    // if less than 3 ghosts are nearby
    private bool NoVisibleGhost()
    {
        return true;
    }
}

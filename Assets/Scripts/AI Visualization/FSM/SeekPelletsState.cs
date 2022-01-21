using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekPelletsState : PlayerState
{
    public SeekPelletsState() : base()
    {
        name = STATE.SEEK_PELLETS;
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
        else if (GhostInSight())
        {
            nextState = new EvadeGhostsState();
            stage = EVENT.EXIT;
        }
        else
        {


            base.Update();
        }

    }

    public override void Exit()
    {

        base.Exit();
    }

    #endregion

    // if pacman ate a power pill
    private bool PowerPillEaten()
    {
        return false;
    }

    // if 3 or more ghosts are nearby
    private bool GhostInSight()
    {
        return false;
    }

}

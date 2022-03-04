using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState
{
    public enum STATE { SEEK_PELLETS, CHASE_GHOSTS, EVADE_GHOSTS }

    public enum EVENT { ENTER, UPDATE, EXIT }

    public STATE name;

    protected EVENT stage;

    protected PlayerState nextState;

    protected Vector2 moveDirection;

    public PlayerState()
    {
        name = STATE.SEEK_PELLETS;
        stage = EVENT.ENTER;
        nextState = null;
        moveDirection = Vector2.zero;
    }

    // virtual methods
    public virtual void Enter() { stage = EVENT.UPDATE; }
    public virtual void Update() { stage = EVENT.UPDATE; }
    public virtual void Exit() { stage = EVENT.EXIT; }

    public PlayerState Process()
    {
        if (stage == EVENT.ENTER) Enter();
        if (stage == EVENT.UPDATE) Update();
        if (stage == EVENT.EXIT)
        {
            Exit();
            return nextState;
        }

        return this;
    }

    public Vector2 GetDirection()
    {
        return moveDirection;
    }
}

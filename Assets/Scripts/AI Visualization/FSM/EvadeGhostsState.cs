using System;
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
        else
        {
            List<Stack<Vector2>> dangerPaths = new List<Stack<Vector2>>();

            foreach (var ghost in PlayerAI.Instance.ghosts)
                dangerPaths.Add(PlayerAI.Instance.PathfindTargetFullInfo(ghost).Item2);

            dangerPaths.Sort(SortByDistance);

            List<Vector2> toAvoid = new List<Vector2>();
            foreach (var path in dangerPaths)
            {
                // ghost still in cage
                if (path.Count == 0)
                    continue;

                if (!toAvoid.Contains(path.Peek()))
                    toAvoid.Add(path.Peek());
            }

            foreach (var direction in PlayerAI.Instance.PossibleDirections())
            {
                if (!toAvoid.Contains(direction))
                {
                    moveDirection = direction;
                    return;
                }
                else
                    continue;
            }

            moveDirection = toAvoid[toAvoid.Count - 1];

            //base.Update();
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
        return GameManager.scared;
    }

    // if less than 2 ghosts are nearby
    private bool NoVisibleGhost()
    {
        int ghostsNearby = 0;
        float proximityRadius = 10f;

        foreach (GameObject ghost in PlayerAI.Instance.ghosts)
        {
            if (Vector3.Distance(ghost.transform.position, PlayerAI.Instance.pacman.transform.position) <= proximityRadius)
                ghostsNearby++;
        }

        return ghostsNearby < 2;
    }

    static int SortByDistance(Stack<Vector2> stack1, Stack<Vector2> stack2)
    {
        return stack1.Count.CompareTo(stack2.Count);
    }

}

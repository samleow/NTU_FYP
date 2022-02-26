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
        if (PowerPillEaten() && !GhostsFlashing())
        {
            nextState = new ChaseGhostsState();
            stage = EVENT.EXIT;
        }
        else if (!PlayerAI.Instance.GhostInSight())
        {
            nextState = new SeekPelletsState();
            stage = EVENT.EXIT;
        }
        else
        {
            List<Tuple<PlayerAI.Node, Stack<Vector2>>> dangerPaths = new List<Tuple<PlayerAI.Node, Stack<Vector2>>>();

            foreach (var ghost in PlayerAI.Instance.ghosts)
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
    // may be bugged cause some ghosts may have respawned without being scared
    // will keep alternating between chase ghost and evade ghost state
    private bool PowerPillEaten()
    {
        return GameManager.scared;
    }

    // boost is running out
    private bool GhostsFlashing()
    {
        return PlayerAI.Instance.PoweringDown();
    }

}

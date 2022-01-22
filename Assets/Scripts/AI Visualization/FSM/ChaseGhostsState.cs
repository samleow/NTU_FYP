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
        if (GhostsFlashing() || GhostInSight())
        {
            nextState = new EvadeGhostsState();
            stage = EVENT.EXIT;
        }
        else
        {
            // chase ghost
            Tuple<PlayerAI.Node, Stack<Vector2>> t = PlayerAI.Instance.PathfindTargetFullInfo(GetClosestGhost());
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
        // might need to use a timer
        return !GameManager.scared;
    }

    private bool GhostInSight()
    {
        int personalSpace = 8;

        foreach (GameObject ghost in PlayerAI.Instance.ghosts)
        {
            if (ghost.GetComponent<GhostMove>().state == GhostMove.State.Run)
                continue;
            Tuple<PlayerAI.Node, Stack<Vector2>> t = PlayerAI.Instance.PathfindTargetFullInfo(ghost);
            if (t.Item2.Count > 0 && t.Item2.Count <= personalSpace)
                return true;
        }

        return false;
    }

    // might have a problem when ghost has respawned after being eaten
    // pacman will chase ghost when ghost is not scared
    GameObject GetClosestGhost()
    {
        GameObject closestGhost = null;
        float shortest_dist = Mathf.Infinity;
        foreach (var ghost in PlayerAI.Instance.ghosts)
        {
            if (ghost.GetComponent<GhostMove>().state != GhostMove.State.Run)
                continue;
            float dist = Vector3.Distance(ghost.transform.position, PlayerAI.Instance.pacman.transform.position);
            if (dist < shortest_dist)
            {
                closestGhost = ghost;
                shortest_dist = dist;
            }
        }

        return closestGhost;
    }

}

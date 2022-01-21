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
        else
        {
            // chase ghost
            moveDirection = PlayerAI.Instance.PathfindTargetDirection(GetClosestGhost());


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

    // might have a problem when ghost has respawned after being eaten
    // pacman will chase ghost when ghost is not scared
    GameObject GetClosestGhost()
    {
        GameObject closestGhost = null;
        float shortest_dist = Mathf.Infinity;
        foreach (var ghost in PlayerAI.Instance.ghosts)
        {
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

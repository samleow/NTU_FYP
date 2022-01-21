using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekPelletsState : PlayerState
{
    GameObject[] pellets;
    GameObject closestPellet;

    public SeekPelletsState() : base()
    {
        name = STATE.SEEK_PELLETS;
        pellets = null;
        closestPellet = null;
    }

    GameObject GetClosestPellet()
    {
        float closestDist = Mathf.Infinity;
        foreach (var pellet in pellets)
        {
            if (pellet == null)
                continue;

            float distance = Vector3.Distance(pellet.transform.position, PlayerAI.Instance.pacman.transform.position);
            if (distance < closestDist)
            {
                closestPellet = pellet;
                closestDist = distance;
            }
        }

        return closestPellet;
    }

    #region Events

    public override void Enter()
    {
        pellets = GameObject.FindGameObjectsWithTag("pacdot");

        GetClosestPellet();

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
            // if closest pellet is null (deleted), re-search
            if (closestPellet == null)
                GetClosestPellet();

            // chase after closest pellet
            // sets moveDirection
            moveDirection = PlayerAI.Instance.PathfindTargetDirection(closestPellet);

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

    // if 2 or more ghosts are nearby
    private bool GhostInSight()
    {
        int ghostsNearby = 0;
        float proximityRadius = 10f;

        foreach (GameObject ghost in PlayerAI.Instance.ghosts)
        {
            if (Vector3.Distance(ghost.transform.position, PlayerAI.Instance.pacman.transform.position) <= proximityRadius)
                ghostsNearby++;
        }

        return ghostsNearby >= 2;
    }

}

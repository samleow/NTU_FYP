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

            Tuple<PlayerAI.Node, Stack<Vector2>> t = PlayerAI.Instance.PathfindTargetFullInfo(closestPellet);
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

    // if pacman ate a power pill
    // may be bugged cause some ghosts may have respawned without being scared
    private bool PowerPillEaten()
    {
        return GameManager.scared;
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

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corridor : MonoBehaviour
{
    public int pellet_count = 0;
    public int ghost_count = 0;
    public GameObject pellet = null;

    private void Start()
    {
        Collider2D[] colliders = new Collider2D[15];
        Collider2D collider = gameObject.GetComponent<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        /*LayerMask layerMask = LayerMask.GetMask("pacdot");
        filter.SetLayerMask(layerMask);
        pellet_count = collider.OverlapCollider(filter, colliders);*/

        // for some reason, layer mask filtering does not work
        collider.OverlapCollider(filter.NoFilter(), colliders);

        foreach (Collider2D col in colliders)
        {
            if (col == null)
                break;
            else if (col.CompareTag("pacdot"))
            {
                pellet_count++;
                if (pellet == null)
                    pellet = col.gameObject;
            }
        }

    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("ghost"))
        {
            ghost_count++;
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("pacdot"))
        {
            pellet_count--;
            if (pellet == null)
            {
                UpdatePellet();
            }
        }
        else if (col.CompareTag("ghost"))
        {
            ghost_count--;
        }
    }

    public GameObject UpdatePellet()
    {
        Collider2D[] colliders = new Collider2D[15];
        Collider2D collider = gameObject.GetComponent<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();

        collider.OverlapCollider(filter.NoFilter(), colliders);

        foreach (Collider2D c in colliders)
        {
            if (c == null)
                break;
            else if (c.CompareTag("pacdot"))
            {
                if (pellet == null)
                {
                    pellet = c.gameObject;
                    break;
                }
            }
        }

        return pellet;
    }

    public bool ContainsPacMan()
    {
        Collider2D[] colliders = new Collider2D[15];
        Collider2D collider = gameObject.GetComponent<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();

        collider.OverlapCollider(filter.NoFilter(), colliders);

        foreach (Collider2D c in colliders)
        {
            if (c == null)
                break;
            if (c.name == "pacman")
            {
                return true;
            }
        }

        return false;
    }

}

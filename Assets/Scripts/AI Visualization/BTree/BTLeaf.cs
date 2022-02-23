using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTLeaf : BTNode
{
    public delegate Status Tick();
    public Tick ProcessMethod;

    public BTLeaf() { }
    public BTLeaf(string n, Tick pm)
    {
        name = n;
        ProcessMethod = pm;
    }

    public override Status Process()
    {
        // debug
        Debug.Log("Leaf processing:\t" + name);

        if (ProcessMethod != null)
            return ProcessMethod();
        return Status.FAILURE;
    }
}

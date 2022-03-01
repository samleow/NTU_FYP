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
        currentLeaf = name;
        ProcessMethod = pm;
    }

    public override Status Process()
    {
        if (ProcessMethod != null)
            return ProcessMethod();
        return Status.FAILURE;
    }
}

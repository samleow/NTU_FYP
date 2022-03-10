using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTLeaf : BTNode
{
    public delegate Status Tick();
    public Tick ProcessMethod;
    public bool coreProcess = false;

    public BTLeaf() { }
    public BTLeaf(string n, Tick pm, bool coreProcess = false)
    {
        name = n;
        currentLeaf = this;
        ProcessMethod = pm;
        this.coreProcess = coreProcess;
    }

    public override Status Process()
    {
        if (ProcessMethod != null)
            return ProcessMethod();
        return Status.FAILURE;
    }
}

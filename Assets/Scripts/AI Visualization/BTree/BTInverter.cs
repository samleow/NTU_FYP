using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTInverter : BTNode
{

    public BTInverter(string name)
    {
        this.name = name;
    }

    public override Status Process()
    {
        Status childStatus = children[currentChild].Process();
        currentLeaf = children[currentChild].currentLeaf;
        if (childStatus == Status.SUCCESS)
        {
            return Status.FAILURE;
        }
        else if (childStatus == Status.FAILURE)
        {
            return Status.SUCCESS;
        }
        else
            return Status.RUNNING;
    }
}

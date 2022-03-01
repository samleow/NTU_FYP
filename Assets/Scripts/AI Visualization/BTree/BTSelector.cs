using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// priority selector
public class BTSelector : BTNode
{
    public BTSelector(string name)
    {
        this.name = name;
    }

    public override Status Process()
    {
        Status childStatus = children[currentChild].Process();
        currentLeaf = children[currentChild].currentLeaf;
        if (childStatus == Status.RUNNING) return Status.RUNNING;
        if (childStatus == Status.SUCCESS)
        {
            currentChild = 0;
            return Status.SUCCESS;
        }

        currentChild++;
        if (currentChild >= children.Count)
        {
            currentChild = 0;
            return Status.FAILURE;
        }

        return Status.RUNNING;
    }

}

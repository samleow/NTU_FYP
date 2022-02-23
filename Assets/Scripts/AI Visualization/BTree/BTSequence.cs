using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTSequence : BTNode
{
    public BTSequence(string name)
    {
        this.name = name;
    }

    public override Status Process()
    {
        // debug
        Debug.Log("Sequence processing child:\t" + children[currentChild].name);

        Status childStatus = children[currentChild].Process();
        if (childStatus != Status.SUCCESS) return childStatus;

        currentChild++;
        if (currentChild >= children.Count)
        {
            currentChild = 0;
            return Status.SUCCESS;
        }

        return Status.RUNNING;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTNode
{
    public enum Status { SUCCESS, RUNNING, FAILURE };
    public Status status;
    public List<BTNode> children = new List<BTNode>();
    public int currentChild = 0;
    public string name;
    public BTLeaf currentLeaf = null;

    public BTNode() { }
    public BTNode(string name)
    {
        this.name = name;
    }


    public virtual Status Process()
    {
        return Status.SUCCESS;
    }

    public void AddChild(BTNode n)
    {
        children.Add(n);
    }
}

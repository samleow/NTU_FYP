using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree : BTNode
{
    struct NodeLevel
    {
        public int level;
        public BTNode node;
    }

    public BehaviourTree() : base("Tree") { }
    public BehaviourTree(string name) : base(name) { }

    public override Status Process()
    {
        return children[currentChild].Process();
    }


    // Debug
    public void PrintTree()
    {
        string treePrintout = "";
        Stack<NodeLevel> nodeStack = new Stack<NodeLevel>();
        BTNode currentNode = this;
        nodeStack.Push(new NodeLevel { level = 0, node = currentNode });

        while (nodeStack.Count != 0)
        {
            NodeLevel nextNode = nodeStack.Pop();
            treePrintout += new string('-', nextNode.level) + nextNode.node.name + "\n";
            for (int i = nextNode.node.children.Count - 1; i >= 0; i--)
            {
                nodeStack.Push(new NodeLevel { level = nextNode.level+1, node = nextNode.node.children[i] });
            }

        }

        Debug.Log(treePrintout);
    }

}

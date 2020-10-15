using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tetrahedron
{
    public List<Node> nodes;

    public Tetrahedron(Node nodeA, Node nodeB, Node nodeC, Node nodeD)
    {
        nodes = new List<Node>(4);
        nodes[0] = nodeA;
        nodes[1] = nodeB;
        nodes[2] = nodeC;
        nodes[3] = nodeD;
    }

    public Tetrahedron()
    {
        nodes = new List<Node>(4);
    }

    public void Add(Node node)
    {
        nodes.Add(node);
    }

    public bool CheckNode(Node node)
    {
        bool result = false;
        foreach(Node n in nodes)
        {
            if (n.Equals(node))
            {
                result = true;
            }
        }
        return result;
    }

    public string ToString()
    {
        return ("Nodo 1: " + nodes[0].ToString() + " | Nodo 2: " + nodes[1].ToString() + " | Nodo 3: " + nodes[2].ToString() + " | Nodo 4: " + nodes[3].ToString());
    }
}

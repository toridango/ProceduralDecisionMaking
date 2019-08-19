
using System.Collections.Generic;


// BT abstract node
public abstract class Node
{

    public enum Status
    {
        FRESH, RUNNING, FAILED, SUCCEEDED, CANCELLED
    };

    protected Status m_status;

    protected Node m_parent;
    protected List<Node> m_children;

    public Node()
    {

    }


    public Status GetStatus()
    {
        return m_status;
    }


    public abstract Status Evaluate();


    public void AddChild(Node n)
    {
        m_children.Add(n);
    }
}

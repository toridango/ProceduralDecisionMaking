
using System.Collections.Generic;

// BT Selector node:
// runs children until one succeeds or all fail
public class Selector : Node
{
    
    
    public Selector(List<Node> children)
    {
        m_children = children;
    }


    public override Status Evaluate()
    {
        foreach(Node n in m_children)
        {
            switch(n.Evaluate())
            {
                case Status.FAILED:
                    continue;
                case Status.SUCCEEDED:
                    m_status = Status.SUCCEEDED;
                    return m_status;
                case Status.RUNNING:
                    m_status = Status.RUNNING;
                    return m_status;
                default:
                    continue;
            }
        }
        m_status = Status.FAILED;
        return m_status;
    }
}

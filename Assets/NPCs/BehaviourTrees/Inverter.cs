
using System.Collections.Generic;


// BT Decorator node:
// inverts output of child node
public class Inverter : Node
{


    public Inverter(Node child)
    {
        m_children = new List<Node>() { child };
    }


    public override Status Evaluate()
    {
        foreach (Node n in m_children)
        {
            switch (n.Evaluate())
            {
                case Status.FAILED:
                    m_status = Status.SUCCEEDED;
                    return m_status;
                case Status.SUCCEEDED:
                    m_status = Status.FAILED;
                    return m_status;
                case Status.RUNNING:
                    m_status = Status.RUNNING;
                    return m_status;
            }
        }
        m_status = Status.SUCCEEDED;
        return m_status;
    }
}

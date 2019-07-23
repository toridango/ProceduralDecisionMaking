﻿
using System.Collections.Generic;

public class Sequence : Node
{


    public Sequence(List<Node> children)
    {
        m_children = children;
    }


    public override Status Evaluate()
    {
        bool anyChildRunning = false;

        foreach (Node n in m_children)
        {
            switch (n.Evaluate())
            {
                case Status.FAILED:
                    m_status = Status.FAILED;
                    return m_status;
                case Status.SUCCEEDED:
                    continue;
                case Status.RUNNING:
                    anyChildRunning = true;
                    continue;
                default:
                    m_status = Status.SUCCEEDED;
                    return m_status;
            }
        }
        m_status = anyChildRunning ? Status.RUNNING : Status.SUCCEEDED;
        return m_status;
    }
}

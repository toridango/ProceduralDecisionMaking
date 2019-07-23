﻿
using System.Collections.Generic;

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
}
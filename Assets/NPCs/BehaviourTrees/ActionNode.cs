

// BT Leaf node for delegate function that takes a string parameter
public class ActionNode : Node
{

    public delegate Status ActionDelegate(string s);

    private ActionDelegate m_action;
    private string m_arg;


    public ActionNode(ActionDelegate action, string arg)
    {
        m_action = action;
        m_arg = arg;
    }


    public override Status Evaluate()
    {
        switch (m_action(m_arg))
        {
            case Status.FAILED:
                m_status = Status.FAILED;
                return m_status;
            case Status.SUCCEEDED:
                m_status = Status.SUCCEEDED;
                return m_status;
            case Status.RUNNING:
                m_status = Status.RUNNING;
                return m_status;
            default:
                m_status = Status.FAILED;
                return m_status;
        }
    }
}

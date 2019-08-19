

// BT Leaf node for delegate function that takes two string parameters
public class ThresholdActionNode : Node
{

    public delegate Status ThresholdActionDelegate(string s, string targ);

    private ThresholdActionDelegate m_action;
    private string m_arg;
    private string m_targ;


    public ThresholdActionNode(ThresholdActionDelegate action, string arg, string targ)
    {
        m_action = action;
        m_arg = arg;
        m_targ = targ;
    }


    public override Status Evaluate()
    {
        switch (m_action(m_arg, m_targ))
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


// BT Leaf node for delegate function that takes a string and NPC parameters
public class VersusActionNode : Node
{

    public delegate Status VersusActionDelegate(string s, NPC npc);

    private VersusActionDelegate m_action;
    private string m_arg;
    private NPC m_npc;


    public VersusActionNode(VersusActionDelegate action, string arg, NPC npc)
    {
        m_action = action;
        m_arg = arg;
        m_npc = npc;
    }


    public override Status Evaluate()
    {
        switch (m_action(m_arg, m_npc))
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

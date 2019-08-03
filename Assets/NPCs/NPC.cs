using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class NPC : MonoBehaviour
{
    [SerializeField] private TextAsset npcSourceFile;
    [SerializeField] private Text m_overheadTextName;
    [SerializeField] private Text m_overheadTextDescription;
    [Tooltip("Public items in inventory")]
    [SerializeField] private List<string> m_publicInventory;
    [SerializeField] private bool m_verbose;
    [SerializeField] private bool m_logTimes;


    // <Properties>

    private string m_name;

    private Dictionary<string, int> m_personality;
    private Dictionary<string, int> m_skills;
    //private Dictionary<string, int> m_pseudoSkills;
    private Dictionary<string, int> m_allegiances;

    private Dialog m_dialog;
    private Node m_treeRoot;
    private Node.Status m_treeStatus;

    private Dictionary<string, string> m_actionToSkill;
    private Dictionary<string, double> m_skillThreshold;



    // <\Properties>


    // Dialog Data Structures

    // TODO Overhead message queue
    private Queue<string> m_overheadMessageQ;
    private string m_currOverheadMessage;

    // TODO Overhead message timer
    private float m_overheadMessageTimestamp;


    private float m_NPCProximityRadius;

    void Awake()
    {
        m_personality = new Dictionary<string, int>();
        m_skills = new Dictionary<string, int>();
        //m_pseudoSkills = new Dictionary<string, int>();
        m_allegiances = new Dictionary<string, int>();
        m_overheadMessageQ = new Queue<string>();

        m_actionToSkill = new Dictionary<string, string>()
        {
            { "steal", "avg_pickpocket_stealth" },
            { "persuade", "charisma" },
            { "intimidate", "combat" },
            { "craft", "crafts" },
            { "buy", "wealth" },
            { "bribe", "wealth" },
            { "fight", "combat" },
            { "assassinate", "stealth" },
            { "build", "crafts" },
            { "outsource", "wealth" }

        };

        m_skillThreshold = new Dictionary<string, double>()
        {
            { "apple", 1.0 }, // number in terms of wealth
            { "sword", 35.0 }, // number in terms of craftsmanship
            { "enchant", 80.0 }
        };

        LoadFromXmlTextAsset();
        m_treeRoot = null;

        m_NPCProximityRadius = 1.5f;
    }


    // Start is called before the first frame update
    void Start()
    {
        
        //m_overheadTextName = GetComponentInChildren<Canvas>().GetComponentsInChildren<Text>()[0];
        m_overheadTextName.text = m_name;
        gameObject.name = "NPC_" + m_name.Replace(" ", "");
        //m_overheadTextDescription = GetComponentInChildren<Canvas>().GetComponentsInChildren<Text>()[1];
        m_overheadTextDescription.text = "";
        PushOverheadMessage("Idling...");


    }

    // Update is called once per frame
    void Update()
    {
        if (m_treeRoot != null)
        {
            m_treeStatus = m_treeRoot.Evaluate();
            if(m_treeStatus == Node.Status.SUCCEEDED || 
                m_treeStatus == Node.Status.FAILED ||
                m_treeStatus == Node.Status.CANCELLED)
            {
                string msg = "";
                if (m_treeStatus == Node.Status.SUCCEEDED)
                    msg = "Objective Accomplished";
                else if (m_treeStatus == Node.Status.FAILED)
                    msg = "Objective Failed";
                else if (m_treeStatus == Node.Status.CANCELLED)
                    msg = "Objective Cancelled";

                if (m_verbose) Debug.Log(msg);
                m_treeRoot = null;
            }
        }
        UpdateOverheadMessage();
    }


    public string MakeDialogChoice(int choice)
    {
        string exitCode = "";
        exitCode = m_dialog.MakeChoice(choice);
        if (exitCode.StartsWith("a"))
        {
            PreProcessAction(exitCode);
        }
        return exitCode;
    }

    public Dialog GetDialog()
    {
        return m_dialog;
    }


    private void PushOverheadMessage(string msg)
    {
        m_overheadMessageQ.Enqueue(msg);
    }

    private void UpdateOverheadMessage()
    {
        if (m_overheadMessageQ.Count > 0)
        {
            if (m_currOverheadMessage != m_overheadMessageQ.Peek())
            {
                m_currOverheadMessage = m_overheadMessageQ.Dequeue();
                m_overheadTextDescription.text = m_currOverheadMessage;
                m_overheadMessageTimestamp = Time.realtimeSinceStartup;
            }
        }
        /*if (Time.realtimeSinceStartup - m_overheadMessageTimestamp > 5.0)
        {
            m_currOverheadMessage = "";
            m_overheadTextDescription.text = m_currOverheadMessage;
        }*/
    }


    private void PreProcessAction(string code)
    {
        char separator = '_';
        string[] codeSlices = code.Split(separator);
        float i_time = Time.realtimeSinceStartup;

        //foreach (string s in codeSlices)
        //    Debug.Log(s);

        if (codeSlices[0].Equals("a") && codeSlices.Length > 2)
            switch(codeSlices[1])
            {
                // Get Item
                case "gi":
                    {
                        string targetItem = codeSlices[2];
                        bool craftable = CheckCraftable(targetItem);
                        double utilityFind = CheckFind(targetItem);
                        float t0 = Time.realtimeSinceStartup;

                        List<System.Tuple<string, double>> uS = UtilityScoring.ScoreGetItem(m_personality, 
                                                                                            m_skills, 
                                                                                            utilityFind,
                                                                                            craftable);
                        if (m_logTimes) Debug.Log("\tScoring time:" + (Time.realtimeSinceStartup - t0).ToString());

                        t0 = Time.realtimeSinceStartup;
                        m_treeRoot = MakeTree(uS, targetItem);
                        if (m_logTimes) Debug.Log("\tMake Tree time:" + (Time.realtimeSinceStartup - t0).ToString());


                        // NPC announcement
                        /*string msg = "I will try ";

                        if (uS[0].Item1.EndsWith("e"))
                            msg += uS[0].Item1.Substring(0, uS[0].Item1.Length - 1) + "ing";
                        else
                            msg += uS[0] + "ing";

                        PushOverheadMessage(msg + " first");*/
                        break;
                    }
                // Convince
                case "co":
                    {
                        string targetName = "NPC_"+codeSlices[2];
                        float t0 = Time.realtimeSinceStartup;

                        List<System.Tuple<string, double>> uS = UtilityScoring.ScoreConvince(m_personality,
                                                                                            m_skills);
                        if (m_logTimes) Debug.Log("\tScoring time:" + (Time.realtimeSinceStartup - t0).ToString());


                        t0 = Time.realtimeSinceStartup;
                        m_treeRoot = MakeTree(uS, targetName);
                        if (m_logTimes) Debug.Log("\tMake Tree time:" + (Time.realtimeSinceStartup - t0).ToString());

                        break;
                    }
                // Neutralize
                case "nu":
                    {
                        string targetName = "NPC_"+codeSlices[2];
                        double allegianceToTarget = 0;
                        float t0 = Time.realtimeSinceStartup;

                        if (m_allegiances.ContainsKey(targetName))
                        {
                            allegianceToTarget = m_allegiances[targetName];
                        }
                        else
                        {
                            m_allegiances[targetName] = 0;
                        }

                        List<System.Tuple<string, double>> uS = UtilityScoring.ScoreNeutralize(m_personality,
                                                                                            m_skills,
                                                                                            allegianceToTarget);
                        if (m_logTimes) Debug.Log("\tScoring time:" + (Time.realtimeSinceStartup - t0).ToString());

                        t0 = Time.realtimeSinceStartup;
                        m_treeRoot = MakeTree(uS, targetName);
                        if (m_logTimes) Debug.Log("\tMake Tree time:" + (Time.realtimeSinceStartup - t0).ToString());

                        break;
                    }
                // Develop
                case "dv":
                    {
                        string target = codeSlices[2];
                        float t0 = Time.realtimeSinceStartup;

                        List<System.Tuple<string, double>> uS = UtilityScoring.ScoreDevelop(m_personality,
                                                                                            m_skills);
                        if (m_logTimes) Debug.Log("\tScoring time:" + (Time.realtimeSinceStartup - t0).ToString());

                        t0 = Time.realtimeSinceStartup;
                        m_treeRoot = MakeTree(uS, target);
                        if (m_logTimes) Debug.Log("\tMake Tree time:" + (Time.realtimeSinceStartup - t0).ToString());

                        break;
                    }
                default:
                    break;
            }

        if (m_logTimes) Debug.Log("\tTOTAL PREPROCESS TIME: " + (Time.realtimeSinceStartup - i_time).ToString());
    }

    // TODO specify for the rest of goals that are not move + versus
    private Node MakeTree(List<System.Tuple<string, double>> uScores, string target)
    {
        List<Node> actions = new List<Node>();
        string msg = "| ";

        foreach (System.Tuple<string, double> t in uScores)
        {

            // utility over zero
            if (t.Item2 > 0.0)
            {
                //Debug.Log(t.Item1 +": " + t.Item2.ToString());
                msg += t.Item1 + ": " + t.Item2.ToString() + " | ";

                Node action;
                // THRESHOLD NODE, NO MOVE
                if (t.Item1.Equals("craft") || t.Item1.Equals("build"))
                {
                    ThresholdActionNode.ThresholdActionDelegate check = new ThresholdActionNode.ThresholdActionDelegate(CheckSkillThreshold);
                    
                    ThresholdActionNode checkNode = new ThresholdActionNode(check, t.Item1, target);
                    //action = checkNode;
                    actions.Add(checkNode);

                }
                // ONLY MOVE
                else if (t.Item1.Equals("find"))
                {
                    ActionNode.ActionDelegate move = new ActionNode.ActionDelegate(MoveToTarget);
                    ActionNode moveNode = new ActionNode(move, target);
                    //action = moveNode;
                    actions.Add(moveNode);

                }
                else
                {
                    List<GameObject> npcs = FindNPCsWith(target);
                    if(npcs.Count > 0)
                    {
                        // MOVE THEN THRESHOLD NODE
                        if (t.Item1.Equals("buy") || t.Item1.Equals("outsource"))
                        {

                            string npc = npcs[0].name;
                            ActionNode.ActionDelegate move = new ActionNode.ActionDelegate(MoveToTarget);
                            ActionNode moveNode = new ActionNode(move, npc);

                            ThresholdActionNode.ThresholdActionDelegate check = new ThresholdActionNode.ThresholdActionDelegate(CheckSkillThreshold);

                            ThresholdActionNode thNode = new ThresholdActionNode(check, t.Item1, target);
                            Sequence moveandCheck = new Sequence(new List<Node>() { moveNode, thNode });
                            action = moveandCheck;
                            actions.Add(action);
                        }
                        // MOVE THEN VERSUS NODE
                        // steal, persuade, intimidate, bribe, fight, assassinate
                        else
                        {
                            string npc = npcs[0].name;
                            ActionNode.ActionDelegate move = new ActionNode.ActionDelegate(MoveToTarget);
                            ActionNode moveNode = new ActionNode(move, npc);

                            VersusActionNode.VersusActionDelegate check = new VersusActionNode.VersusActionDelegate(CheckSkillVersus);
                            NPC vs = GameObject.Find(npc).GetComponent<NPC>();
                            VersusActionNode checkNode = new VersusActionNode(check, t.Item1, vs);
                            Sequence moveandCheck = new Sequence(new List<Node>() { moveNode, checkNode });
                            action = moveandCheck;
                            actions.Add(action);

                        }
                    }
                }
            }
        }
        Selector selector = new Selector(actions);

        if (m_verbose) Debug.Log(msg);
        return selector;
    }
    

    private Node.Status MoveToTarget(string s)
    {
        GameObject go = GameObject.Find(s);
        if (go == null)
            return Node.Status.FAILED;

        GetComponent<AICharacterControl>().SetDestination(GetDestinationForGO(go), false);

        float remainingDist = (go.transform.position - transform.position).magnitude;

        if(m_verbose) Debug.Log("Moving to " + go.name);

        if(remainingDist < 2.0f)
        {
            return Node.Status.SUCCEEDED;
        }
        return Node.Status.RUNNING;
    }

    private Node.Status CheckSkillVersus(string a, NPC npc)
    {
        if (m_verbose) Debug.Log("Checking " + a + " vs " + npc.name);

        double myScore = 0.0;
        double theirScore = 0.0;
        string s = m_actionToSkill[a];

        if (s.StartsWith("avg"))
        {
            char separator = '_';
            string[] skills = s.Split(separator);
            for(int i = 1; i < skills.Length; ++i)
            {
                myScore += m_skills[skills[i]];
                theirScore += npc.GetSkill(skills[i]);
                //Debug.Log(skills[i]);
            }
            myScore /= (skills.Length - 1);
            theirScore /= (skills.Length - 1);
            //Debug.Log(myScore.ToString()+" "+theirScore.ToString());

        }
        else
        {
            myScore = m_skills[s];
            theirScore = npc.GetSkill(s);
        }

        if (myScore > theirScore)
            return Node.Status.SUCCEEDED;
        else
            return Node.Status.FAILED;
    }

    private Node.Status CheckSkillThreshold(string s, string targ)
    {
        string ps = m_actionToSkill[s];
        if (m_verbose) Debug.Log("Checking " + ps + " for " + targ);

        double threshold = m_skillThreshold[targ];
        double skill = 0.0;
        
        skill = (double)m_skills[ps];

        if (skill >= threshold)
            return Node.Status.SUCCEEDED;
        else
            return Node.Status.FAILED;
    }

    private bool CheckCraftable(string itemName)
    {
        switch(itemName)
        {
            case "sword":
                return true;
            default:
                return false;
        }
    }

    private double UtilityFind(GameObject go)
    {
        double dist = Vector2.Distance(this.transform.position, go.transform.position);
        return 1000.0 / (0.005 * dist + 10);
    }

    private double CheckFind(string itemName)
    {
        // utility depending on distance
        double utility = -1.0;
        List<GameObject> findables = new List<GameObject>();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Findable"))
        {
            if (go.name.StartsWith(itemName))
                findables.Add(go);
        }

        if(findables.Count > 0)
        {
            findables = findables.OrderBy(
                                    x => Vector2.Distance(this.transform.position, x.transform.position)
                                    ).ToList();

            utility = UtilityFind(findables[0]);
        }
        return utility;
    }

    public bool CheckHasItem(string itemName)
    {
        bool hasIt = false;
        foreach(string s in m_publicInventory)
        {
            if (s.Equals(itemName))
                hasIt = true;
        }

        return hasIt;
    }


    // Checks Inventories for target
    // Returns sorted list
    private List<GameObject> FindNPCsWith(string target)
    {
        List<GameObject> npcs = new List<GameObject>();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("NPC"))
        {
            if (go.name.Contains(target))
                npcs.Add(go);
            else if (go.GetComponent<NPC>().CheckHasItem(target))
                npcs.Add(go);
        }

        if (npcs.Count > 0)
        {
            npcs = npcs.OrderBy(
                                x => Vector2.Distance(this.transform.position, x.transform.position)
                                ).ToList();
        }
        return npcs;
    }


    /*private void GetItem(string target, string skillUse)
    {

        switch(skillUse)
        {
            case "pickpocket":
            {
                PushOverheadMessage("Pickpocketing...");
                GetComponent<AICharacterControl>().SetDestination(GetDestinationForString(target), true);
                break;
            }
            case "pay":
            {
                PushOverheadMessage("Paying...");
                GetComponent<AICharacterControl>().SetDestination(GetDestinationForString(target), true);
                break;
            }
            default:
                break;
        }

    }*/

    private Vector3 GetDestinationForString(string go)
    {
        GameObject targetGO = GameObject.Find(go);
        /*Vector3 targPos = targetGO.GetComponent<Transform>().position;
        Vector3 iniPos = gameObject.GetComponent<Transform>().position;
        Vector3 dest = iniPos + (targPos - iniPos) - m_NPCProximityRadius * (targPos - iniPos).normalized;

        return dest;*/
        return GetDestinationForGO(targetGO);
    }

    private Vector3 GetDestinationForGO(GameObject targetGO)
    {
        Vector3 targPos = targetGO.GetComponent<Transform>().position;
        Vector3 iniPos = gameObject.GetComponent<Transform>().position;
        Vector3 dest = iniPos + (targPos - iniPos) - m_NPCProximityRadius * (targPos - iniPos).normalized;

        return dest;
    }



    public int GetSkill(string s)
    {
        return m_skills[s];
    }

    /*public int GetPseudoSkill(string s)
    {
        return m_pseudoSkills[s];
    }*/


    // Read XML and set properties
    private void LoadFromXmlTextAsset()
    {

        XmlDocument xmld = new XmlDocument();
        xmld.LoadXml(npcSourceFile.text);
        string xpath = "npc";
        XmlNode root = xmld.SelectNodes(xpath)[0];
        m_name = root.Attributes["name"].Value;

        foreach (XmlNode child in root)
        {
            if (child.GetType() != typeof(System.Xml.XmlComment))
            {
                // swicth for names
                switch(child.Name)
                {
                    case "personality":
                    {
                        foreach (XmlNode grandchild in child)
                        {
                            //Debug.Log("per_" + grandchild.Name + ": " + grandchild.InnerText);
                            m_personality.Add(grandchild.Name, int.Parse(grandchild.InnerText));
                        }
                        break;
                    }
                    case "skills":
                    {
                        foreach (XmlNode grandchild in child)
                        {
                            //Debug.Log("ski_" + grandchild.Name + ": " + grandchild.InnerText);
                            m_skills.Add(grandchild.Name, int.Parse(grandchild.InnerText));
                        }                        
                        break;
                    }
                    /*case "pseudoskills":
                    {
                        foreach (XmlNode grandchild in child)
                        {
                            //Debug.Log("pse_" + grandchild.Name + ": " + grandchild.InnerText);
                            m_pseudoSkills.Add(grandchild.Name, int.Parse(grandchild.InnerText));
                        }
                        break;
                    }*/
                    case "allegiances":
                    {
                        foreach (XmlNode grandchild in child)
                        {
                            //Debug.Log("all_" + grandchild.Name + ": " + grandchild.InnerText);
                            m_allegiances.Add(grandchild.Name, int.Parse(grandchild.InnerText));
                        }
                        break;
                    }
                    case "dialog":
                    {
                        m_dialog = new Dialog(child);
                        /*foreach (XmlNode page in child)
                        {
                            string id = page.Attributes["id"].Value;
                            string s2 = page.SelectSingleNode("premise").InnerText;
                            string s3 = "";
                            foreach (XmlNode opt in page.SelectNodes("option"))
                            {
                                s3 += opt.Attributes["pointer"].Value + ": ";
                                s3 += opt.InnerText + " // ";
                            }

                            Debug.Log(id + ") " + s2 + "\n" + s3);
                            

                        }*/
                        break;
                    }
                    default:
                        break;
                }

                /*
                foreach(KeyValuePair<string, int> kvp in m_personality)
                    Debug.Log(kvp.ToString());
                foreach (KeyValuePair<string, int> kvp in m_skills)
                    Debug.Log(kvp.ToString());
                foreach (KeyValuePair<string, int> kvp in m_pseudoSkills)
                    Debug.Log(kvp.ToString());
                foreach (KeyValuePair<string, int> kvp in m_allegiances)
                    Debug.Log(kvp.ToString());
                */

            }
        }

    }
    // Dialog Getter


    // Choice receiver (process choice made)
}

using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;


// NPC acts as a container for the character's traits and their inventory,
// as well as their movement, dialog, skill checks, acceptal/refusal,
// and taking care of their own action scorings and BT generation
// find utility is calculated here instead in the utility scoring class
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
    private Dictionary<string, int> m_skills; // pseudoskills are included in skills for flexibility
    private Dictionary<string, int> m_allegiances;

    private Dialog m_dialog;
    private Node m_treeRoot;
    private Node.Status m_treeStatus;

    private Dictionary<string, string> m_actionToSkill;
    private Dictionary<string, double> m_skillThreshold;


    private System.Random rand;



    // <\Properties>


    // Dialog Data Structures


    private Queue<string> m_overheadMessageQ;
    private string m_currOverheadMessage;


    private float m_overheadMessageTimestamp;


    private float m_NPCProximityRadius;


    // Initialise NPC values
    void Awake()
    {
        // Initialise structures
        m_personality = new Dictionary<string, int>();
        m_skills = new Dictionary<string, int>();
        m_allegiances = new Dictionary<string, int>();
        m_overheadMessageQ = new Queue<string>();

        // seed random
        rand = new System.Random(Guid.NewGuid().GetHashCode());


        // Note: These information dictionaries ought to be in an .init file

        // action - skill equivalences
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

        // Db with wealth & craft shared thresholds for items
        m_skillThreshold = new Dictionary<string, double>()
        {
            { "apple", 1.0 }, // number in terms of wealth
            { "sword", 35.0 }, // number in terms of craftsmanship,
            { "wizardstaff", 90.0 },
            { "enchant", 70.0 },
            { "house", 98.0 },
            { "nails", 5.0 },
        };

        LoadFromXmlTextAsset();
        m_treeRoot = null;

        m_NPCProximityRadius = 1.5f;
    }


    // Start is called before the first frame update
    // Initialise name, and action text in overhead message
    void Start()
    {
        
        m_overheadTextName.text = m_name;
        gameObject.name = "NPC_" + m_name.Replace(" ", "");
        m_overheadTextDescription.text = "";
        PushOverheadMessage("Idling...");

    }

    // Update is called once per frame
    void Update()
    {
        // Check Behaviour Tree status if one has been set
        if (m_treeRoot != null)
        {
            m_treeStatus = m_treeRoot.Evaluate();
            if(m_treeStatus == Node.Status.SUCCEEDED || 
                m_treeStatus == Node.Status.FAILED ||
                m_treeStatus == Node.Status.CANCELLED)
            {
                string msg = "";
                if (m_treeStatus == Node.Status.SUCCEEDED)
                {
                    msg = "Objective Accomplished";
                }
                else if (m_treeStatus == Node.Status.FAILED)
                {
                    msg = "Objective Failed";
                }
                else if (m_treeStatus == Node.Status.CANCELLED)
                {
                    msg = "Objective Cancelled";
                }

                PushOverheadMessage(msg);

                if (m_verbose) Debug.Log(msg);
                m_treeRoot = null;
            }
        }

        // Update NPC's overhead message
        UpdateOverheadMessage();
    }


    // Make and process dialog choice
    public string MakeDialogChoice(int choice)
    {
        string exitCode = "";
        exitCode = m_dialog.MakeChoice(choice);
        string pageCode = exitCode;
        if (exitCode.StartsWith("a")) // Action
        {
            pageCode = exitCode;
            PreProcessAction(exitCode);
        }
        else if (exitCode.StartsWith("r")) // Request
        {
            if (AgreeToRequest()) // if agrees to the request
            {
                pageCode = "a" + exitCode.Substring(1);
                Debug.Log(pageCode);
                PreProcessAction(pageCode);
            }
            else
            {
                pageCode = "qn";
            }
        }
        else if (exitCode.StartsWith("t")) // Tests
        {
            BehaviourAnalytics.PerformAnalytics();
        }
        m_dialog.NextPage(pageCode);
        return pageCode;
    }

    public Dialog GetDialog()
    {
        return m_dialog;
    }

    // Add a message to the queue for the overhead message box
    private void PushOverheadMessage(string msg)
    {
        m_overheadMessageQ.Enqueue(msg);
    }

    // Update message shown over NPC's head
    private void UpdateOverheadMessage()
    {
        if (m_overheadMessageQ.Count > 0)
        {
            // Update if next is different
            if (m_currOverheadMessage != m_overheadMessageQ.Peek())
            {
                m_currOverheadMessage = m_overheadMessageQ.Dequeue();
                m_overheadTextDescription.text = m_currOverheadMessage;
                m_overheadMessageTimestamp = Time.realtimeSinceStartup;
            }
            // If it's the same
            // Update if there are messages waiting and current one has been there for more than 2 seconds
            else if (m_overheadMessageQ.Count > 1 && Time.realtimeSinceStartup - m_overheadMessageTimestamp > 2)
            {
                m_currOverheadMessage = m_overheadMessageQ.Dequeue();
                m_overheadTextDescription.text = m_currOverheadMessage;
                m_overheadMessageTimestamp = Time.realtimeSinceStartup;
            }
        }
    }

    // Score candidate actions depending on the goal then make a behaviour tree out of the ranking
    private void PreProcessAction(string code)
    {
        char separator = '_';
        string[] codeSlices = code.Split(separator);
        float i_time = Time.realtimeSinceStartup;
        

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

    // Generate Behaviour Tree given action ranking
    // target contains target gameobject (depends on the goal)
    private Node MakeTree(List<System.Tuple<string, double>> uScores, string target)
    {
        List<Node> actions = new List<Node>();
        string msg = "| ";

        foreach (System.Tuple<string, double> t in uScores)
        {

            // utility over zero
            if (t.Item2 > 0.0)
            {
                msg += t.Item1 + ": " + t.Item2.ToString() + " | ";

                Node action;
                // THRESHOLD NODE, NO MOVE
                if (t.Item1.Equals("craft") || t.Item1.Equals("build"))
                {
                    ThresholdActionNode.ThresholdActionDelegate check = new ThresholdActionNode.ThresholdActionDelegate(CheckSkillThreshold);
                    
                    ThresholdActionNode checkNode = new ThresholdActionNode(check, t.Item1, target);
                    actions.Add(checkNode);

                }
                // ONLY MOVE
                else if (t.Item1.Equals("find"))
                {
                    ActionNode.ActionDelegate move = new ActionNode.ActionDelegate(MoveToTarget);
                    ActionNode moveNode = new ActionNode(move, target);
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
    
    // Move to target (given as string code with the game object's name)
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


    // Check this NPC's skill against someone else's
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
            }
            myScore /= (skills.Length - 1);
            theirScore /= (skills.Length - 1);

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

    // Check if NPC's skill is enough for an action code
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

    // List of craftable items
    // Note: this ought to be a dictionary like the others
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

    // Utility for find is calculated in npc since UtilityScorings does not have access to Unity
    // Inverse of distance, aprox bell curve shape
    private double UtilityFind(GameObject go)
    {
        double dist = Vector2.Distance(this.transform.position, go.transform.position);
        return 1000.0 / (0.005 * dist + 10);
    }


    // Check if item is findable (given as string code) and return the find Utility for it (-1 if not found)
    private double CheckFind(string itemName)
    {
        // utility depending on distance
        double utility = -1.0;
        List<GameObject> findables = new List<GameObject>();

        // For all findable objects
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Findable"))
        {
            // Check if they're the target
            if (go.name.StartsWith(itemName))
                findables.Add(go);
        }

        // Sort by distance, calculate utility to closest
        if(findables.Count > 0)
        {
            findables = findables.OrderBy(
                                    x => Vector2.Distance(this.transform.position, x.transform.position)
                                    ).ToList();

            utility = UtilityFind(findables[0]);
        }
        return utility;
    }

    // return wether this NPC has target item (given as string code)
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


    // Checks NPC's Inventories for target item (given as string code)
    // Returns list sorted by proximity
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
    

    // Find game object and forward to GetDestination function
    private Vector3 GetDestinationForString(string go)
    {
        GameObject targetGO = GameObject.Find(go);
        return GetDestinationForGO(targetGO);
    }

    // Vector with target game object position in respect to origin of coordinates (closest given proximity radius)
    // Note: seems like targPos and proximity radius should be enough. REVIEW 
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
    

    // If enough allegiance for this NPC, roll for a chance they agree
    private bool AgreeToRequest()
    {
        double fr = m_personality["friendliness"];
        double ca = m_personality["caution"];
        double factor = (fr - ca) / 10;
        double allegiance;
        if (m_allegiances.ContainsKey("player"))
            allegiance = m_allegiances["player"];
        else
            allegiance = 0.0;

        double xMod = 0.01;


        if (allegiance > 50 - factor)
        {
            return rand.Next(100) < (factor + xMod * allegiance * allegiance);
        }
        else
            return false;
    }




    // Read XML and set properties of the NPC
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
                // swicth for names of nodes
                // add data to corresponding dictionary
                switch(child.Name)
                {
                    case "personality":
                    {
                        foreach (XmlNode grandchild in child)
                        {
                            m_personality.Add(grandchild.Name, int.Parse(grandchild.InnerText));
                        }
                        break;
                    }
                    case "skills":
                    {
                        foreach (XmlNode grandchild in child)
                        {
                            m_skills.Add(grandchild.Name, int.Parse(grandchild.InnerText));
                        }                        
                        break;
                    }
                    case "allegiances":
                    {
                        foreach (XmlNode grandchild in child)
                        {
                            m_allegiances.Add(grandchild.Name, int.Parse(grandchild.InnerText));
                        }
                        break;
                    }
                    case "dialog":
                    {
                        m_dialog = new Dialog(child);
                        break;
                    }
                    default:
                        break;
                }
                

            }
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class NPC : MonoBehaviour
{
    [SerializeField] private TextAsset npcSourceFile;
    [SerializeField] private Text m_overheadTextName;
    [SerializeField] private Text m_overheadTextDescription;
    
    
    // <Properties>

    private string m_name;

    private Dictionary<string, int> m_personality;
    private Dictionary<string, int> m_skills;
    private Dictionary<string, int> m_pseudoSkills;
    private Dictionary<string, int> m_allegiances;

    private Dialog m_dialog;

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
        m_pseudoSkills = new Dictionary<string, int>();
        m_allegiances = new Dictionary<string, int>();
        m_overheadMessageQ = new Queue<string>();

        LoadFromXmlTextAsset();

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
        switch(code)
        {
            case "aApple":
            {
                string skillUse = "pickpocket";
                string target = "NPC_VictimusMaximus";
                    Debug.Log(m_pseudoSkills["wealth"]);
                if (m_pseudoSkills["wealth"] < 75)
                {
                    skillUse = "pay";
                    target = "NPC_Belethor";
                }
                GetItem(target, skillUse);
                break;
            }
            default:
                break;
        }
    }

    private void GetItem(string target, string skillUse)
    {

        switch(skillUse)
        {
            case "pickpocket":
            {
                PushOverheadMessage("Pickpocketing...");
                GetComponent<AICharacterControl>().SetDestination(GetDestinationForNPC(target), true);
                break;
            }
            case "pay":
            {
                PushOverheadMessage("Paying...");
                GetComponent<AICharacterControl>().SetDestination(GetDestinationForNPC(target), true);
                break;
            }
            default:
                break;
        }

    }

    private Vector3 GetDestinationForNPC(string npc)
    {
        GameObject targetGO = GameObject.Find(npc);
        Vector3 targPos = targetGO.GetComponent<Transform>().position;
        Vector3 iniPos = gameObject.GetComponent<Transform>().position;
        Vector3 dest = iniPos + (targPos - iniPos) - m_NPCProximityRadius * (targPos - iniPos).normalized;

        return dest;
    }



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
                    case "pseudoskills":
                    {
                        foreach (XmlNode grandchild in child)
                        {
                            //Debug.Log("pse_" + grandchild.Name + ": " + grandchild.InnerText);
                            m_pseudoSkills.Add(grandchild.Name, int.Parse(grandchild.InnerText));
                        }
                        break;
                    }
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

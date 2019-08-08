using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Characters.FirstPerson;

public class DialogManager : MonoBehaviour
{

    [SerializeField] private GameObject dialogContainer;
    [SerializeField] private GameObject interactPromptContainer;
    [SerializeField] private Text dialogText;
    [SerializeField] private GameObject buttonContainer;
    [SerializeField] private GameObject playerController;

    private List<Button> optionButtons;
    private GameObject[] NPCs;
    private GameObject interactTarget;

    private int maxOptions = 9;
    private bool dialogActive = false;
    private bool interactPromptActive = false;
    private int optionCount = 0;
    private int lastOptionPressed = 0;

    private string[] keyNames = { "Option1", "Option2", "Option3", "Option4", "Option5",
                                "Option6", "Option7", "Option8", "Option9" };

    // Start is called before the first frame update
    void Start()
    {
        optionButtons = new List<Button>();

        for (int i = 0; i < maxOptions; ++i)
        {
            Button b = buttonContainer.gameObject.transform.Find(String.Format("Button ({0})", i + 1)).GetComponent<Button>();
            optionButtons.Add(b);

            b.gameObject.SetActive(false);
        }

        NPCs = GameObject.FindGameObjectsWithTag("NPC");

        //Text dialogText = dialogCanvas.gameObject.transform.Find("Text Dialog").GetComponent<Text>();
        dialogContainer.gameObject.SetActive(dialogActive);
        interactPromptContainer.gameObject.SetActive(dialogActive);

    }

    // Update is called once per frame
    void Update()
    {

        if (dialogActive)
        {
            int i = 0;
            int key = 0;

            while (key == 0 && i < keyNames.Length)
            {
                if (CrossPlatformInputManager.GetButtonDown(keyNames[i]))
                {
                    key = i + 1;
                }

                ++i;
            }

            lastOptionPressed = key;
            if (lastOptionPressed != 0)
                Debug.Log(String.Format("{0} pressed", lastOptionPressed));
        }
        else
        {
            bool showInteract = false;
            List<GameObject> foundNPCs = new List<GameObject>();

            foreach (GameObject go in NPCs)
            {
                if(Vector3.Distance(go.transform.position, playerController.transform.position) < 3.0f)
                {
                    showInteract = true;
                    foundNPCs.Add(go);
                }
            }
            if (showInteract)
            {
                interactPromptActive = true;
                interactPromptContainer.gameObject.SetActive(interactPromptActive);
                interactTarget = GetClosest(playerController.transform, foundNPCs);
            }
            else
            {
                interactPromptActive = false;
                interactPromptContainer.gameObject.SetActive(interactPromptActive);
            }

            if (CrossPlatformInputManager.GetButtonDown("Activate") && interactTarget != null)
            {
                interactPromptActive = false;
                interactPromptContainer.gameObject.SetActive(interactPromptActive);

                StartDialog();
            }

        }
    }


    public GameObject GetClosest(Transform targetTrans, List<GameObject> gos)
    {

        GameObject bestTarget = null;
        float minDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = targetTrans.transform.position;

        foreach (GameObject potentialTarget in gos)
        {
            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float sqrDistToTarget = directionToTarget.sqrMagnitude;
            if (sqrDistToTarget < minDistanceSqr)
            {
                minDistanceSqr = sqrDistToTarget;
                bestTarget = potentialTarget;
            }
        }

        return bestTarget;
    }


    // TODO: Give control of dialog text and choices to NPC
    // Change parameter to NPC in dialog and get information from them
    public void StartDialog()
    {
        dialogActive = true;
        dialogContainer.gameObject.SetActive(dialogActive);


        FirstPersonController fpc = playerController.GetComponent<FirstPersonController>();
        //Debug.Log(fpc.ToString());
        fpc.SendMessage("DialogLock");
        playerController.GetComponent<FirstPersonController>().enabled = false;

        UpdateDialog();

    }

    public void UpdateDialog()
    {
        Page currentPage = interactTarget.GetComponent<NPC>().GetDialog().GetCurrentPage();
        optionCount = currentPage.GetOptionCount();

        dialogText.text = currentPage.GetPremise();
        for (int i = 0; i < maxOptions; ++i)
        {
            if (i < optionCount)
            {
                optionButtons[i].gameObject.SetActive(true);
                optionButtons[i].GetComponentInChildren<Text>().text = currentPage.GetOption(i);
            }
            else
                optionButtons[i].gameObject.SetActive(false);
        }


    }

    public void QuitDialog()
    {
        dialogActive = false;
        dialogText.text = "";    

        dialogContainer.gameObject.SetActive(dialogActive);
        interactTarget.GetComponent<NPC>().GetDialog().Reset();
        interactTarget = null;

        /*playerController.GetComponent<FirstPersonController>().enabled = true;

        FirstPersonController fpc = playerController.GetComponent<FirstPersonController>();
        fpc.SendMessage("DialogUnlock");*/


    }

    public void SetChoiceVisibility(int num)
    {
    }
    

    public void SetText(string s)
    {
        dialogText.text = s;
    }


    public void MakeChoice(int choice)
    {
        if(interactTarget != null)
        {
            string exitCode = interactTarget.GetComponent<NPC>().MakeDialogChoice(choice-1);
            //Debug.Log(exitCode);

            UpdateDialog();

            if (exitCode.StartsWith("q") || exitCode.StartsWith("a") || exitCode.StartsWith("t"))
            {
                StartCoroutine(WaitSecondsAndQuitDialog(2));
            }
        }
    }

    public IEnumerator WaitSecondsAndQuitDialog(int seconds)
    {

        playerController.GetComponent<FirstPersonController>().enabled = true;

        FirstPersonController fpc = playerController.GetComponent<FirstPersonController>();
        fpc.SendMessage("DialogUnlock");


        //Debug.Log("Exiting dialog in " + seconds.ToString() + " seconds");
        yield return new WaitForSeconds(seconds);
        QuitDialog();

    }


    public IEnumerator GetChoice()
    {

        string[] keyNames = {
                            "Option1",
                            "Option2",
                            "Option3",
                            "Option4",
                            "Option5",
                            "Option6",
                            "Option7",
                            "Option8",
                            "Option9"
                         };


        // While not any is true (all are false). Note for self: array.Any(<var> => <condition on var>)
        //while (!numKeys.Any(item => item))


        int sum = 0;
        string option = "";

        do
        {
            option = "";
            foreach (string s in keyNames)
            {
                if(CrossPlatformInputManager.GetButtonDown(s))
                {
                    sum++;
                    option = s; 
                }
            }
            yield return null;

        } while (sum != 1);

        switch( Array.IndexOf(keyNames, option) + 1)
        {
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
                break;
            case 7:
                break;
            case 8:
                break;
            case 9:
                break;
            default:
                break;
        }
    }
}

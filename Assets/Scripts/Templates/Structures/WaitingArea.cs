using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

[CreateAssetMenu(fileName = "WaitingArea", menuName = "Structure/WaitingArea")]
public class WaitingArea : ScriptableObject{

    public static int numberOfActions;
    public List<string> methods;
    public MethodInfo[] methodArrInfo;
    public List<GameObject> agentsButtons = new List<GameObject>();
    public GameObject buttonSample;
    public string instanceName;

    public int GetNumberOfActions(){
        return numberOfActions;
    }

    public List<string> GetMethodNames(){
        return methods;
    }
    

    public void InitStats(){
        Type myType = (typeof(WaitingArea));
        methodArrInfo = myType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        for (int i = 0; i < methodArrInfo.Length; i++){
            if (methodArrInfo[i].Name.Contains("WaitingArea") && !methods.Contains(methodArrInfo[i].Name)) methods.Add(methodArrInfo[i].Name);
        }
        numberOfActions = methods.Count;
    }

    //USER DEFINED STRUCTURE INTERACTIONS
    public void AllocateRefugeeWaitingArea(){
        agentsButtons.Clear();// Reset agentButtons.

        //Enable UI pop up.
        GameObject gmManager = GameObject.Find("CampGameManager");
        GameObject uiPopUp = gmManager?.GetComponent<RefugeeCampGameManager>()?.GetWaitingAreaInteractionsGO();
        if (!uiPopUp.activeInHierarchy) uiPopUp.SetActive(true);
         
        //Acquire the child object for list of buttons (Agents Awaiting Allocation) aka WaitingArea as home
        GameObject refugeeHolder = GameObject.Find("RefugeeList");

        //Destroy Childs on each call to update.
        foreach (Transform child in refugeeHolder.transform) Destroy(child.transform.gameObject);
        

        //Add Agents Existing in Waiting Area as Home and include them in the list as (Buttons)
        List<GameObject> agents = new List<GameObject>(GameObject.FindGameObjectsWithTag("Refugee"));
        List<GameObject> waitingAreaAgents = new List<GameObject>();

        //Create Agent buttons 
        foreach (GameObject agent in agents){
            if (agent.GetComponent<RefugeeStats>().GetHome() != null){
                if (agent.GetComponent<RefugeeStats>().GetHome().name.Contains("WaitingArea") && agent.GetComponent<RefugeeStats>().GetHealthStatus() != RefugeeStats.Health.Dead){
                    agentsButtons.Add(Instantiate(buttonSample, refugeeHolder.transform, false));
                    waitingAreaAgents.Add(agent);
                }
            }
        }

        //Setting Button Names
        for (int i = 0; i < agentsButtons.Count; i++)
            agentsButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = waitingAreaAgents[i].GetComponent<RefugeeStats>().RefugeeName.Trim() + " " + waitingAreaAgents[i].GetComponent<RefugeeStats>().RefugeeSurname.Trim();

        //Acquire Tent 
        GameObject[] tentArr= GameObject.FindGameObjectsWithTag("Structure");
        List<GameObject> tentList = new List<GameObject>();

        //Check Tent availability and Type
        for (int z=0; z < tentArr.Length; z++){
            if (tentArr[z].name.Contains("Tent") && tentArr[z]?.GetComponent<TentStats>().occupants.Count < tentArr[z]?.GetComponent<TentStats>().maxCapacity) tentList.Add(tentArr[z]);
        }


        //Add Functionality to Buttons
        for(int j = 0; j < agentsButtons.Count; j++){
            int rndIndexTent = UnityEngine.Random.Range(0,tentList.Count); //selects a random Tent to allocate agent when pressing the button
            var refNav = waitingAreaAgents[j].GetComponent<RefugeeNavigation>();
            agentsButtons[j].GetComponent<Button>().onClick.AddListener(() => {  uiPopUp.SetActive(false); GameObject.Find("WaitingArea")?.GetComponent<StructureInteractor>().DisableActionMenu(); refNav.AddManualLocation(true); }); //1. Random Location refNav.AddNextLocation(tentArr[rndIndexTent]); 2.Manual Handle Location refNav.AddManualLocation(true);
        }
    }


    //REWORK IT. TODO DISMISS AGENTS DIRECTLY FROM WAITING AREA IF WE WANT TO.
    /*
    public void DismissRefugeeWaitingArea(){
        agentsButtons.Clear();// Reset agentButtons.

        //Enable UI pop up.
        GameObject gmManager = GameObject.Find("CampGameManager");
        GameObject uiPopUp = gmManager?.GetComponent<RefugeeCampGameManager>()?.GetWaitingAreaInteractionsGO();
        if (!uiPopUp.activeInHierarchy) uiPopUp.SetActive(true);

        //Acquire the child object for list of buttons (Agents Awaiting Allocation) aka WaitingArea as home
        GameObject refugeeHolder = GameObject.Find("RefugeeList");

        //Destroy Childs on each call to update.
        foreach (Transform child in refugeeHolder.transform) Destroy(child.transform.gameObject);


        //Add Agents Existing in Waiting Area as Home and include them in the list as (Buttons)
        List<GameObject> agents = new List<GameObject>(GameObject.FindGameObjectsWithTag("Refugee"));
        List<GameObject> waitingAreaAgents = new List<GameObject>();

        //Create Agent buttons 
        foreach (GameObject agent in agents){
            if (agent.GetComponent<RefugeeStats>().GetHome() != null){
                if (agent.GetComponent<RefugeeStats>().GetHome().name.Contains("WaitingArea") && agent.GetComponent<RefugeeStats>().GetHealthStatus() != RefugeeStats.Health.Dead){
                    agentsButtons.Add(Instantiate(buttonSample, refugeeHolder.transform, false));
                    waitingAreaAgents.Add(agent);
                }
            }
        }

        //Setting Button Names
        for (int i = 0; i < agentsButtons.Count; i++) agentsButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = waitingAreaAgents[i].name;

        //Add Functionality to Buttons
        for (int j = 0; j < agentsButtons.Count; j++){
            var refNav = waitingAreaAgents[j].GetComponent<RefugeeNavigation>();
            agentsButtons[j].GetComponent<Button>().onClick.AddListener(() => { Destroy(agents[j]); uiPopUp.SetActive(false); GameObject.Find("WaitingArea")?.GetComponent<StructureInteractor>().DisableActionMenu(); }); //1. Random Location refNav.AddNextLocation(tentArr[rndIndexTent]); 2.Manual Handle Location refNav.AddManualLocation(true);
        }
    }
    */

    //TODO ADD Similar to other structures logic 
    //public void UpgradeRefugeeWaitingArea(){
      //  string funcBaseStr = MethodBase.GetCurrentMethod().ToString();
      //  Debug.Log("Upgrade Waiting Area Capacity");
   // }

}

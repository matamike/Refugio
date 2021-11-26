using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Reflection;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

public class RefugeeInteractor : MonoBehaviour{
    public GameObject targetGO; // GO that holds a list of Actions for the said GO
    public GameObject buttonSample; //GO that is a button element and contains the basic structure for a button.
    public GameObject returnButtonActionMenu; //Return Button from Action Menu UI.
    public int numberOfButtons; //How many interactions should the member have.
    public List<GameObject> buttons;

    public UnityEngine.Object scrpObj; //Generic Object

    private List<TextMeshProUGUI> targetGOButtonTexts; // Assign for each of the Button texts a name and function to trigger

    void Awake(){
        scrpObj = DetectSrcObjClass(); //Detect the Generic Object as Scriptable Object Class and return and object that will be handled.
        SetInteractionButtons();
    }

    void Start(){
        targetGO = RefugeeCampGameManager.Instance.GetAgentButtonActionsGO();//Find Action Panel Holder
        //targetGO?.SetActive(false); //init disabled. --TODO RETHINK. MAYBE GLITCH.
    }

    void OnDestroy(){
        if (targetGO && RefugeeCampGameManager.Instance.actionListActiveGO == gameObject){
            DisableActionMenu();
        }
    }

    //Set Interaction Buttons For every Refugee Type
    private void SetInteractionButtons(){
        if (scrpObj is Refugee){
            Refugee myTent = scrpObj as Refugee;
            numberOfButtons = myTent.GetNumberOfActions(); //assign the predefined Scriptable Object Interaction count to buttons.
        } 
    }

    //Detect Scriptable Object Class for Refugee
    private UnityEngine.Object DetectSrcObjClass(){
        if (this.gameObject.name.Contains("Refugee")){
            Refugee x = (Refugee)scrpObj;
            x.InitStats();
            return x as Refugee;
        }
        return null;
    }

    //Enable Action Menu 
    public void EnableActionMenu(bool flag = false, GameObject go = null){

        if (!targetGO.activeInHierarchy){
            targetGO.SetActive(flag);
            GenerateButtons();
            RefugeeCampGameManager.Instance.actionListActiveGO = go; //update the Manager which GO is actively using the ActionList
            //Update Action List according to gameobject.
            //Update Icon /Description and all.
        }
        else{
            UpdateButtons();
            RefugeeCampGameManager.Instance.actionListActiveGO = go; //update the Manager which GO is actively using the ActionList
            //Update Action List according to gameobject.
            //Update Icon /Description and all.
        }
    }

    //Disable Action Menu
    public void DisableActionMenu(){

        if (RefugeeCampGameManager.Instance.actionListActiveGO is null){
            RefugeeCampGameManager.Instance.actionListActiveGO = null;
            RemoveButtons();
            buttons.Clear();
            if (targetGO != null) targetGO.SetActive(false);
        }
        else{
            RemoveButtons();
            buttons.Clear();
            if (targetGO != null) targetGO.SetActive(false);
        }
    }

    //Generate Button for Refugee Object
    void GenerateButtons(){
        RemoveButtons(); //HOTFIX FOR DUPLICATE REFUGEE OBJS
        for (int i = 1; i <= numberOfButtons; i++) buttons.Add(Instantiate(buttonSample, GameObject.Find("ActionHolder").transform));

        //REFUGEE BUTTON LIST AND EVENTS
        if (scrpObj is Refugee){
            Refugee myRefugee = scrpObj as Refugee;
            myRefugee.instanceName = this.gameObject.name;
            List<string> tentMethodNames = myRefugee.GetMethodNames();

            for (int i = 0; i < myRefugee.GetNumberOfActions(); i++)
            {
                //Add event to each button respectively.
                MethodInfo funcInfo = scrpObj.GetType().GetMethod(tentMethodNames[i]);
                UnityAction uAC = (UnityAction)funcInfo.CreateDelegate(typeof(UnityAction), scrpObj);
                buttons[i].GetComponent<Button>().onClick.AddListener(uAC); //Need to find a way to fit that format.
                //Add Text to each button respectively
                buttons[i].gameObject.name = tentMethodNames[i];
                buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = tentMethodNames[i];
            }
        }
    }

    //Remove Buttons for Structure Object
    public void RemoveButtons(){
        Stack<GameObject> actionsStackTemp = new Stack<GameObject>(GameObject.FindGameObjectsWithTag("ActionButton"));

        buttons.Clear(); //clear List
        while (actionsStackTemp.Count > 0){
            GameObject tempGO = actionsStackTemp.Pop();
            Destroy(tempGO);
        }
    }

    //Update Buttons between transition from one structure to another.
    void UpdateButtons(){
        RemoveButtons(); //clear buttons (if any)
        GenerateButtons(); //create new ones.
    }
}

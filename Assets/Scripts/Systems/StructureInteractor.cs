using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Reflection;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

public class StructureInteractor : MonoBehaviour {
    public GameObject targetGO = null; // GO that holds a list of Actions for the said GO
    public GameObject buttonSample; //GO that is a button element and contains the basic structure for a button.
    public GameObject returnButtonActionMenu; //Return Button from Action Menu UI.
    public int numberOfButtons; //How many interactions should the member have.
    public List<GameObject> buttons;

    public UnityEngine.Object scrpObj; //Generic Object
    
    private List<TextMeshProUGUI> targetGOButtonTexts; // Assign for each of the Button texts a name and function to trigger


    void Awake() {      
        scrpObj = DetectSrcObjClass(); //Detect the Generic Object as Scriptable Object Class and return and object that will be handled.
        SetInteractionButtons();
    }

    void Start(){
        targetGO = RefugeeCampGameManager.Instance.GetStructureButtonActionsGO();
        targetGO?.SetActive(false); //init disabled.
    }

    //Set Interaction Buttons For every structure Type
    private void SetInteractionButtons(){
        if (scrpObj is Tent){
            Tent myTent = scrpObj as Tent;
            numberOfButtons = myTent.GetNumberOfActions(); //assign the predefined Scriptable Object Interaction count to buttons.
        }
        if (scrpObj is Hospital){
            Hospital myHospital = scrpObj as Hospital;
            numberOfButtons = myHospital.GetNumberOfActions(); //assign the predefined Scriptable Object Interaction count to buttons.
        }
        if (scrpObj is FoodCourt){
            FoodCourt myFoodCourt = scrpObj as FoodCourt;
            numberOfButtons = myFoodCourt.GetNumberOfActions(); //assign the predefined Scriptable Object Interaction count to buttons.
        }

        if (scrpObj is WaitingArea){
            WaitingArea myWaitingArea = scrpObj as WaitingArea;
            numberOfButtons = myWaitingArea.GetNumberOfActions(); //assign the predefined Scriptable Object Interaction count to buttons.
        }
    }

    //Detect Scriptable Object Class for Structures
    private UnityEngine.Object DetectSrcObjClass(){
        if (this.gameObject.name.Contains("Tent")){
            Tent x = (Tent)scrpObj;
            x.InitStats();
            return x as Tent;
        }
        if (this.gameObject.name.Contains("Hospital")){
            Hospital x = (Hospital)scrpObj;
            x.InitStats();
            return x as Hospital;
        }
        if (this.gameObject.name.Contains("FoodCourt")){
            FoodCourt x = (FoodCourt)scrpObj;
            x.InitStats();
            return x as FoodCourt;
        }

        if (this.gameObject.name.Contains("WaitingArea")){
            WaitingArea x = (WaitingArea)scrpObj;
            x.InitStats();
            return x as WaitingArea;
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

    //Generate Button for Structure Object
    void GenerateButtons(){
        RemoveButtons(); //HOTFIX FOR DUPLICATE STRUCTURE OBJECTS (E.G TENTS)
        for (int i = 1; i <= numberOfButtons; i++) buttons.Add(Instantiate(buttonSample,GameObject.Find("ActionHolder").transform));

        //TENT BUTTON LIST AND EVENTS
        if (scrpObj is Tent){
           Tent myTent = scrpObj as Tent;
            myTent.instanceName = this.gameObject.name;
            List<string> tentMethodNames = myTent.GetMethodNames();

           for(int i = 0; i < myTent.GetNumberOfActions(); i++){
                //Add event to each button respectively.
                MethodInfo funcInfo = scrpObj.GetType().GetMethod(tentMethodNames[i]);
                UnityAction uAC = (UnityAction)funcInfo.CreateDelegate(typeof(UnityAction), scrpObj);
                buttons[i].GetComponent<Button>().onClick.AddListener(uAC);
                //Add Text to each button respectively
                buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = tentMethodNames[i];
           }
        }

        //HOSPITAL BUTTON LIST AND EVENTS
        if (scrpObj is Hospital){
            Hospital myHospital = scrpObj as Hospital;
            myHospital.instanceName = this.gameObject.name;
            List<string> tentMethodNames = myHospital.GetMethodNames();

            for (int i = 0; i < myHospital.GetNumberOfActions(); i++){
                //Add event to each button respectively.
                MethodInfo funcInfo = scrpObj.GetType().GetMethod(tentMethodNames[i]);
                UnityAction uAC = (UnityAction)funcInfo.CreateDelegate(typeof(UnityAction), scrpObj);
                buttons[i].GetComponent<Button>().onClick.AddListener(uAC); //Need to find a way to fit that format.
                //Add Text to each button respectively
                buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = tentMethodNames[i];
            }
        }

        //FOOD COURT BUTTON LIST AND EVENTS
        if (scrpObj is FoodCourt){
            FoodCourt myFoodCourt = scrpObj as FoodCourt;
            myFoodCourt.instanceName = this.gameObject.name;
            List<string> tentMethodNames = myFoodCourt.GetMethodNames();

            for (int i = 0; i < myFoodCourt.GetNumberOfActions(); i++){
                //Add event to each button respectively.
                MethodInfo funcInfo = scrpObj.GetType().GetMethod(tentMethodNames[i]);
                UnityAction uAC = (UnityAction)funcInfo.CreateDelegate(typeof(UnityAction), scrpObj);
                buttons[i].GetComponent<Button>().onClick.AddListener(uAC); //Need to find a way to fit that format.
                //Add Text to each button respectively
                buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = tentMethodNames[i];
            }
        }

        //WAITING AREA BUTTON LIST AND EVENTS
        if (scrpObj is WaitingArea){
            WaitingArea myWaitingArea = scrpObj as WaitingArea;
            myWaitingArea.instanceName = this.gameObject.name;
            List<string> tentMethodNames = myWaitingArea.GetMethodNames();

            for (int i = 0; i < myWaitingArea.GetNumberOfActions(); i++){
                //Add event to each button respectively.
                MethodInfo funcInfo = scrpObj.GetType().GetMethod(tentMethodNames[i]);
                UnityAction uAC = (UnityAction)funcInfo.CreateDelegate(typeof(UnityAction), scrpObj);
                buttons[i].GetComponent<Button>().onClick.AddListener(uAC); //Need to find a way to fit that format.
                //Add Text to each button respectively
                buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = tentMethodNames[i];
            }
        }
    }

    //Remove Buttons for Structure Object
    public void RemoveButtons(){
        Stack<GameObject> actionsStackTemp = new Stack<GameObject>(GameObject.FindGameObjectsWithTag("ActionButton"));

        buttons.Clear(); //clear List
        while(actionsStackTemp.Count > 0) {
            GameObject tempGO = actionsStackTemp.Pop();
            Destroy(tempGO);
        }
    }

    //Update Buttons between transition from one structure to another.
    void UpdateButtons(){
        RemoveButtons(); //clear buttons (if any)
        GenerateButtons(); //create new ones.
    }


    //public void SetTargetGO(GameObject target){
    //  targetGO = target;
    //}

    //public GameObject GetTargetGO(){
    //  GameObject target = targetGO;
    //  return target;
    //}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectHighlight : MonoBehaviour{
    UnityEvent highlightInfo_event;
    private bool isHighlighted = false;
    GameObject objFocus;
    GameObject camGO;

    public GameObject highlightTextGO;

    void Start(){
        camGO = GameObject.Find("Main Camera");
        objFocus = GameObject.Find("CameraFocus");
        if (highlightInfo_event == null) highlightInfo_event = new UnityEvent();

    }

    void Update(){
        if (isHighlighted){
            highlightInfo_event.RemoveAllListeners(); //clear listeners
            highlightInfo_event.AddListener(ShowHighlightedText); // add listener
            highlightInfo_event.AddListener(UpdateHighlightedInfo); //add listener
            
            if(objFocus != null)
                if(Vector3.Distance(objFocus.transform.position, this.transform.position) > 1.0f) isHighlighted = false;
            
            FocusUIDirection(); //Look at Camera
        }
        else{
            isHighlighted = false;
            highlightInfo_event.RemoveAllListeners(); //clear listeners
            highlightInfo_event.AddListener(HideHighlightedText); // add listener
            highlightInfo_event.Invoke(); //trigger
        }

        //Enforce Highlight to switch off (Bug Preventing)
        if (camGO.GetComponent<CameraControl>().interactingGO != gameObject) {
            isHighlighted = false;
            highlightTextGO.SetActive(isHighlighted); 
        }
    }

    public void Highlight(bool isHL){
        isHighlighted = isHL;
        highlightInfo_event.Invoke(); //trigger
    }

    void ShowHighlightedText(){
        //Enable UI Highlight
        if(!highlightTextGO.activeSelf) highlightTextGO.SetActive(true);
        //this.gameObject.transform.localScale = new Vector3(4.0f, 4.0f, 4.0f);//DEBUG 
    }

    void HideHighlightedText(){
        //Disable UI HighLight
        if (highlightTextGO.activeSelf) highlightTextGO.SetActive(false);
        //this.gameObject.transform.localScale = new Vector3(3.0f, 3.0f, 3.0f); //DEBUG
    }

    void UpdateHighlightedInfo(){    
        if (highlightTextGO.gameObject.activeInHierarchy){
            //Update Structure Occupants/Capacity and update progress bar.

            //Tent
            if (gameObject.name.Contains("Tent")){
                Transform occupants = highlightTextGO.transform.Find("Background/Ballon/Info/Info 1"); //Acquire Occupant
                Transform progressBar = highlightTextGO.transform.Find("Background/Ballon/ProgressBar"); //Acquire Progress Bar 

                TentStats tntStats = gameObject.GetComponent<TentStats>();
                if (occupants != null) occupants.Find("InfoValue").GetComponent<TextMeshProUGUI>().text = tntStats.occupants.Count.ToString() + "/" + tntStats.maxCapacity.ToString();
                if (progressBar != null){
                    progressBar.GetComponent<Slider>().maxValue = tntStats.maxCapacity;
                    progressBar.GetComponent<Slider>().value = tntStats.occupants.Count;
                }
            }

            //Hospital
            if (gameObject.name.Contains("Hospital")){
                Transform occupants = highlightTextGO.transform.Find("Background/Ballon/Info/Info 1"); //Acquire Occupants
                Transform capacity = highlightTextGO.transform.Find("Background/Ballon/Info/Info 2"); //Acquire Occupants
                Transform progressBar = highlightTextGO.transform.Find("Background/Ballon/ProgressBar"); //Acquire Progress Bar 

                HospitalStats hospitalStats = gameObject.GetComponent<HospitalStats>();
                if (occupants != null) occupants.Find("InfoValue").GetComponent<TextMeshProUGUI>().text = RefugeeCampGameManager.Instance.medicineResourceNum.ToString();
                if (capacity != null) capacity.Find("InfoValue").GetComponent<TextMeshProUGUI>().text = hospitalStats.occupants.Count.ToString() + "/" + hospitalStats.maxCapacity.ToString();
                if (progressBar != null)
                {
                    progressBar.GetComponent<Slider>().maxValue = hospitalStats.maxCapacity;
                    progressBar.GetComponent<Slider>().value = hospitalStats.occupants.Count;
                }
            }

            //Food Court
            if (gameObject.name.Contains("Food")){
                Transform occupants = highlightTextGO.transform.Find("Background/Ballon/Info/Info 1"); //Acquire Occupants
                Transform capacity = highlightTextGO.transform.Find("Background/Ballon/Info/Info 2"); //Acquire Occupants
                Transform progressBar = highlightTextGO.transform.Find("Background/Ballon/ProgressBar"); //Acquire Progress Bar 

                FoodCourtStats foodCourtStats = gameObject.GetComponent<FoodCourtStats>();
                if (occupants != null) occupants.Find("InfoValue").GetComponent<TextMeshProUGUI>().text = RefugeeCampGameManager.Instance.foodResourceNum.ToString();
                if (capacity != null) capacity.Find("InfoValue").GetComponent<TextMeshProUGUI>().text = foodCourtStats.occupants.Count.ToString() + "/" +foodCourtStats.maxCapacity.ToString();
                if (progressBar != null)
                {
                    progressBar.GetComponent<Slider>().maxValue = foodCourtStats.maxCapacity;
                    progressBar.GetComponent<Slider>().value = foodCourtStats.occupants.Count;
                }
            }

            //Waiting Area
            if (gameObject.name.Contains("Waiting")){
                Transform occupants = highlightTextGO.transform.Find("Background/Ballon/Info/Info 1"); //Acquire Occupant
                Transform progressBar = highlightTextGO.transform.Find("Background/Ballon/ProgressBar"); //Acquire Progress Bar 

                WaitingAreaStats waitingAreaStats = gameObject.GetComponent<WaitingAreaStats>();
                if (occupants != null) occupants.Find("InfoValue").GetComponent<TextMeshProUGUI>().text = waitingAreaStats.occupants.Count.ToString() + "/" + waitingAreaStats.maxCapacity.ToString();
                if (progressBar != null){
                    progressBar.GetComponent<Slider>().maxValue = waitingAreaStats.maxCapacity;
                    progressBar.GetComponent<Slider>().value = waitingAreaStats.occupants.Count;
                }
            }

            //Refugee
            if (gameObject.name.Contains("Refugee")){
                GameObject refugeeNameGO = highlightTextGO.transform.Find("HighLightTextName").gameObject; //Acquire FullName TextHolder
                GameObject refugeeSurnameGO = highlightTextGO.transform.Find("HighLightTextSurname").gameObject; //Acquire FullName TextHolder
                RefugeeStats refStats = gameObject.GetComponent<RefugeeStats>(); //Get Refugee Stats

                if (refugeeNameGO != null && refugeeSurnameGO != null){
                    refugeeNameGO.GetComponent<TextMeshProUGUI>().text = "Name : " + refStats.refName;
                    refugeeSurnameGO.GetComponent<TextMeshProUGUI>().text = "";//"Surname : " + refStats.refSurname;
                }
            }
        }
    }

    void FocusUIDirection(){
        highlightTextGO.transform.LookAt(camGO.transform.position, Vector3.up); //Face direction towards the camera.
    }
}

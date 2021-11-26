using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{
    private GameObject cam;

    void Start(){
        cam = Camera.main.gameObject;
    }

    public void OnPointerEnter(PointerEventData eventData){
        if (Time.timeScale == 0) eventData.eligibleForClick = false;
        if (Time.timeScale > 0) eventData.eligibleForClick = true;

        if (cam != null && cam.TryGetComponent(out CameraControl comp)) cam.GetComponent<CameraControl>().rayLength = 0.0f;

        //Main Menu only
        if (SceneManager.GetActiveScene().name.Contains("Menu")) {
            if (eventData.pointerCurrentRaycast.gameObject.name.Equals("StartText")) {
                RectTransform rectTrans = eventData.pointerCurrentRaycast.gameObject.transform.parent.gameObject.GetComponent<RectTransform>();
                rectTrans.sizeDelta = new Vector3(200, 200);
            }

            if (eventData.pointerCurrentRaycast.gameObject.name.Equals("ExitText")){
                RectTransform rectTrans = eventData.pointerCurrentRaycast.gameObject.transform.parent.gameObject.GetComponent<RectTransform>();
                rectTrans.sizeDelta = new Vector3(200, 200);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData){
        if (Time.timeScale == 0) eventData.eligibleForClick = true;
        if (cam != null && cam.TryGetComponent(out CameraControl comp)) cam.GetComponent<CameraControl>().rayLength = 40.0f;

        //Main Menu only
        if (SceneManager.GetActiveScene().name.Contains("Menu")){
            if (!eventData.pointerCurrentRaycast.gameObject.name.Equals("StartText")){
                RectTransform rectTrans = GameObject.Find("Start")?.GetComponent<RectTransform>();
                rectTrans.sizeDelta = new Vector3(133, 133);
            }

            if (!eventData.pointerCurrentRaycast.gameObject.name.Equals("ExitText")){
                RectTransform rectTrans = GameObject.Find("Exit")?.GetComponent<RectTransform>();
                rectTrans.sizeDelta = new Vector3(133, 133);
            }
        }
    }

    void OnDisable(){
        if(cam!=null && cam.TryGetComponent(out CameraControl comp)) comp.rayLength = 40.0f;
    }
}

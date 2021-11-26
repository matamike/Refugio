using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonClick : MonoBehaviour,IPointerClickHandler{
    private GameObject cam;

    void Start(){
        cam = Camera.main.gameObject;
    }


    public void OnPointerClick(PointerEventData eventData){
       // Debug.Log("The button has been clicked");
    }


    void OnDisable(){
       // Debug.Log("The Button has been disabled");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackBall : MonoBehaviour{

    Vector3 lastKnownPosition; 
    void Start(){
        transform.position = new Vector3(0.0f, 0.0f, 0.0f);
        lastKnownPosition = transform.position;
    }
    void Update(){
        if (Input.GetMouseButton(2)) transform.position = lastKnownPosition;
        else lastKnownPosition = transform.position;

        //Debug.Log(lastKnownPosition);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLimiter : MonoBehaviour
{
    public Vector3 WorldPointOrigin = Vector3.zero; //default (0,0,0)

    
    [Range(10,100)]
    public float MaxCameraRange = 40.0f;

    public bool lockControls = false;

    void Update(){
        if (Mathf.Abs(transform.position.x) > (WorldPointOrigin.x + MaxCameraRange) || Mathf.Abs(transform.position.z) > (WorldPointOrigin.z + MaxCameraRange)){
            lockControls = true;
            transform.position = Vector3.Lerp(transform.position, new Vector3(WorldPointOrigin.x, transform.position.y, WorldPointOrigin.z), 1.0f * Time.deltaTime);
        }
        else{ lockControls = false; }
    }

    public bool GetControlLockStatus() { return lockControls; }
}

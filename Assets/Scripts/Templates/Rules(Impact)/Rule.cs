using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;


[CreateAssetMenu(fileName = "Rule", menuName = "Impact/Rule")]
public class Rule : ScriptableObject{

    [Tooltip("Restriction Type")]
    public string restrictionType = "";
    public string restrictionValue = "";
    public bool hasImpact = false;

    private Type ruleType = null;
   // private Type ruleTypeValue = null;

    public void ConvertStringToType(){
        Type tempType = Type.GetType(restrictionType);
        if (tempType != null)
        {
            Debug.Log("Type: " + tempType.ToString());
            ruleType = tempType;
        }
        else ruleType = null;
    }

    public void ConvertStringValueToTypeValue(){
        if (ruleType.IsClass){

        }
        if (ruleType.IsEnum){

        }
        if(ruleType == typeof(string)){

        }
        if(ruleType == typeof(int)){

        }
        if (ruleType == typeof(float)){

        }
        if (ruleType == typeof(Vector3)){

        }

    }
}

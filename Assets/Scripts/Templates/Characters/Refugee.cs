using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

[CreateAssetMenu(fileName = "Refugee", menuName = "NPC/Refugee")]
public class Refugee : ScriptableObject{

    public static int numberOfActions;
    public List<string> methods;
    public MethodInfo[] methodArrInfo;
    public string instanceName;


    public int GetNumberOfActions(){
        return numberOfActions;
    }

    public List<string> GetMethodNames(){
        return methods;
    }


    public void InitStats(){
        Type myType = (typeof(Refugee));
        methodArrInfo = myType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        for (int i = 0; i < methodArrInfo.Length; i++){
            if (methodArrInfo[i].Name.Contains("Refugee") && !methods.Contains(methodArrInfo[i].Name)) methods.Add(methodArrInfo[i].Name);
        }
        numberOfActions = methods.Count;
    }


    //USER DEFINED STRUCTURE INTERACTIONS
    //Keep them in case we need to do something else or change something.

    //public void AllocateRefugee(){
      //  GameObject instance = GameObject.Find(instanceName); //Find the Instance this scrObj is refering to
      //  instance?.GetComponent<RefugeeNavigation>().AddManualLocation(true);
    //}

    //public void DismissRefugee(){
     //   string funcBaseStr = MethodBase.GetCurrentMethod().ToString();
     //   Debug.Log("Release Agent from the camp. Farewell!");
    //}
}

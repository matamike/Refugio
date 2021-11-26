using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Restriction", menuName = "Restriction")]
public class Restriction : ScriptableObject {
    public bool hasInitialized = false;

    [Serializable]
    public enum RestrictionEntityType {
        None, Women, Men, Children, People, Families
    };

    [Serializable]
    public enum EntityCondition {
        None,
        Healthy, //= RefugeeStats.Health.Healthy,
        Sick, //= RefugeeStats.Health.Sick,
        Dead,// = RefugeeStats.Health.Dead,
        Full,// = RefugeeStats.Hunger.Full,
        Hungry, //= RefugeeStats.Hunger.Hungry,
        Starving,// = RefugeeStats.Hunger.Starving,
        Happy, //= RefugeeStats.EmotionStatus.Happy,
        Sad, //= RefugeeStats.EmotionStatus.Sad,
        Depressed //= RefugeeStats.EmotionStatus.Depressed
    }

    
    public string donationEntity = "";
    public int score = 0;
    [SerializeField]
    private string agreementMessage = "Don't accept "; //message to display (generated)
    public RestrictionEntityType entityType = RestrictionEntityType.None;
    public string birthplace = "";
    public string profession = "";
    public EntityCondition entityCond = EntityCondition.None;
    public bool olderThan = false;
    public bool youngerThan = false;
    [Range(1,100)]
    public int age;
    

    public void AnalyzeMessage(){
        //if (hasInitialized) return;
        agreementMessage = "Don't accept ";
        if (entityType == RestrictionEntityType.None) return;

        //Append Entity Type and Amount
        agreementMessage += " " + entityType.ToString();

        if (olderThan) agreementMessage += " older than " + age;
        if (youngerThan) agreementMessage += " younger than " + age;

        //Append Birthplace requirement if Exists
        if (birthplace != "") agreementMessage += " from " + birthplace;

        //Append Profession requirement if Exists
        if (profession != "") agreementMessage += " that work as " + profession;

        if (entityCond != EntityCondition.None) agreementMessage += " with " + entityCond.ToString() + " condition ";

        agreementMessage += "."; //finishing the message

        if (!agreementMessage.Contains("Don't accept ")){
            string temp = "Don't accept ";
            temp += agreementMessage;
            agreementMessage = temp;
        }

        //DEBUG
        //Debug.Log(agreementMessage);
        score = 0;
        hasInitialized = true;
    }

    public string GetMessage(){
        return agreementMessage;
    }

}

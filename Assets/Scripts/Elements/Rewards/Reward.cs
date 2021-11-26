using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Reward", menuName = "Reward")]
public class Reward : ScriptableObject{
    public bool hasInitialized = false;

    public enum RewardEntityType{
        None,Women,Men,Children,People,Families
    };

    
    public string donationEntity = "";
    public int score = 0;
    [SerializeField]
    private string agreementMessage = "Add "; //message to display (generated)
    [Range(3, 7)]
    public int entitiesAmount;
    public RewardEntityType entityType = RewardEntityType.None;
    public string birthplace = "";
    public string profession = "";
    

    public void AnalyzeMessage(){
        //if (hasInitialized) return;
        agreementMessage = "Add ";
        if (entityType == RewardEntityType.None) return;

        //Append Entity Type and Amount
        agreementMessage += entitiesAmount.ToString() + " " + entityType.ToString();
                
        //Append Birthplace requirement if Exists
        if (birthplace != "") agreementMessage += " from " + birthplace;

        //Append Profession requirement if Exists
        if (profession != "") agreementMessage += " that work as " + profession;

        agreementMessage += "."; //finishing the message

        if (!agreementMessage.Contains("Add")){
            string temp = "Add ";
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

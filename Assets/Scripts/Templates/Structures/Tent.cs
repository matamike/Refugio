using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using UnityEngine.UI;
using TMPro;

[CreateAssetMenu(fileName ="Tent", menuName ="Structure/Tent")]
public class Tent : ScriptableObject{

    public static int numberOfActions;
    public  List<string> methods;
    public MethodInfo[] methodArrInfo;
    public string instanceName;

    //Upgrade Tent Variables

    private TextMeshProUGUI upgradePriceTag;
    private Button acceptPurchase, cancelPurchase;


    public int GetNumberOfActions(){
       return numberOfActions;
    }

    public List<string> GetMethodNames(){
        return methods;
    }

    public void InitStats(){
        Type myType = (typeof(Tent));
        methodArrInfo = myType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        for (int i = 0; i < methodArrInfo.Length; i++){
            if(methodArrInfo[i].Name.Contains("Tent") && !methods.Contains(methodArrInfo[i].Name)) methods.Add(methodArrInfo[i].Name);
        }
        numberOfActions = methods.Count;
    }

    //USER DEFINED STRUCTURE INTERACTIONS
    public void UpgradeTent(){
        GameObject tempDialogueUpgrade = RefugeeCampGameManager.Instance.GetUpgradeDialogueGO(); //Get Access to Dialogue from Manager
        TentStats tent = GameObject.Find(instanceName)?.GetComponent<TentStats>();

        if (!tempDialogueUpgrade.activeInHierarchy){
            tempDialogueUpgrade.SetActive(true); //enable Dialogue
            upgradePriceTag = GameObject.Find("UpgradePopUpDialogueText")?.GetComponent<TextMeshProUGUI>(); //Get Access to Price Tag dialogue

            //Update Dialogue Text Accordingly (Upgrade Price Tag)
            if (tent.currentLevel < tent.upgradeLevels.Length) upgradePriceTag.text = "Upgrade Cost: " + tent.upgradeLevelCosts[tent.currentLevel].ToString();
            else upgradePriceTag.text = "Reached Max Level";

            Button[] dialogueButtons = tempDialogueUpgrade.GetComponentsInChildren<Button>();
            foreach (Button b in dialogueButtons){
                //Clean and Add Listeners for Accept
                if (b.gameObject.name.Contains("Accept")){
                    acceptPurchase = b;
                    acceptPurchase.onClick.RemoveAllListeners();
                    acceptPurchase.onClick.AddListener(() => { tent.Transaction(); acceptPurchase.onClick.RemoveAllListeners(); tempDialogueUpgrade.SetActive(false);});
                }

                //Clean and Add Listeners for Cancel
                if (b.gameObject.name.Contains("Cancel")){
                    cancelPurchase = b;
                    cancelPurchase.onClick.RemoveAllListeners();
                    cancelPurchase.onClick.AddListener(() => { cancelPurchase.onClick.RemoveAllListeners(); tempDialogueUpgrade.SetActive(false); });
                }
            }

            //When we reach Max Upgrades (MAYBE CHANGE THE WAY IT WORKS ????~~~)
            if (upgradePriceTag.text.Contains("Max"))
            {
                acceptPurchase.gameObject.SetActive(false); // Disable Accept button
                cancelPurchase.GetComponentInChildren<TextMeshProUGUI>().text = "Return"; //Rename Cancel to Return
                cancelPurchase.onClick.AddListener(() => { acceptPurchase.gameObject.SetActive(true); });
            }
            else cancelPurchase.GetComponentInChildren<TextMeshProUGUI>().text = "Cancel"; //Rename Return to Cancel

        }
    }
}

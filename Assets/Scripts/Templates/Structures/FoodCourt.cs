using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using UnityEngine.UI;
using TMPro;

[CreateAssetMenu(fileName = "FoodCourt", menuName = "Structure/FoodCourt")]
public class FoodCourt : ScriptableObject
{

    public static int numberOfActions;
    public List<string> methods;
    public MethodInfo[] methodArrInfo;
    public string instanceName;


    //Upgrade FoodCourt Variables

    private TextMeshProUGUI upgradePriceTag;
    private Button acceptPurchase, cancelPurchase;

    public int GetNumberOfActions(){
     return numberOfActions;
    }

    public List<string> GetMethodNames(){
      return methods;
    }

    public void InitStats(){
        Type myType = (typeof(FoodCourt));
        methodArrInfo = myType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        for (int i = 0; i < methodArrInfo.Length; i++){
            if (methodArrInfo[i].Name.Contains("FoodCourt") && !methods.Contains(methodArrInfo[i].Name)) methods.Add(methodArrInfo[i].Name);
        }
        numberOfActions = methods.Count;
    }

    //USER DEFINED STRUCTURE INTERACTIONS

    public void UpgradeFoodCourt(){
        GameObject tempDialogueUpgrade = RefugeeCampGameManager.Instance.GetUpgradeDialogueGO(); //Get Access to Dialogue from Manager
        FoodCourtStats foodCourt = GameObject.Find(instanceName)?.GetComponent<FoodCourtStats>();

        if (!tempDialogueUpgrade.activeInHierarchy)
        {
            tempDialogueUpgrade.SetActive(true); //enable Dialogue
            upgradePriceTag = GameObject.Find("UpgradePopUpDialogueText")?.GetComponent<TextMeshProUGUI>(); //Get Access to Price Tag dialogue

            //Update Dialogue Text Accordingly (Upgrade Price Tag)
            if (foodCourt.currentLevel < foodCourt.upgradeLevels.Length) upgradePriceTag.text = "Upgrade Cost: " + foodCourt.upgradeLevelCosts[foodCourt.currentLevel].ToString();
            else upgradePriceTag.text = "Reached Max Level";

            Button[] dialogueButtons = tempDialogueUpgrade.GetComponentsInChildren<Button>();
            foreach (Button b in dialogueButtons)
            {
                //Clean and Add Listeners for Accept
                if (b.gameObject.name.Contains("Accept"))
                {
                    acceptPurchase = b;
                    acceptPurchase.onClick.RemoveAllListeners();
                    acceptPurchase.onClick.AddListener(() => { foodCourt.Transaction(); acceptPurchase.onClick.RemoveAllListeners();  tempDialogueUpgrade.SetActive(false); });
                }

                //Clean and Add Listeners for Cancel
                if (b.gameObject.name.Contains("Cancel"))
                {
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

    public void BuySuppliesFoodCourt(){
        RefugeeCampGameManager.Instance.UpdateFoodResource("+", 1); //1 unit of food
        RefugeeCampGameManager.Instance.UpdateMoney("-", 40); //1 unit of food costs 40 coins of the currency
    }

    public void BuySupplies25UnitsFoodCourt()
    {
        RefugeeCampGameManager.Instance.UpdateFoodResource("+", 25); //1 unit of food
        RefugeeCampGameManager.Instance.UpdateMoney("-", 25 * 40); //1 unit of food costs 40 coins of the currency 
    }

    public void BuySupplies50UnitsFoodCourt()
    {
        RefugeeCampGameManager.Instance.UpdateFoodResource("+", 50); //50 units of food 
        RefugeeCampGameManager.Instance.UpdateMoney("-", 50 * 40); //1 unit of food costs 40 coins of the currency 
    }


}

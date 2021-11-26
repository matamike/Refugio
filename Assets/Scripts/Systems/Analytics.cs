using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Analytics : MonoBehaviour{

    //Singleton GameManager
    private static Analytics instance;
    public static Analytics Instance { get { return instance; } }

    // Text/Representation Variables public (GUI ONLY)

    // //////////////////////////////////////////////
    
    // Properties Holder (strings and properties)
    [SerializeField] public List<string> variablesNames = new List<string>();
    [SerializeField] public FieldInfo[] fields;

    //Tracked Properties
    [SerializeField] private float OverallHappiness; //DONE
    [SerializeField] private float MoneyLeft; //DONE
    [SerializeField] private int NumberOfTents; //DONE
    [SerializeField] private int NumberOfRefugees; //DONE
    [SerializeField] private int NumberOfFemalesAccepted; //DONE
    [SerializeField] private int NumberOfMenAccepted; //DONE
    [SerializeField] private int NumberOfChildrenAccepted; //DONE
    [SerializeField] private int NumberOfDonationsAccepted; //DONE
    [SerializeField] private int NumberOfDonationsRejected; //DONE
    [SerializeField] private int NumberOfRemainingMedicine; //DONE
    [SerializeField] private int NumberOfRemainingFood; //DONE
    [SerializeField] private int NumberOfRewardsSuccess;  //DONE
    [SerializeField] private int NumberOfRestrictionsFailed; //DONE
    [SerializeField] private int NumberOfPendingRewards; //DONE
    [SerializeField] private int NumberOfPendingRestrictions; //DONE
    // //////////////////////////////////////////////////

    void Start(){
        SingletonCheck();
        InitializeStats();
    }

    //Singleton
    private void SingletonCheck()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    //During the start of the game initialize all tracked variables
    private void InitializeStats(){
        EnlistAttributes();
    }
    
    //Final Gather of Attributes and Display them in one report card
    public void GenerateReport(){
        //Acquire Variables
        OverallHappiness = RefugeeCampGameManager.Instance.GetOverallHappiness();
        MoneyLeft = RefugeeCampGameManager.Instance.money;
        NumberOfTents = RefugeeCampGameManager.Instance.GetCurrentHousingFacilities();
        NumberOfRefugees = RefugeeCampGameManager.Instance.GetCurrentPopulation();
        NumberOfFemalesAccepted = RefugeeCampGameManager.Instance.womenAccepted;
        NumberOfMenAccepted = RefugeeCampGameManager.Instance.menAccepted;
        NumberOfChildrenAccepted = RefugeeCampGameManager.Instance.childrenAccepted;
        NumberOfRemainingFood = RefugeeCampGameManager.Instance.foodResourceNum;
        NumberOfRemainingMedicine = RefugeeCampGameManager.Instance.medicineResourceNum;
        NumberOfRewardsSuccess = AgreementsManager.Instance.completedRewardsCount;
        NumberOfRestrictionsFailed = AgreementsManager.Instance.completedRestrictionsCount;
        NumberOfPendingRestrictions = AgreementsManager.Instance.activeRestrictions.Count;
        NumberOfPendingRewards = AgreementsManager.Instance.activeRewards.Count;
        NumberOfDonationsAccepted = RefugeeCampGameManager.Instance.acceptedDonations;
        NumberOfDonationsRejected = RefugeeCampGameManager.Instance.rejectedDonations;

        //Assign them to their respective text fields
        //TODO PLACE TextMeshProUGUI holders for variables

        //Calculate Any Results and assign them to their respective text fields
        //TODO PLACE TextMeshProUGUI holders for calculated Variables (post analytics processes)
    }

    void EnlistAttributes(){
        //Initialize Only.
        System.Type classType = this.GetType();
        fields = classType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
        //Debug.Log("Found : " + fields.Length + " properties");
        foreach (var field in fields)
        {
            //Debug.Log(field);
            //Debug.Log(field.FieldType);
            //Numeric Type
            if (field.FieldType == typeof(int) || field.FieldType == typeof(float) || field.FieldType == typeof(double)){
                if (field.Name != variablesNames.ToString())
                {
                    variablesNames.Add(field.Name);
                    field.SetValue(this, 1);
                }
            }

            //Text Type
            if(field.FieldType == typeof(string) || field.FieldType == typeof(char)){
                if (field.Name != variablesNames.ToString()){
                    variablesNames.Add(field.Name);
                    field.SetValue(this, 'A');
                }
            }
        }
    }


    public void SetAttribute(string varName = default , int value = default){
        foreach(var f in fields){
            if (f.Name.Contains(varName) && f.FieldType == value.GetType()){
                f.SetValue(this, value);
            }
        }
    }

    public void SetAttribute(string varName = default, float value = default){
        foreach (var f in fields){
            if (f.Name.Contains(varName) && f.FieldType == value.GetType()){
                f.SetValue(this, value);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RefugeeStats : MonoBehaviour{
    public enum Health{Healthy, Sick, Injured, Disabled, Exhausted, Dead};
    public enum Hunger{Full, Hungry, Thirsty, Feeble, Malnutrition, Starving};
    public enum SocialStatus {Rich, Mediocre, Poor};
    public enum EmotionStatus {Happy, Sad, Distressed, Mourning, Depressed};

    //Home Status enums
    public enum HomeStatus {Homeless, Resident, Leaving};
    

    //Conditions
    public Hunger hunger;
    public Health health;
    public SocialStatus status;
    public EmotionStatus emotionStatus;

    //Home Status
    public HomeStatus homeStatus;
    public GameObject assignedHome = null;

    //Refugee Properties
    [SerializeField]
    public string RefugeeName { get; set; }
    [SerializeField]
    public string RefugeeSurname { get; set; }
    [SerializeField]
    public string RefugeeBirthplace { get; set; }
    [SerializeField]
    public string RefugeeProfession { get; set; }
    [SerializeField]
    public int RefugeeGender { get; set; } // 0: Female (F) , 1: Male (M)
    [SerializeField]
    public Vector3 RefugeeBirthDate { get; set; } // X (Day) , Y(Month) , Z(Year)

    public int familyMemberID = -1; // -1 = individual no Family. countFamiliesNumber = familyID

    public string refName = "";
    public string refSurname = "";
    public string refBirthPlace = "";
    public string refBirthProfession = "";
    public int refGender = default;
    public string refBirthDate = "";
    public int refFamilyMemberID = -1;

    void Awake(){
        InitStatsRefugee(); //Use upon Creation of an instance to randomize the incoming stats.
    }

    //USER DEFINED FUNCTIONS FOR STATS ONLY

    //Initialize Stats for the Refugee Spawning
    void InitStatsRefugee(){
        Random.InitState((int)(Random.value * 100 * Time.realtimeSinceStartup));
        hunger = (Hunger)Random.Range(0, System.Enum.GetNames(typeof(Hunger)).Length);
        Random.InitState((int)(Random.value * 100 * Time.realtimeSinceStartup));
        health = (Health)Random.Range(0, System.Enum.GetNames(typeof(Health)).Length - 1);
        Random.InitState((int)(Random.value * 100 * Time.realtimeSinceStartup));
        status = (SocialStatus)Random.Range(0, System.Enum.GetNames(typeof(SocialStatus)).Length);
        Random.InitState((int)(Random.value * 100 * Time.realtimeSinceStartup));
        emotionStatus = (EmotionStatus)Random.Range(0, System.Enum.GetNames(typeof(EmotionStatus)).Length);

        homeStatus = HomeStatus.Homeless; //Homeless upon arrival/Pending Allocation (if possible)
    }


    //Setters for Stats Properties
    public void SetHungerStatus(Hunger hungerStatus){
        hunger = hungerStatus;
        //Update Interactor UI for Agent
    }

    public void SetHealthStatus(Health healthStatus){
        health = healthStatus;
        //Update Interactor UI for Agent
    }

    public void SetSocialStatus(SocialStatus socialStatus){
        status = socialStatus;
        //Update Interactor UI for Agent
    }

    public void SetEmotionStatus(EmotionStatus emoStatus){
        emotionStatus = emoStatus;
    }

    public void SetHomeStatus(HomeStatus homeStatus, GameObject home){
        this.homeStatus = homeStatus; // change status
        assignedHome = home; //assign a home
    }
    //............................//

    //Getters for Stats Properties
    public Hunger GetHungerStatus(){
        return hunger;
    }

    public Health GetHealthStatus(){
        return health;
    }

    public SocialStatus GetSocialStatus(){
        return status;
    }

    public EmotionStatus GetEmotionStatus(){
        return emotionStatus;
    }

    public HomeStatus GetHomeStatus(){
        return homeStatus;
    }

    public GameObject GetHome(){
        return assignedHome;
    }

    public string GetConditionsText() {
        string conditions = "";

        if (!GetHealthStatus().Equals(Health.Healthy)) conditions += GetHealthStatus().ToString() + ',';
        if (!GetHungerStatus().Equals(Hunger.Full)) conditions += GetHungerStatus().ToString() + ',';
        if (!GetEmotionStatus().Equals(EmotionStatus.Happy)) conditions += GetEmotionStatus().ToString() + '|';

        //trim  , or | excess in the end of the string
        char[] conditionsArr = conditions.ToCharArray();
        if (conditionsArr.Length > 0){
            if (conditionsArr[conditions.Length - 1] == '|' || conditionsArr[conditions.Length - 1] == ',')
                conditionsArr[conditions.Length - 1] = ' ';
        }
        conditions = new string(conditionsArr); 

        return conditions;
    }
    //............................//

    //DEBUG
    public void ExposeRefugeeStats(){
        refBirthDate = RefugeeBirthDate.ToString();
        refBirthPlace = RefugeeBirthplace;
        refBirthProfession = RefugeeProfession;
        refGender = RefugeeGender;
        refFamilyMemberID = familyMemberID;
        refName = RefugeeName;
        refSurname = RefugeeSurname;
    }

    public void DebugRefugeeIDProperties(){
        print("Name: " + RefugeeName);
        print("Surname: " + RefugeeSurname);
        print("Gender: " + RefugeeGender);
        print("Birth Date: " + ((int)RefugeeBirthDate.x).ToString("00") + " " + ((int)RefugeeBirthDate.y).ToString("00") + " " + (int)RefugeeBirthDate.z);
        print("Birth Place: " + RefugeeBirthplace);
        print("Profession: " + RefugeeProfession);
    }
}

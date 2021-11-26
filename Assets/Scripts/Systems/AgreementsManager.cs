using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class AgreementsManager : MonoBehaviour{

    //Singleton GameManager
    private static AgreementsManager instance;
    public static AgreementsManager Instance { get { return instance; } }
    // ////////////////////

    public Reward[] rewards;//Rewards Pool (Can update if anything is added or missing outside of play mode automatically)
    public Restriction[] restrictions;//Restrictions Pool (Can update if anything is added or missing outside of play mode automatically)

    //Active Restrictions
    public List<Restriction> activeRestrictions = new List<Restriction>();
    public List<Restriction> awaitingAllocationRestrictions = new List<Restriction>();
    public List<Restriction> completedRestrictions = new List<Restriction>();
    public int completedRestrictionsCount = 0;

    //Active Rewards
    public List<Reward> activeRewards = new List<Reward>();
    public List<Reward> awaitingAllocationRewards = new List<Reward>();
    public List<Reward> completedRewards = new List<Reward>();
    public int completedRewardsCount = 0;

    private List<string> donators = new List<string>(); //Donator Entities
    private List<string> unListedDonators = new List<string>(); //Not updated Donator Entities

    public string[] exposedDonators; //DEBUG 
    public string[] exposedUnlistedDonators; //DEBUG

    private int  restrictionMaxPenalties = 3; //General guideline for threshold in restrictions (strikes)

    //Agreement Book GameObject
    public GameObject AgreementBook; //The whole GO for Agreements (Index, Map, Agreements)

    //Index Tab Variables
    public GameObject indexTab; //holder of Donator entities in Index
    int donatorIndexChildCount; // number of donators in list
    public GameObject donatorIndexPrefab; //donator index prefab
    
    //Agreements Tab Variables
    public GameObject agreementsTab; //holder of Donator agreements in Agreements
    public GameObject donatorAgreementPrefab; //agreement pefab



    //generic use
    char bulletListCharacter = '\u2022'; //bullet list character
    public Color rewardColor;
    // //////////////////////////////////

    [SerializeField]
    List<GameObject> incomingAgentsList = new List<GameObject>(); //List of Agents that are going to checked upon admission for meeting requirements or causing agreements violation.

    [SerializeField]
    List<Reward> updatedRewards = new List<Reward>(); //List of Rewards subject to be updated
    [SerializeField]
    List<Restriction> updatedRestrictions = new List<Restriction>(); //List of Restrictions subject to be updated


    void Awake(){
        //AgreementsManager Tasks
        //DontDestroyOnLoad(gameObject); //preserve Manager LifeTime
        SingletonCheck(); //Singleton
    }


    void Start(){
        donatorIndexChildCount = indexTab.transform.childCount; //init child count for Index
        InitRewards(); //Form the Messages for Rewards
        InitRestrictions(); //Form the Messages for Restrictions
    }

    void Update(){
        //Update Donators Index Tab with newly added Entities if any
        if(indexTab.activeInHierarchy && unListedDonators.Count > 0) UpdateIndex("");
        
        UpdateAgreements(); //Update internally the score from incoming agents if match is hit.
        UpdateAgreementsUI(); //Add new or pending agreements | Update existing and awaiting to be added agreements

        CheckAgreementCompletion(); //Check for an Agreement being Completed
        RemoveCompletedAgreements(); //Remove Completed Agreements from Active and Awaiting lists.
    }


    private void OnDisable(){
        //UpdateAgreements(); //Update internally the score from incoming agents if match is hit.
        //UpdateAgreementsUI(); //Add new or pending agreements | Update existing and awaiting to be added agreements
    }

    //Donators Functions

    //Add a new Donator Entity in the Index tab
    public void AddDonator(string donator){
        if (donators.Contains(donator) || unListedDonators.Contains(donator)) return;
        //Index Check
        if (indexTab.activeInHierarchy)
        {
            donators.Add(donator);
            exposedDonators = donators.ToArray(); //DEBUG
            UpdateIndex(donator); //Update Donators
        }
        else{
            unListedDonators.Add(donator);
            exposedUnlistedDonators = unListedDonators.ToArray();  //DEBUG
        }
    }

    //Remove an existing Donator Entity (Technically used when we would want to remove a donator but that doesn't happen) (DEBUG ONLY)
    public void RemoveDonator(string donator){
        if (!donators.Contains(donator)) return;

        donators.Remove(donator);
        exposedDonators = donators.ToArray(); //DEBUG
    }

    //Reward Agreement
    async public void AddAgreement(Reward rew){
        bool isDone = false;

        GameObject x = Instantiate(donatorAgreementPrefab, agreementsTab.transform);
        x.name = rew.donationEntity;
        x.GetComponent<TextMeshProUGUI>().text = bulletListCharacter + rew.donationEntity;
        GameObject caveAt = x.transform.GetChild(0).gameObject;
        caveAt.GetComponent<TextMeshProUGUI>().text = rew.GetMessage();
        caveAt.GetComponent<TextMeshProUGUI>().color = rewardColor;
        GameObject progression = caveAt.transform.GetChild(0).gameObject;
        progression.GetComponent<TextMeshProUGUI>().text = "0/" + rew.entitiesAmount.ToString();
        progression.GetComponent<TextMeshProUGUI>().color = rewardColor;

        //Double Check
        if (!activeRewards.Contains(rew)) activeRewards.Add(rew);

        isDone = true;

        while (!isDone) await Task.Yield();
    }

    //Restriction Agreement
    async public void AddAgreement(Restriction res){
        bool isDone = false;

        GameObject x = Instantiate(donatorAgreementPrefab, agreementsTab.transform);
        x.name = res.donationEntity;
        x.GetComponent<TextMeshProUGUI>().text = bulletListCharacter + res.donationEntity;
        GameObject caveAt = x.transform.GetChild(0).gameObject;
        caveAt.GetComponent<TextMeshProUGUI>().text = res.GetMessage();
        caveAt.GetComponent<TextMeshProUGUI>().color = Color.red;
        GameObject progression = caveAt.transform.GetChild(0).gameObject;
        progression.GetComponent<TextMeshProUGUI>().text = "0/3";
        progression.GetComponent<TextMeshProUGUI>().color = Color.red;
        
        
        //Double Check
        if(!activeRestrictions.Contains(res))activeRestrictions.Add(res);

        isDone = true;

        while (!isDone) await Task.Yield();
    }

    //Add Pending Visualization Agreements upon UI Activation
    async void UpdateAgreements(){
        bool isDone = false;

        //Case Agreements awaiting to be added
        if (agreementsTab.activeInHierarchy && (awaitingAllocationRestrictions.Count > 0 || awaitingAllocationRewards.Count > 0)){

            //Reward Agreements
            foreach (Reward rew in awaitingAllocationRewards)
            {
                GameObject x = Instantiate(donatorAgreementPrefab, agreementsTab.transform);
                x.name = rew.donationEntity;
                x.GetComponent<TextMeshProUGUI>().text = bulletListCharacter + rew.donationEntity;
                GameObject caveAt = x.transform.GetChild(0).gameObject;
                caveAt.GetComponent<TextMeshProUGUI>().text = rew.GetMessage();
                GameObject progression = caveAt.transform.GetChild(0).gameObject;
                progression.GetComponent<TextMeshProUGUI>().text = "0/" + rew.entitiesAmount.ToString();

                //Recolor
                caveAt.GetComponent<TextMeshProUGUI>().color = rewardColor;
                progression.GetComponent<TextMeshProUGUI>().color = rewardColor;

                activeRewards.Add(rew);
            }

            //Restriction Agreements
            foreach (Restriction res in awaitingAllocationRestrictions)
            {
                GameObject x = Instantiate(donatorAgreementPrefab, agreementsTab.transform);
                x.name = res.donationEntity;
                x.GetComponent<TextMeshProUGUI>().text = bulletListCharacter + res.donationEntity;
                GameObject caveAt = x.transform.GetChild(0).gameObject;
                caveAt.GetComponent<TextMeshProUGUI>().text = res.GetMessage();
                GameObject progression = caveAt.transform.GetChild(0).gameObject;
                progression.GetComponent<TextMeshProUGUI>().text = "0/3";

                //Recolor
                caveAt.GetComponent<TextMeshProUGUI>().color = Color.red;
                progression.GetComponent<TextMeshProUGUI>().color = Color.red;

                activeRestrictions.Add(res);
            }

            //Reset Awaiting Lists for Agreements
            awaitingAllocationRestrictions.Clear(); //Empty awaiting restrictions
            awaitingAllocationRewards.Clear(); //Empty awaiting rewards           
        }
        isDone = true;

        while (!isDone) await Task.Yield();
        // //////////////////////////////////////////////////////////////////////////////////////
    }

    //Update Agreements (Progress/ Complete / Fail) - Apply Penalty or Rewards
    async void CheckAgreements(){
        bool isDone = false;

        //Case of existing CaveAts - Check if they need to be updated (Score) - Active/Inactive Tab Only 
        if (incomingAgentsList.Count != 0)
        {

           // Debug.Log("Checking Agents for Rewards and Restrictions");

            //Acquire agent entities / entity
            foreach (GameObject agent in incomingAgentsList)
            {
                RefugeeStats agentStats = agent.GetComponent<RefugeeStats>();
                agentStats.DebugRefugeeIDProperties();

                //Acquire current year to compare with birth year of agent
                int currentYear = RefugeeCampGameManager.Instance.UIClockController.GetYear();
                int birthYear = (int)agentStats.RefugeeBirthDate.z;
                int age = currentYear - birthYear;


                //Check Agents for Hitting any Restriction (Active)
                if (activeRestrictions.Count > 0)
                {
                    foreach (Restriction restr in activeRestrictions)
                    {
                        int restrCriteria = 0; //holds criteria count for each restriction and 
                        int agentMatchingCriteria = 0; //player is matching any of the criteria.

                        //Profession Restriction (PROBABLY WORKS)
                        if (restr.profession != "")
                        {
                            restrCriteria += 1;
                            if (agentStats.RefugeeProfession.Contains(restr.profession)) agentMatchingCriteria += 1;
                        }

                        //BirthPlace Restriction (PROBABLY WORKS)
                        if (restr.birthplace != "")
                        {
                            restrCriteria += 1;
                            if (agentStats.RefugeeBirthplace.Contains(restr.birthplace)) agentMatchingCriteria += 1;
                        }

                        //Age Restriction (WORKS)
                        if (restr.youngerThan)
                        {
                            restrCriteria += 1;
                            if (age < restr.age) agentMatchingCriteria += 1;
                        }
                        if (restr.olderThan)
                        {
                            restrCriteria += 1;
                            if (age > restr.age) agentMatchingCriteria += 1;
                        }
                        // //////////////////////////////////

                        //Entity Type Restriction (WORKS) - Family Fix
                        if (!restr.entityType.Equals(Restriction.RestrictionEntityType.None))
                        {
                            restrCriteria += 1;
                            if (restr.entityType.Equals(Restriction.RestrictionEntityType.Children) && age < 18) agentMatchingCriteria += 1;
                            if (restr.entityType.Equals(Restriction.RestrictionEntityType.Men) && agentStats.RefugeeGender == 0) agentMatchingCriteria += 1;
                            if (restr.entityType.Equals(Restriction.RestrictionEntityType.Women) && agentStats.RefugeeGender == 1) agentMatchingCriteria += 1;
                            if (restr.entityType.Equals(Restriction.RestrictionEntityType.People)) agentMatchingCriteria += 1;
                            //if (restr.entityType.Equals(Restriction.RestrictionEntityType.Families) && agentStats.familyMemberID != currentFamilyID && isFamilyAccepted)
                            //{
                            //  currentFamilyID = agentStats.familyMemberID;
                            //  agentMatchingCriteria += 1;
                            //}
                        }

                        //Entity Condition Restriction (WORKS)
                        if (!restr.entityCond.Equals(Restriction.EntityCondition.None))
                        {
                            restrCriteria += 1;
                            if (restr.entityCond.Equals(Restriction.EntityCondition.Dead) && agentStats.health.Equals(RefugeeStats.Health.Dead)) agentMatchingCriteria += 1;
                            if (restr.entityCond.Equals(Restriction.EntityCondition.Sick) && agentStats.health.Equals(RefugeeStats.Health.Sick)) agentMatchingCriteria += 1;
                            if (restr.entityCond.Equals(Restriction.EntityCondition.Healthy) && agentStats.health.Equals(RefugeeStats.Health.Healthy)) agentMatchingCriteria += 1;
                            if (restr.entityCond.Equals(Restriction.EntityCondition.Starving) && agentStats.health.Equals(RefugeeStats.Hunger.Starving)) agentMatchingCriteria += 1;
                            if (restr.entityCond.Equals(Restriction.EntityCondition.Hungry) && agentStats.health.Equals(RefugeeStats.Hunger.Hungry)) agentMatchingCriteria += 1;
                            if (restr.entityCond.Equals(Restriction.EntityCondition.Full) && agentStats.health.Equals(RefugeeStats.Hunger.Full)) agentMatchingCriteria += 1;
                            if (restr.entityCond.Equals(Restriction.EntityCondition.Depressed) && agentStats.emotionStatus.Equals(RefugeeStats.EmotionStatus.Depressed)) agentMatchingCriteria += 1;
                            if (restr.entityCond.Equals(Restriction.EntityCondition.Sad) && agentStats.emotionStatus.Equals(RefugeeStats.EmotionStatus.Sad)) agentMatchingCriteria += 1;
                            if (restr.entityCond.Equals(Restriction.EntityCondition.Happy) && agentStats.emotionStatus.Equals(RefugeeStats.EmotionStatus.Happy)) agentMatchingCriteria += 1;
                        }

                        //Check restriction critera count if they match agents hits.
                        if (restrCriteria == agentMatchingCriteria)
                        {
                            updatedRestrictions.Add(restr); //Once agent's criteria match restrictions apply penalty
                        }
                    }
                }

                //Check Agents for Hitting any Restriction (Awaiting)
                if (awaitingAllocationRestrictions.Count > 0)
                {
                    foreach (Restriction restr in awaitingAllocationRestrictions)
                    {
                        int restrCriteria = 0; //holds criteria count for each restriction and 
                        int agentMatchingCriteria = 0; //player is matching any of the criteria.

                        //Profession Restriction (WORKS)
                        if (restr.profession != "")
                        {
                            restrCriteria += 1;
                            if (agentStats.RefugeeProfession.Contains(restr.profession)) agentMatchingCriteria += 1;
                        }

                        //BirthPlace Restriction (WORKS)
                        if (restr.birthplace != "")
                        {
                            restrCriteria += 1;
                            if (agentStats.RefugeeBirthplace.Contains(restr.birthplace)) agentMatchingCriteria += 1;
                        }

                        //Age Restriction (WORKS)
                        if (restr.youngerThan)
                        {
                            restrCriteria += 1;
                            if (age < restr.age) agentMatchingCriteria += 1;
                        }
                        if (restr.olderThan)
                        {
                            restrCriteria += 1;
                            if (age > restr.age) agentMatchingCriteria += 1;
                        }
                        // //////////////////////////////////

                        //Entity Type Restriction (WORKS) - Family Fix
                        if (!restr.entityType.Equals(Restriction.RestrictionEntityType.None))
                        {
                            restrCriteria += 1;
                            if (restr.entityType.Equals(Restriction.RestrictionEntityType.Children) && age < 18) agentMatchingCriteria += 1;
                            if (restr.entityType.Equals(Restriction.RestrictionEntityType.Men) && agentStats.RefugeeGender == 0) agentMatchingCriteria += 1;
                            if (restr.entityType.Equals(Restriction.RestrictionEntityType.Women) && agentStats.RefugeeGender == 1) agentMatchingCriteria += 1;
                            if (restr.entityType.Equals(Restriction.RestrictionEntityType.People)) agentMatchingCriteria += 1;
                            //if (restr.entityType.Equals(Restriction.RestrictionEntityType.Families) && agentStats.familyMemberID != currentFamilyID && isFamilyAccepted)
                            //{
                            //  currentFamilyID = agentStats.familyMemberID;
                            //  agentMatchingCriteria += 1;
                            //}
                        }

                        //Entity Condition Restriction (WORKS)
                        if (!restr.entityCond.Equals(Restriction.EntityCondition.None))
                        {
                            restrCriteria += 1;
                            if (restr.entityCond.Equals(Restriction.EntityCondition.Dead) && agentStats.health.Equals(RefugeeStats.Health.Dead)) agentMatchingCriteria += 1;
                            if (restr.entityCond.Equals(Restriction.EntityCondition.Sick) && agentStats.health.Equals(RefugeeStats.Health.Sick)) agentMatchingCriteria += 1;
                            if (restr.entityCond.Equals(Restriction.EntityCondition.Healthy) && agentStats.health.Equals(RefugeeStats.Health.Healthy)) agentMatchingCriteria += 1;
                            if (restr.entityCond.Equals(Restriction.EntityCondition.Starving) && agentStats.health.Equals(RefugeeStats.Hunger.Starving)) agentMatchingCriteria += 1;
                            if (restr.entityCond.Equals(Restriction.EntityCondition.Hungry) && agentStats.health.Equals(RefugeeStats.Hunger.Hungry)) agentMatchingCriteria += 1;
                            if (restr.entityCond.Equals(Restriction.EntityCondition.Full) && agentStats.health.Equals(RefugeeStats.Hunger.Full)) agentMatchingCriteria += 1;
                            if (restr.entityCond.Equals(Restriction.EntityCondition.Depressed) && agentStats.emotionStatus.Equals(RefugeeStats.EmotionStatus.Depressed)) agentMatchingCriteria += 1;
                            if (restr.entityCond.Equals(Restriction.EntityCondition.Sad) && agentStats.emotionStatus.Equals(RefugeeStats.EmotionStatus.Sad)) agentMatchingCriteria += 1;
                            if (restr.entityCond.Equals(Restriction.EntityCondition.Happy) && agentStats.emotionStatus.Equals(RefugeeStats.EmotionStatus.Happy)) agentMatchingCriteria += 1;
                        }

                        //Check restriction critera count if they match agents hits.
                        if (restrCriteria == agentMatchingCriteria)
                        {
                            updatedRestrictions.Add(restr); //Once agent's criteria match restrictions apply penalty
                        }
                    }
                }

                //Reset Family ID
                //currentFamilyID = -1;

                //Check Agents for Hitting any Reward (Active)
                if (activeRewards.Count > 0)
                {
                    foreach (Reward rew in activeRewards)
                    {
                        int rewCriteria = 0; //holds criteria count for each restriction and 
                        int agentMatchingCriteria = 0; //player is matching any of the criteria.

                        //Profession Reward (PROBABLY WORKS)
                        if (rew.profession != "")
                        {
                            rewCriteria += 1;
                            if (agentStats.RefugeeProfession.Contains(rew.profession)) agentMatchingCriteria += 1;
                        }
                        //BirthPlace Reward (PROBABLY WORKS)
                        if (rew.birthplace != "")
                        {
                            rewCriteria += 1;
                            if (agentStats.RefugeeBirthplace.Contains(rew.birthplace)) agentMatchingCriteria += 1;
                        }

                        //Entity Type Reward (WORKS) - Family Fix
                        if (!rew.entityType.Equals(Reward.RewardEntityType.None))
                        {
                            rewCriteria += 1;
                            if (rew.entityType.Equals(Reward.RewardEntityType.Children) && age < 18) agentMatchingCriteria += 1;
                            if (rew.entityType.Equals(Reward.RewardEntityType.Men) && agentStats.RefugeeGender == 0) agentMatchingCriteria += 1;
                            if (rew.entityType.Equals(Reward.RewardEntityType.Women) && agentStats.RefugeeGender == 1) agentMatchingCriteria += 1;
                            if (rew.entityType.Equals(Reward.RewardEntityType.People)) agentMatchingCriteria += 1;
                            //if (rew.entityType.Equals(Reward.RewardEntityType.Families) && agentStats.familyMemberID != currentFamilyID && isFamilyAccepted)
                            //{
                            //  currentFamilyID = agentStats.familyMemberID;
                            //  agentMatchingCriteria += 1;
                            //}
                        }

                        //Check reward criteria count if they match agents hits.
                        if (rewCriteria == agentMatchingCriteria)
                        {
                            updatedRewards.Add(rew); //Once agent's criteria match restrictions apply penalty
                        }
                    }
                }

                //Check Agents for Hitting any Reward (Awaiting)
                if (awaitingAllocationRewards.Count > 0)
                {
                    foreach (Reward rew in awaitingAllocationRewards)
                    {
                        int rewCriteria = 0; //holds criteria count for each restriction and 
                        int agentMatchingCriteria = 0; //player is matching any of the criteria.

                        //Profession Reward (PROBABLY WORKS)
                        if (rew.profession != "")
                        {
                            rewCriteria += 1;
                            if (agentStats.RefugeeProfession.Equals(rew.profession)) agentMatchingCriteria += 1;
                        }
                        //BirthPlace Reward (PROBABLY WORKS)
                        if (rew.birthplace != "")
                        {
                            rewCriteria += 1;
                            if (agentStats.RefugeeBirthplace.Equals(rew.birthplace)) agentMatchingCriteria += 1;
                        }

                        //Entity Type Reward (WORKS) - Family Fix
                        if (!rew.entityType.Equals(Reward.RewardEntityType.None))
                        {
                            rewCriteria += 1;
                            if (rew.entityType.Equals(Reward.RewardEntityType.Children) && age < 18) agentMatchingCriteria += 1;
                            if (rew.entityType.Equals(Reward.RewardEntityType.Men) && agentStats.RefugeeGender == 0) agentMatchingCriteria += 1;
                            if (rew.entityType.Equals(Reward.RewardEntityType.Women) && agentStats.RefugeeGender == 1) agentMatchingCriteria += 1;
                            if (rew.entityType.Equals(Reward.RewardEntityType.People)) agentMatchingCriteria += 1;
                            //if (rew.entityType.Equals(Reward.RewardEntityType.Families) && agentStats.familyMemberID != currentFamilyID && isFamilyAccepted)
                            //{
                            //  currentFamilyID = agentStats.familyMemberID;
                            //  agentMatchingCriteria += 1;
                            //}
                        }

                        //Check reward criteria count if they match agents hits.
                        if (rewCriteria == agentMatchingCriteria)
                        {
                            updatedRewards.Add(rew); //Once agent's criteria match restrictions apply penalty
                        }
                    }
                }

                //Reset Family ID
                //currentFamilyID = -1;
            }
            // /////////////////////////////////////////////////////////////////////////////////////////

           // Debug.Log("Rewards progressing : " + updatedRewards.Count);
          //  Debug.Log("Restrictions progressing : " + updatedRestrictions.Count);

            //Update Scores internally in active and awaiting agreements (independent of UI) before clearing

            //Rewards
            foreach (Reward r in updatedRewards)
            {
                if (activeRewards.Contains(r))
                {
                    int id = activeRewards.FindInstanceID(r);
                    activeRewards[id].score += 1; //update score

                    //Apply reward if an agreement is completed (X/X)
                    if (activeRewards[id].score == activeRewards[id].entitiesAmount)
                    {
                        NotificationManager.Instance.CreateStandaloneNotification("Completed : " + activeRewards[id].GetMessage().Trim() + "\n"
                                                      + activeRewards[id].donationEntity.Trim() + " will provide double rewards.", activeRewards[id].GetType().ToString());
                        if (!RefugeeCampGameManager.Instance.rewardingDonators.Contains(activeRewards[id].donationEntity))
                        {
                            RefugeeCampGameManager.Instance.rewardingDonators.Add(activeRewards[id].donationEntity);
                        }
                        if (RefugeeCampGameManager.Instance.blackListDonators.Contains(activeRewards[id].donationEntity))
                            RefugeeCampGameManager.Instance.blackListDonators.Remove(activeRewards[id].donationEntity);
                    }
                }
                if (awaitingAllocationRewards.Contains(r))
                {
                    int id = awaitingAllocationRewards.FindInstanceID(r);
                    awaitingAllocationRewards[id].score += 1; //update score

                    //Apply reward if an agreement is completed (X/X)
                    if (awaitingAllocationRewards[id].score == awaitingAllocationRewards[id].entitiesAmount)
                    {
                        NotificationManager.Instance.CreateStandaloneNotification("Completed : " + awaitingAllocationRewards[id].GetMessage().Trim() + "\n"
                                                    + awaitingAllocationRewards[id].donationEntity.Trim() + " will provide double donation amounts.", awaitingAllocationRewards[id].GetType().ToString());
                        if (!RefugeeCampGameManager.Instance.rewardingDonators.Contains(awaitingAllocationRewards[id].donationEntity))
                        {
                            RefugeeCampGameManager.Instance.rewardingDonators.Add(awaitingAllocationRewards[id].donationEntity);
                        }
                        if (RefugeeCampGameManager.Instance.blackListDonators.Contains(awaitingAllocationRewards[id].donationEntity))
                            RefugeeCampGameManager.Instance.blackListDonators.Remove(awaitingAllocationRewards[id].donationEntity);
                    }
                }
            }

            //Restrictions
            foreach (Restriction s in updatedRestrictions)
            {
                if (activeRestrictions.Contains(s))
                {
                    int id = activeRestrictions.FindInstanceID(s);
                    activeRestrictions[id].score += 1; //update score
                    //Apply penalty if an agreement is violated (3/3)
                    if (activeRestrictions[id].score == restrictionMaxPenalties)
                    {
                        NotificationManager.Instance.CreateStandaloneNotification("Failed : " +activeRestrictions[id].GetMessage().Trim() + "\n"
                                                    + " Penalty will be applied from " + activeRestrictions[id].donationEntity.Trim() + "\n" + " (Donation amount /2)", activeRestrictions[id].GetType().ToString());
                        if (!RefugeeCampGameManager.Instance.blackListDonators.Contains(activeRestrictions[id].donationEntity))
                        {
                            RefugeeCampGameManager.Instance.blackListDonators.Add(activeRestrictions[id].donationEntity);
                        }
                        if (!RefugeeCampGameManager.Instance.rewardingDonators.Contains(activeRestrictions[id].donationEntity))
                        {
                            RefugeeCampGameManager.Instance.rewardingDonators.Remove(activeRestrictions[id].donationEntity);
                        }
                    }
                }
                if (awaitingAllocationRestrictions.Contains(s))
                {
                    int id = awaitingAllocationRestrictions.FindInstanceID(s);
                    awaitingAllocationRestrictions[id].score += 1; //update score
                    //Apply penalty if an agreement is violated (3/3)
                    if (awaitingAllocationRestrictions[id].score == restrictionMaxPenalties)
                    {
                        NotificationManager.Instance.CreateStandaloneNotification("Failed : " + awaitingAllocationRestrictions[id].GetMessage().Trim() + "\n"
                                                    + " Penalty will be applied from " + awaitingAllocationRestrictions[id].donationEntity.Trim() + "\n" + " (Donation amount /2)", awaitingAllocationRestrictions[id].GetType().ToString());
                        if (!RefugeeCampGameManager.Instance.blackListDonators.Contains(awaitingAllocationRestrictions[id].donationEntity))
                        {
                            RefugeeCampGameManager.Instance.blackListDonators.Add(awaitingAllocationRestrictions[id].donationEntity);
                        }
                        if (!RefugeeCampGameManager.Instance.rewardingDonators.Contains(awaitingAllocationRestrictions[id].donationEntity))
                        {
                            RefugeeCampGameManager.Instance.rewardingDonators.Remove(awaitingAllocationRestrictions[id].donationEntity);
                        }
                    }
                }
            }

            //Clear all incoming info about agents and caveats once they are updated in the UI otherwise store them
            updatedRewards.Clear(); //Debug Clear
            updatedRestrictions.Clear();//Debug Clear
            incomingAgentsList.Clear(); //clear any incoming agents            
            isDone = true;
        }
        else isDone = true;
     
        while (!isDone) await Task.Yield();     
    }

    //GUI Agreements
    public async void UpdateAgreementsUI(){
        bool isDone = false;

        if (agreementsTab.activeInHierarchy){
            int agreementsCount = agreementsTab.transform.childCount; // get number of active agreements

            //Case - Active Rewards
            for (int i = 0; i < agreementsCount; i++){
                
                //Acquire Agreement GO Composition
                GameObject agreementEntry = agreementsTab.transform.GetChild(i).gameObject; //Donation Entity (Donator)
                GameObject caveAt = agreementEntry.transform.GetChild(0).gameObject; //Donation Agreement (CaveAt)
                GameObject score = caveAt.transform.GetChild(0).gameObject; //Progression

                //Active Child State (maybe add more filters to make sure completed ones are not affected) 
                if (agreementEntry.activeSelf || agreementEntry.GetComponent<TextMeshProUGUI>().color != Color.gray){
                    string caveAtTxt = caveAt.GetComponent<TextMeshProUGUI>().text.ToString();
                    string scoreTxt =  score.GetComponent<TextMeshProUGUI>().text.ToString();

                    foreach (Reward reward in activeRewards){
                        if (caveAtTxt.Contains(reward.GetMessage()) && caveAt.GetComponent<TextMeshProUGUI>().color != Color.gray){
                            scoreTxt = reward.score.ToString() + "/" + reward.entitiesAmount; //update score
                            score.GetComponent<TextMeshProUGUI>().text = scoreTxt;
                            if (reward.score >= reward.entitiesAmount){
                                //Recolor
                                agreementEntry.GetComponent<TextMeshProUGUI>().color = Color.gray;
                                caveAt.GetComponent<TextMeshProUGUI>().color = Color.gray;
                                score.GetComponent<TextMeshProUGUI>().color = Color.gray;
                                //Cross Line as Checked. (Line Overlay)
                                agreementEntry.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough;
                                caveAt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough; 
                                score.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough;
                            }
                        }
                    }

                    foreach (Reward reward in awaitingAllocationRewards){
                        if (caveAtTxt.Contains(reward.GetMessage()) && caveAt.GetComponent<TextMeshProUGUI>().color != Color.gray){
                            scoreTxt = reward.score.ToString() + "/" + reward.entitiesAmount; //update score
                            score.GetComponent<TextMeshProUGUI>().text = scoreTxt;
                            if (reward.score >= reward.entitiesAmount)
                            {
                                //Recolor
                                agreementEntry.GetComponent<TextMeshProUGUI>().color = Color.gray;
                                caveAt.GetComponent<TextMeshProUGUI>().color = Color.gray;
                                score.GetComponent<TextMeshProUGUI>().color = Color.gray;
                                //Cross Line as Checked. (Line Overlay)
                                agreementEntry.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough;
                                caveAt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough;
                                score.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough;
                            }
                        }
                    }

                    foreach (Reward reward in completedRewards)
                    {
                        if (caveAtTxt.Contains(reward.GetMessage()) && caveAt.GetComponent<TextMeshProUGUI>().color != Color.gray)
                        {
                            scoreTxt = reward.score.ToString() + "/" + reward.entitiesAmount; //update score
                            score.GetComponent<TextMeshProUGUI>().text = scoreTxt;
                            if (reward.score >= reward.entitiesAmount)
                            {
                                //Recolor
                                agreementEntry.GetComponent<TextMeshProUGUI>().color = Color.gray;
                                caveAt.GetComponent<TextMeshProUGUI>().color = Color.gray;
                                score.GetComponent<TextMeshProUGUI>().color = Color.gray;
                                //Cross Line as Checked. (Line Overlay)
                                agreementEntry.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough;
                                caveAt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough;
                                score.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough;
                            }
                        }
                    }

                    foreach (Restriction restriction in activeRestrictions){
                        if (caveAtTxt.Contains(restriction.GetMessage()) && caveAt.GetComponent<TextMeshProUGUI>().color != Color.gray)
                        {
                            scoreTxt = restriction.score + "/" + restrictionMaxPenalties.ToString(); //update score
                            score.GetComponent<TextMeshProUGUI>().text = scoreTxt;
                            if (restriction.score >= 3){
                                //Recolor
                                agreementEntry.GetComponent<TextMeshProUGUI>().color = Color.gray;
                                caveAt.GetComponent<TextMeshProUGUI>().color = Color.gray;
                                score.GetComponent<TextMeshProUGUI>().color = Color.gray;
                                //Cross Line as Checked. (Line Overlay)
                                agreementEntry.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough;
                                caveAt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough;
                                score.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough;
                            }
                        }
                    }

                    foreach (Restriction restriction in awaitingAllocationRestrictions){
                        if (caveAtTxt.Contains(restriction.GetMessage()) && caveAt.GetComponent<TextMeshProUGUI>().color != Color.gray)
                        {
                            scoreTxt = restriction.score + "/" + restrictionMaxPenalties.ToString(); //update score
                            score.GetComponent<TextMeshProUGUI>().text = scoreTxt;
                            if (restriction.score >= 3){
                                //Recolor
                                agreementEntry.GetComponent<TextMeshProUGUI>().color = Color.gray;
                                caveAt.GetComponent<TextMeshProUGUI>().color = Color.gray;
                                score.GetComponent<TextMeshProUGUI>().color = Color.gray;
                                //Cross Line as Checked. (Line Overlay)
                                agreementEntry.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough;
                                caveAt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough;
                                score.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough;
                            }
                        }
                    }

                    foreach (Restriction restriction in completedRestrictions)
                    {
                        if (caveAtTxt.Contains(restriction.GetMessage()) && caveAt.GetComponent<TextMeshProUGUI>().color != Color.gray)
                        {
                            scoreTxt = restriction.score + "/" + restrictionMaxPenalties.ToString(); //update score
                            score.GetComponent<TextMeshProUGUI>().text = scoreTxt;
                            if (restriction.score >= 3)
                            {
                                //Recolor
                                agreementEntry.GetComponent<TextMeshProUGUI>().color = Color.gray;
                                caveAt.GetComponent<TextMeshProUGUI>().color = Color.gray;
                                score.GetComponent<TextMeshProUGUI>().color = Color.gray;
                                //Cross Line as Checked. (Line Overlay)
                                agreementEntry.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough;
                                caveAt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough;
                                score.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough;
                            }
                        }
                    }

                    //ClearCompletedAgreement();
                }
            }
            isDone = true;
        }else isDone = true;

        while (!isDone) await Task.Yield();
    }

    //Assigns Completed Agreements to Completed
    void CheckAgreementCompletion(){
        //Check for Complete Restrictions
        foreach (Restriction res in activeRestrictions) {
            if(res.score >= 3)
                if (!completedRestrictions.Contains(res)) completedRestrictions.Add(res); 
        }

        foreach (Restriction res in awaitingAllocationRestrictions){
            if (res.score >= 3)
                if(!completedRestrictions.Contains(res)) completedRestrictions.Add(res);
        }


        //Check for Complete Rewards
        foreach (Reward rew in activeRewards){
            if (rew.score >= rew.entitiesAmount) completedRewards.Add(rew);
        }

        foreach (Reward rew in awaitingAllocationRewards){
            if (rew.score >= rew.entitiesAmount) completedRewards.Add(rew);
        }
    }

    //Remove Completed Agreements from Active or Awaiting
    void RemoveCompletedAgreements(){
        //Remove from Active or Awaiting Rewards completed one.
        foreach(Reward r in completedRewards){
            if (activeRewards.Contains(r)) activeRewards.Remove(r);
            if (awaitingAllocationRewards.Contains(r)) awaitingAllocationRewards.Remove(r);
        }

        //Remove from Active or Awaiting Rewards completed one.
        foreach (Restriction restr in completedRestrictions){
            if (activeRestrictions.Contains(restr)) activeRestrictions.Remove(restr);
            if (awaitingAllocationRestrictions.Contains(restr)) awaitingAllocationRestrictions.Remove(restr);
        }
    }


    public void ClearCompletedAgreement(){
        //Flush Completed Restrictions and Rewards.
        if (completedRestrictions.Count > 0) completedRestrictionsCount += completedRestrictions.Count;
        if (completedRewards.Count > 0) completedRewardsCount += completedRewards.Count;

        completedRestrictions.Clear();
        completedRewards.Clear();
    }

    //GUI Index
    void UpdateIndex(string donator){
        //Case of Inactive Tab
       if(unListedDonators.Count > 0){
            //Donation Entity Index Generation
            foreach (string s in unListedDonators)
            {
                GameObject x = Instantiate(donatorIndexPrefab, indexTab.transform);
                x.name = s;
                x.GetComponent<TextMeshProUGUI>().text = bulletListCharacter + s;

                donators.Add(s); //add to primary list
                exposedDonators = donators.ToArray(); //DEBUG
            }
            unListedDonators.Clear(); //remove from temp
            exposedUnlistedDonators = unListedDonators.ToArray();
        }

        //Case of Active Tab
        if (indexTab.activeInHierarchy && unListedDonators.Count == 0){
            donatorIndexChildCount = indexTab.transform.childCount;
            if (donatorIndexChildCount != donators.Count){
                GameObject x = Instantiate(donatorIndexPrefab, indexTab.transform);
                x.name = donator;
                x.GetComponent<TextMeshProUGUI>().text = bulletListCharacter + donator;
            }
        }
    }

    //Reward Functions
    void InitRewards(){
        foreach (Reward reward in rewards) reward.AnalyzeMessage();
    }

    //Restriction Functions
    void InitRestrictions(){
        foreach (Restriction restr in restrictions) restr.AnalyzeMessage();
    }

    //public void SetIncomingFamilyMemberSize(int familyMembersCount)
    //{
      //  familyMembersCounter = familyMembersCount;
    //}
    public void SetIncomingAgentsEntities(List<GameObject> agents, bool isFamily=false){
        //if(isFamily){
          //  if (familyMembersCounter > 0) familyMembersCounter -= agents.Count;
            
          //  if (familyMembersCounter == 0) isFamilyAccepted = true;
          //  else isFamilyAccepted = false;
        //} 

        if (incomingAgentsList.Count > 0){
            foreach (GameObject x in agents){
                if (!incomingAgentsList.Contains(x)) incomingAgentsList.Add(x);
            }
        }
        else incomingAgentsList = agents;

        CheckAgreements();
    }
    public void SetIncomingAgentEntity(GameObject agent, bool isFamily = false){
        //if (isFamily){
          //  if (familyMembersCounter > 0) familyMembersCounter -= 1;

          //  if (familyMembersCounter == 0) isFamilyAccepted = true;
          //  else isFamilyAccepted = false;
        //}

        incomingAgentsList.Add(agent);
        CheckAgreements();
    }

    //Singleton
    private void SingletonCheck(){
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }
}

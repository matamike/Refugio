using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class RefugeeCampGameManager : MonoBehaviour {
    //Refugees and Structures
    public GameObject actionListActiveGO = null;

    //Analytics Tracking
    public int acceptedDonations = 0; //used
    public int rejectedDonations = 0; //used
    public int totalDonationMoneyAcquired = 0;
    public int totalExpenses = 0;
    public int menAccepted = 0; //used
    public int womenAccepted = 0; //used
    public int childrenAccepted = 0; //used

    //Game Time Member
    public struct GameTime {
        private float _secs;
        public ushort Hours {
            get => (ushort) (_secs / 3600);
            set => _secs = _secs % 3600 + value * 3600;
        }

        public ushort Minutes {
            get => (ushort) (_secs % 3600 / 60);
            set => _secs = Hours * 3600 + value * 60 + Seconds;
        }

        public float Seconds {
            get => _secs % 60;
            set => _secs = ((ushort) _secs / 60) * 60 + value;
        }

        public float TotalSeconds {
            get => _secs;
        }

        public GameTime(ushort secs) {
            _secs = secs;
        }
        
        public GameTime(ushort hours, ushort minutes, ushort seconds) {
            _secs = hours * 3600 + minutes * 60 + seconds;
        }
        
        public static GameTime operator +(GameTime gameTime, float deltaSecs) {
            gameTime._secs += deltaSecs;
            return gameTime;
        } 
    }

    //Singleton GameManager
    private static RefugeeCampGameManager instance;
    public static RefugeeCampGameManager Instance { get { return instance; } }

    //Starting year
    private int startingYear; //sets the starting year according to UI Controller
    public int endingYear; //set the ending year
    public float gameOverMoneyCriteria; //target threshold for money to fail the game.
    public GameObject gameoverGO;
    public Button returnToMainMenuButton, quitGame;

    //Time related variables
    private float timestamp = 0.0f;
    private float populationCheckTimeStamp = 0.0f;
    private float updateOverallHappinessTimestamp = 0.0f;
    public float spawnTimeOffset;

    [Range(0, 2)] //[Range(0,1)]
    public float tSpeedVar = 1;
    public float targetSpeedModifier = 1;

    //public float Timespeed { get { return Timespeed; } set { Timespeed = tSpeedVar; } } //Used to simulate pause time without affecting in engine time

    //Game Clock
    public GameTime gameTime;
    public UIClockController UIClockController;

    //Properties to Save and re use
    private GameObject actionsListGO; //Structure GO for Interactions
    private GameObject agentActionsListGO; //Agent GO for Interactions
    public GameObject agentHealthSliderGO; //Agent GO for Health Visualization
    public GameObject agentHungerSliderGO; //Agent GO for Hunger Visualization
    public GameObject agentHappinessSliderGO; //Agent GO for Happiness Visualization
    private GameObject waitingAreaPopupAgentSelectionListGO; //GO for allocating Agents from the waiting Area
    private TextMeshProUGUI populationResourceValueText;
    private TextMeshProUGUI moneyResourceValueText;
    private TextMeshProUGUI medicineResourceValueText;
    private TextMeshProUGUI foodResourcesValueText;

    public int communityMaxSize = default;
    public int medicineResourceNum = 2; //60
    public int foodResourceNum = 2; //60
    public float money = default;

    //Happiness Variables
    public float overallHappiness = default;
    public Image overallHappinessSlider;
    // /////////////////

    //Agreements and Rules
    private GameObject agreementsRulesGO;

    //Donations Properties
    private GameObject incomingDonationAreaGO; 
    private GameObject donationListGO;
    private GameObject donationDialogueGO;
    private TextMeshProUGUI donationButtonGOText,donationDialogueGOText,donationDialogueAgreementGOText;
    private Button donationAcceptButton, donationRejectButton;
    public Button donationAlertButton;
    public GameObject donationButtonPrefabInteraction; //button we press to accept or reject a donation (accept + money) or(reject  + no money)
    public List<GameObject> donationsButtons = new List<GameObject>();

    public Queue<Donation> incomingDonations = new Queue<Donation>(); //all unread donations 
    public List<Donation> activeDonations = new List<Donation>(); //all read donations
    private int unreadDonationsCount = 0; //number of unread donations

    //Spawn Properties
    private GameObject refugeeSpawnNotificationPanelGO;

    //Structure Pop Up UI
    private GameObject upgradePopUpDialogue;

    //Available Tents
    private TentStats[] tents;

    //Residents 
    GameObject[] residents;

    //Families Counter (ID)
    public int familyCounter = 0; //each family spawn gets this Value in their properties and then this value is incremented to provide ID for the next to come and so on.

    //Properties for Visualization in the action panel.
    public TextMeshProUGUI utilityCostText;
    public TextMeshProUGUI occupantsText;

    public List<string> blackListDonators = new List<string>();
    public List<string> rewardingDonators = new List<string>();

    void Awake(){
        //Game Manager Tasks
        //DontDestroyOnLoad(gameObject); //preserve Manager LifeTime
        SingletonCheck(); //Singleton

        //Check Framerate before running the application and setting up the system properly.
        Resolution[] res = Screen.resolutions;
        foreach(Resolution resolution in res){
            if (resolution.ToString().Contains(Screen.currentResolution.ToString())){
                //Debug.Log("Resolution :" + resolution.ToString());
                Application.targetFrameRate = resolution.refreshRate;
            }
        }

        //Agreements and Rules Members
        agreementsRulesGO = GameObject.Find("AgreementBook");//GameObject.Find("Agreements_Rules_Holder");        
        
        //Interactions With Structures and Agents Members
        waitingAreaPopupAgentSelectionListGO = GameObject.Find("WaitingAreaFocusActions"); //Waiting Area Action Panel
        actionsListGO = GameObject.Find("ObjectFocusActions"); //Structures and Agents 
        agentActionsListGO = GameObject.Find("AgentFocusActions");


        //Interactions With Donation Elements Members
        incomingDonationAreaGO = GameObject.Find("DonationListEntries");
        donationListGO = GameObject.Find("DonationList");
        donationButtonGOText = GameObject.Find("DonationAlertPopUp")?.GetComponentInChildren<TextMeshProUGUI>();
        donationDialogueGO = GameObject.Find("DonationDialoguePanel");
        donationDialogueGOText = GameObject.Find("DialogueText")?.GetComponent<TextMeshProUGUI>();
        donationDialogueAgreementGOText = GameObject.Find("DialogueTextAgreement")?.GetComponent<TextMeshProUGUI>();
        donationAcceptButton = GameObject.Find("AcceptDonation")?.GetComponent<Button>();
        donationRejectButton = GameObject.Find("RejectDonation")?.GetComponent<Button>();

        //Interactions With Resources Members
        populationResourceValueText = GameObject.Find("PopulationCaptionValueText")?.GetComponent<TextMeshProUGUI>();
        moneyResourceValueText = GameObject.Find("MoneyCaptionValueText")?.GetComponent<TextMeshProUGUI>();
        medicineResourceValueText = GameObject.Find("MedicalResourcesValueText")?.GetComponent<TextMeshProUGUI>();
        foodResourcesValueText = GameObject.Find("FoodResourcesValueText")?.GetComponent<TextMeshProUGUI>();

        //Refugees Spawning Members
        refugeeSpawnNotificationPanelGO = GameObject.Find("RefugeesEntryCheckDialogue");

        //Structure Pop Up UI
        upgradePopUpDialogue = GameObject.Find("UpgradePopUpDialogue");

        //Structure Action Button List text Elements 
        utilityCostText = GameObject.Find("MaintainanceCosts").GetComponent<TextMeshProUGUI>();
        occupantsText = GameObject.Find("Occupants").GetComponent<TextMeshProUGUI>();
}

    void Start(){
        //Clear On Load
        InitCampManager();

        //Scene Member Tasks
        DisableGOs(); //Disable Common use purpose GOs for init purposes.
        ToggleIncomingDonationsList(); //init state and update if needed

        startingYear = UIClockController.GetYear(); //get starting year;
        timestamp = (int)Mathf.Floor(Time.time); //init with the first tick for the first run
    }

    void Update(){
        GameOver(); // If money < 2k or 1 year has passed game will end.

        ToggleIncomingDonationsList(); //update Switch for Donation Pop up

        //Periodically Create Donations
        if (((int)Mathf.Floor(Time.time) * tSpeedVar) >= timestamp){
            //Random Pick if Donation has Caveat (20% caveat/ 80% no caveat)
            float caveAtProb = Random.Range(0.0f, 1.0f);

            if(caveAtProb > 0.8f){
                //Random Pick if Donation has Restriction or Reward Caveat (50% Restriction / 50% Reward) - Donation with Caveats will be significantly higher, but failure will reduce the base range a lot next time according to liability
                float resOrReward = Random.Range(0.0f, 1.0f);
                if(resOrReward >= 0.5f){
                    //Reward
                    List<Reward> availableRewards = new List<Reward>();
                    foreach(Reward r in AgreementsManager.Instance.rewards){
                        if (!AgreementsManager.Instance.activeRewards.Contains(r) && !AgreementsManager.Instance.awaitingAllocationRewards.Contains(r) && !AgreementsManager.Instance.completedRewards.Contains(r)) availableRewards.Add(r);
                    }

                    if (availableRewards.Count > 0){
                        Reward rew = availableRewards[Random.Range(0, availableRewards.Count)];
                        rew.AnalyzeMessage();
                        CreateDonation(Random.Range(7000, 10000), AgreementsManager.Instance.GetComponent<DonationEntities>().GetRandomDonator(), rew.GetMessage(), rew); //(300,700)
                    }
                }
                else{
                    //Restriction
                    List<Restriction> availableRestrictions = new List<Restriction>();
                    foreach (Restriction res in AgreementsManager.Instance.restrictions){
                        if (!AgreementsManager.Instance.activeRestrictions.Contains(res) && !AgreementsManager.Instance.awaitingAllocationRestrictions.Contains(res) && !AgreementsManager.Instance.completedRestrictions.Contains(res)) availableRestrictions.Add(res);
                    }

                    if (availableRestrictions.Count > 0)
                    {
                        Restriction restr = availableRestrictions[Random.Range(0, availableRestrictions.Count)];
                        restr.AnalyzeMessage();
                        CreateDonation(Random.Range(7000, 10000), AgreementsManager.Instance.GetComponent<DonationEntities>().GetRandomDonator(), restr.GetMessage(), restr); //(450,700)
                    }
                }
                return; 
            }

            //No Caveat
            CreateDonation(Random.Range(4000, 5000), AgreementsManager.Instance.GetComponent<DonationEntities>().GetRandomDonator(), null , null); //(100, 400)
        }

        //Update Donations
        UpdateIncomingDonations();
        
        //Update Time
        UpdateTime();

        //Update Population. (residents)
        if (((int)Mathf.Floor(Time.time) * tSpeedVar) >= populationCheckTimeStamp) UpdateCommunity();

        CleanupBlacklistedExistingDonations(); //clean up any existing donations that are belong to blacklisted donation entities (even previous cases)

        //Update Overall Happiness
        if (((int)Mathf.Floor(Time.time) * tSpeedVar) >= updateOverallHappinessTimestamp) UpdateOverallHappiness();
        if(overallHappinessSlider.fillAmount != overallHappiness) overallHappinessSlider.fillAmount = Mathf.Lerp(overallHappinessSlider.fillAmount, overallHappiness, 1.0f * Time.deltaTime);
    }

    //Game Manager Internal Tasks //////////////////////////////////////

    //Singleton
    private void SingletonCheck(){
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    //Reset Stats for global properties.
    private void InitCampManager(){

        //Game  Time
        gameTime = new GameTime(0); 

        //Reset Values to Original State (It's called after each Scene Load to establish values)
        activeDonations.Clear();
        incomingDonations.Clear();
        money = 80000;
        moneyResourceValueText.text = money.ToString();
        medicineResourceValueText.text = medicineResourceNum.ToString();
        foodResourcesValueText.text = foodResourceNum.ToString();

        //Get Current Max amount of slots
        tents = FindObjectsOfType<TentStats>();
        for (int i=0; i< tents.Length; i++) communityMaxSize += tents[i].maxCapacity;

        //Get Current number of Residents
        residents = GameObject.FindGameObjectsWithTag("Refugee");

        populationResourceValueText.text = residents.Length.ToString() + " | " + communityMaxSize.ToString();

    }


    //Common GOs Manager Functions //////////////////////////////////////

    //Disable Common GOs InitUse
    void DisableGOs(){
        waitingAreaPopupAgentSelectionListGO?.SetActive(false);
        actionsListGO?.SetActive(false);
        agentActionsListGO?.SetActive(false);

        incomingDonationAreaGO?.SetActive(false);
        donationDialogueGO?.SetActive(false);

        refugeeSpawnNotificationPanelGO?.SetActive(false);
        agreementsRulesGO?.SetActive(false);

        upgradePopUpDialogue?.SetActive(false);

        returnToMainMenuButton?.onClick.AddListener(() => { SceneManager.LoadScene("MainMenu"); });
        quitGame?.onClick.AddListener(() => { Application.Quit(); });
        gameoverGO?.SetActive(false);
    }

    //Return Upgrade Dialogue
    public GameObject GetUpgradeDialogueGO(){
        return upgradePopUpDialogue;
    }

    public TextMeshProUGUI GetOccupantsText(){
        return occupantsText;
    }

    public TextMeshProUGUI GetUtilitiesCostText(){
        return utilityCostText;
    }

    //Return the Actions Buttons GameObject reference.
    public GameObject GetStructureButtonActionsGO(){
        return actionsListGO;
    }

    public GameObject GetAgentButtonActionsGO(){
        return agentActionsListGO;
    }

    public GameObject GetAgentHealthSliderGO(){
        return agentHealthSliderGO;
    }

    public GameObject GetAgentHungerSliderGO()
    {
        return agentHungerSliderGO;
    }

    public GameObject GetAgentHappinessSliderGO()
    {
        return agentHappinessSliderGO;
    }

    public GameObject GetAgreementsAndRulesGO(){
        return agreementsRulesGO;
    }

    public GameObject GetSpawnRefugeesNotificationPopUp(){
        return refugeeSpawnNotificationPanelGO;
    }

    //Return the Waiting Area Pop up GameObject reference.
    public GameObject GetWaitingAreaInteractionsGO(){
        return waitingAreaPopupAgentSelectionListGO;
    }

    public int GetCurrentPopulation(){
        return residents.Length;
    }

    public int GetCurrentHousingFacilities(){
        return tents.Length;
    }

    public float GetOverallHappiness()
    {
        return overallHappiness;
    }

    //Game Manager Operations (Routines)

    //Update Camp Happiness
    private void UpdateOverallHappiness(){
        int count = residents.Length;
        float tempScore = 0.0f;

        //Case No agents arrived/exist
        if (count == 0){
            count = 1;
            tempScore = 1;
        }

        //Calculate all agents average and then divide by their count.
        foreach(GameObject r in residents){
            tempScore += r.GetComponent<RefugeeBehavior>().GetAgentHappiness();
        }

        tempScore /= count; //get overall average happiness
        overallHappiness = tempScore; //assign value
        updateOverallHappinessTimestamp += (int)Mathf.Floor(Time.time) + 4;

        //Update UI
        if (overallHappiness > 1) overallHappiness = 1.0f;
        if (overallHappiness < 0) overallHappiness = 0.0f;
    }

    //Update Time
    private void UpdateTime() {
        gameTime += Time.deltaTime * (targetSpeedModifier * tSpeedVar);
        UIClockController.UpdateGameTime(gameTime);
    }

    //Update Income/Expenses in Money
    public void UpdateMoney(string action = "", float amount = 0){
        string calculation = action;

        switch (calculation){
            case "+": {
                    money += amount;
                    moneyResourceValueText.text = money.ToString();
                    totalDonationMoneyAcquired += (int)amount; //Tracking Money from Donations
                    break;
                }
            case "-":{
                    money -= amount;
                    moneyResourceValueText.text = money.ToString();
                    totalExpenses += (int)amount; //Tracking Expenses
                    break;
                }
            default: break;
        }
    }

    //Game Over Function (Win/Lose)
    private void GameOver(){
        //Bankrupt - Lost game
        if (money <= gameOverMoneyCriteria){
            gameoverGO?.SetActive(true); //enable game over menu
            Time.timeScale = 0; //stop system time.
        }

        //1 year has passed - Win - Statistics
        if (UIClockController.GetYear() - startingYear == 1){
            Analytics.Instance.GenerateReport();
            //TODO ADD Report Card with buttons (Return to Main Menu | Quit) to Load Main Menu or Quit the game.
            Time.timeScale = 0; //stop system time.
        }
    }

    //Update Resident Count Coming/Leaving/Staying
    public void UpdateCommunity(){
        //Get Current number of Residents
        if (GameObject.FindGameObjectsWithTag("Refugee").Length != residents.Length){
            GameObject[] agents = GameObject.FindGameObjectsWithTag("Refugee"); //get all agents
            List<GameObject> validResidents = new List<GameObject>(); //empty gameobject list

            //Verify difference in Agents (only residents)
            foreach(GameObject agent in agents){
                if (agent.GetComponent<RefugeeStats>().GetHome() != null) validResidents.Add(agent);
            }

            //Update residents with the newest addition
            residents = validResidents.ToArray();
            populationResourceValueText.text = residents.Length.ToString() + " | " + communityMaxSize.ToString();
        }

        //Get Current Max amount of slots
        tents = FindObjectsOfType<TentStats>();
        communityMaxSize = 0;
        for (int i = 0; i < tents.Length; i++) communityMaxSize += tents[i].maxCapacity;
        populationResourceValueText.text = residents.Length.ToString() + " | " + communityMaxSize.ToString();

        populationCheckTimeStamp = (int)Mathf.Floor(Time.time) + 2;//update TimeStamp
    }

    public void UpdateFoodResource(string action = "", int amount = 0)
    {
        string calculation = action;

        switch (calculation)
        {
            case "+":
                {
                    foodResourceNum += amount;
                    foodResourcesValueText.text = foodResourceNum.ToString();
                    break;
                }
            case "-":
                {
                    if (foodResourceNum > 0) foodResourceNum -= amount;
                    foodResourcesValueText.text = foodResourceNum.ToString();
                    break;
                }
            default: break;
        }
    }

    public void UpdateMedicineResource(string action = "", int amount = 0)
    {
        string calculation = action;

        switch (calculation)
        {
            case "+":
                {
                    medicineResourceNum += amount;
                    medicineResourceValueText.text = medicineResourceNum.ToString();
                    break;
                }
            case "-":
                {
                    if (medicineResourceNum > 0) medicineResourceNum -= amount;
                    medicineResourceValueText.text = medicineResourceNum.ToString();
                    break;
                }
            default: break;
        }
    }

    //Handler to Remove or Add new Donations
    private void UpdateIncomingDonations(){
        if (donationListGO.activeInHierarchy && incomingDonations.Count > 0){ //use .activeSelf if we wish to add them before enabling the list. keep .activeInHierarchy if we want to be added when we enable UI
            GameObject x = Instantiate(donationButtonPrefabInteraction, donationListGO.transform, false); //Temp Variable Holding all info for Button
            Donation d = incomingDonations.Dequeue();

            activeDonations.Add(d); //Add incoming Donations to List When Active (from queue).
            donationsButtons.Add(x); //Create Instance of a Button for the List

            x.GetComponentInChildren<TextMeshProUGUI>().text = "Donator : " + activeDonations[activeDonations.Count - 1].GetDonatorName().Trim() + " with amount: " + activeDonations[activeDonations.Count - 1].GetDonationAmount(); //assign button with DonatorID
            x.gameObject.name = activeDonations[activeDonations.Count - 1].GetDonatorName(); //gameobject name
            
            donationButtonGOText.text = "Donations"; // reset to default once unread notifications have been read.

            //Add Event Listener to Object 
            x?.GetComponent<Button>().onClick.AddListener(() =>{
                donationDialogueGO.SetActive(true);
                donationDialogueGOText.text = x.GetComponentInChildren<TextMeshProUGUI>().text;

                if(d.GetAgreementType() == typeof(Restriction)){
                    Restriction tempRes = d.GetAgreement() as Restriction;
                    if(!AgreementsManager.Instance.activeRestrictions.Contains(tempRes) && !AgreementsManager.Instance.awaitingAllocationRestrictions.Contains(tempRes) && !AgreementsManager.Instance.completedRestrictions.Contains(tempRes))
                        tempRes.AnalyzeMessage();
                    donationDialogueAgreementGOText.text = tempRes.GetMessage();
                }else if (d.GetAgreementType() == typeof(Reward))
                {
                    Reward tempRew = d.GetAgreement() as Reward;
                    if (!AgreementsManager.Instance.activeRewards.Contains(tempRew) && !AgreementsManager.Instance.awaitingAllocationRewards.Contains(tempRew) && !AgreementsManager.Instance.completedRewards.Contains(tempRew))
                        tempRew.AnalyzeMessage();
                    donationDialogueAgreementGOText.text = tempRew.GetMessage();
                }
                else donationDialogueAgreementGOText.text = d.GetDonationDescription();


                //Remove Any Listeners
                donationAcceptButton.GetComponent<Button>().onClick.RemoveAllListeners();
                donationRejectButton.GetComponent<Button>().onClick.RemoveAllListeners();
                donationAcceptButton.GetComponent<Button>().onClick.AddListener(() => {  DestroyDonation(d, x, d.GetDonationAmount()); donationDialogueGO.SetActive(false); });
                donationRejectButton.GetComponent<Button>().onClick.AddListener(() => {  DestroyDonation(d, x, 0.0f); donationDialogueGO.SetActive(false); });
             });
    
            unreadDonationsCount = 0; //No Unread donations
        }
        else{
            //Notify the number of unread donations
            if (unreadDonationsCount < incomingDonations.Count){
                unreadDonationsCount += 1; //increase count
                donationButtonGOText.text = "Donations (" + unreadDonationsCount.ToString() + ")"; //alert the number of unread donations.
            }
        }

        /*
        //If Donation Dialogue is Active 
       // if (donationDialogueGO.activeInHierarchy && donationListGO.activeInHierarchy){

            //Loop Through Donations to update their timer when Active 
            //foreach (Donation donation in activeDonations){
              //  if (donationDialogueGOText.text.Contains(donation.GetDonatorName())){
                //    donationDialogueGO.GetComponentInChildren<Slider>().value = donation.CountDownDonation();

                    //If Slider of Active Donation reaches 0 -> Destroy
                  //  if (donationDialogueGO.GetComponentInChildren<Slider>().value <= 0.0f){
                    //    donationDialogueGO.GetComponentInChildren<Slider>().value = 1.0f;
                    //    donationAcceptButton.GetComponent<Button>().onClick.RemoveAllListeners();
                    //    donationRejectButton.GetComponent<Button>().onClick.RemoveAllListeners();

                        //Destroy When Time Runs Out
                    //    Destroy(GameObject.Find(donation.GetDonatorName()));
                    //    activeDonations.Remove(donation);
                    //    donationsButtons.Remove(GameObject.Find(donation.GetDonatorName()));

                        //Disable Pop up Dialog
                    //    donationDialogueGO.SetActive(false);
                    //    break;
                //    }
              //  }
           // }
        //}
        //else{
        //    donationDialogueGO.SetActive(false);
       // }
       */
    }

    //Destroy Donation
    private void DestroyDonation(Donation d, GameObject donationButton, float donationAmount){
        if (activeDonations.Contains(d)){
            activeDonations.Remove(d);
            UpdateMoney("+", donationAmount);

            //Analytics Tracking Donations
            if (donationAmount != 0) acceptedDonations += 1;
            if (donationAmount == 0) rejectedDonations += 1;
        }

        if (donationsButtons.Contains(donationButton)) donationsButtons.Remove(donationButton);
        if (GameObject.Find(d.GetDonatorName()) != null) Destroy(GameObject.Find(d.GetDonatorName()));
        if (donationDialogueGO.activeInHierarchy && donationListGO.activeInHierarchy) donationDialogueGO.SetActive(false);


        //Agreements Section
        if(d.GetDonationDescription() != default && donationAmount > 0){
            AgreementsManager.Instance.AddDonator(d.GetDonatorName());
            int caveAtThesholdLimiterCount = AgreementsManager.Instance.activeRewards.Count 
                                            + AgreementsManager.Instance.activeRestrictions.Count 
                                            + AgreementsManager.Instance.completedRestrictions.Count
                                            + AgreementsManager.Instance.completedRewards.Count;

            //Restriction Type
            if (d.GetAgreementType().ToString().Equals("Restriction") && caveAtThesholdLimiterCount <= 6) {
                Restriction tempRestriction = d.GetAgreement() as Restriction;
                tempRestriction.donationEntity = d.GetDonatorName();
                if (!AgreementsManager.Instance.activeRestrictions.Contains(tempRestriction) && !AgreementsManager.Instance.awaitingAllocationRestrictions.Contains(tempRestriction))
                {
                    tempRestriction.score = 0; //reset score to 0 upon addition
                    if (AgreementsManager.Instance.agreementsTab.activeInHierarchy){    
                        AgreementsManager.Instance.activeRestrictions.Add(tempRestriction);
                        AgreementsManager.Instance.AddAgreement(tempRestriction);
                    }
                    else AgreementsManager.Instance.awaitingAllocationRestrictions.Add(tempRestriction);
                }
            }

            //Reward Type
            if (d.GetAgreementType().ToString().Equals("Reward") && caveAtThesholdLimiterCount <= 6){
                Reward tempReward = d.GetAgreement() as Reward;
                tempReward.donationEntity = d.GetDonatorName();
                if (!AgreementsManager.Instance.activeRewards.Contains(tempReward) && !AgreementsManager.Instance.awaitingAllocationRewards.Contains(tempReward)) {
                    if (AgreementsManager.Instance.agreementsTab.activeInHierarchy){
                        tempReward.score = 0; //reset score to 0 upon addition
                        AgreementsManager.Instance.activeRewards.Add(tempReward);
                        AgreementsManager.Instance.AddAgreement(tempReward);
                    }
                    else AgreementsManager.Instance.awaitingAllocationRewards.Add(tempReward);
                }
            }
        }
    }

    //Donation Create
    private void CreateDonation(float sum = default, string donator=default, string description=default, Object agreement = default){
        //Round donation values to nearest hundred
        sum = Mathf.Round(sum/100) * 100;

        //Apply CaveAts Penalties or Rewards

        foreach (string s in blackListDonators)
        {
            if (donator.Contains(s)){
                //sum /= 2;
                return; //do not create the donation in case the donator is blacklisted
            }
        }

        foreach (string s in rewardingDonators)
        {
            if (donator.Contains(s))
            {
                sum *= 2;
            }
        }

        //Create and Assign new Donation to List
        Donation donation = new Donation(sum,donator,description,agreement);
        incomingDonations.Enqueue(donation); 

        //Timestamp Manipulation for Receiving next Donation
        timestamp = (int)Mathf.Floor(Time.time) + (spawnTimeOffset/targetSpeedModifier); //update TimeStamp (taking into account the speed modifier as well)
    }

    //Removes any donation that is created that contains a blacklisted donator as entity (case where a donation is created before getting blacklisted by an agreement) 
    public void CleanupBlacklistedExistingDonations(){
        for(int i = 0; i < activeDonations.Count; i++){
           foreach(string s in blackListDonators){
                if (activeDonations[i].GetDonatorName().Trim() == s){
                    DestroyDonation(activeDonations[i], donationsButtons[i], 0.0f); // Destroy existing blacklisted Donation
                }
            }
        }
    }

    //Toggle Listener State
    public void ToggleIncomingDonationsList(){
        donationAlertButton.onClick.RemoveAllListeners(); //Clear Listeners if any
        if (incomingDonationAreaGO.activeSelf)
        {
            donationAlertButton.onClick.AddListener(() => { incomingDonationAreaGO.SetActive(false); });
            if (donationDialogueGO.activeSelf) donationAlertButton.onClick.AddListener(() => { donationDialogueGO.SetActive(false); });
        }
        else donationAlertButton.onClick.AddListener(() => { incomingDonationAreaGO.SetActive(true); });

    } 
}
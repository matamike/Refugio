using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class RefugeeBehavior : MonoBehaviour {
    private NavMeshAgent refugeeNav;
    public float hospitalTime = 10.0f;
    private float foodCourtTime = 10.0f;
    private float socializingTime = 10.0f;
    private RefugeeNavigation refugeeNavigationScriptRef;
    private RefugeeStats refStats;
    public GameObject agentActionsPanel;
    [SerializeField] private float agentHappiness = default;

    public bool init = false;

    //Visualization Properties for UI
    public float hungerLevel = 0;
    public float hungerLevelDropRate;
    public Slider hungerSlider;

    public float healthLevel = 0;
    public float healthLevelDropRate;
    public Slider healthSlider;

    public float emotionLevel = 0;
    public float emotionLevelDropRate;
    public Slider emotionSlider;

    public int decayProbabilityModifier;
    // /////////////////////////////////////////

    // Agent Happiness variables//
    private int updateHappinessTimestamp = 0;
    public int updateHappinessFrequency = 15;
    // //////////////////////////


    //Visualizations Properties for Agent Behavior
    public GameObject objectIndicator;
    public Material objectIndicatorMat;
    public Color objectIndicatorColor;

    [Range(0.01f, 10.0f)]
    public float objectIndicatorColorFlashSpeed = 2;
    // //////////////////////////////////////////

    //Properties for Agent Socializing Behavior
    public float socializingCD = 35.0f;
    public bool isInCooldown = false;
    public int socializingStatus = -1;
    private float timestamp = 0.0f;
    private float socializingBias;
    // //////////////////////////////////////////

    //Properties for Agent Going Back Home
    public float stayingTime = 30.0f; //30 seconds staying time. (in home)
    public float excessStayingHomeBias; //applying extra/less staying home time
    public float stayingHomeTimestamp = 0.0f; //Timestamp for registering agent in Home.

    public float stayingHomeCooldownTimestamp = 0.0f; //Cooldown period timestamp before checking again (update only after reset)
    public int stayingHomeFrequency; //frequency how often to check if the agent could go home.
    public float stayingHomeCooldown; // cooldown time after leaving home

    void Start() {
        socializingBias = Random.Range(1, 100); // (bias between 1% - 100%) 
        stayingHomeFrequency = (int)Random.Range(10.0f, 30.0f); //(frequency between 30-60 %)
        excessStayingHomeBias = Random.Range(-10.0f, 30.0f); //(bias between -10 - 30 seconds)
        stayingTime += (int)excessStayingHomeBias; //apply bias per agent to have a randomness in visiting
        stayingHomeCooldown = (int)Random.Range(15.0f, 45.0f); //Cooldown for initiating the going back home process.

        //Setup Color Indicator Properties
        if (objectIndicator != null) {
            objectIndicatorMat = objectIndicator.GetComponent<SpriteRenderer>().material; //acquire indicator GO material
            objectIndicatorMat.SetColor("_Color", Color.black);
            objectIndicatorColor = objectIndicatorMat.color;
        }

        decayProbabilityModifier = Random.Range(0, 100); //upon creation randomize a percentage threshold.

        refStats = GetComponent<RefugeeStats>();
        refugeeNavigationScriptRef = GetComponent<RefugeeNavigation>();
        refugeeNav = this?.GetComponent<NavMeshAgent>();
        agentActionsPanel = RefugeeCampGameManager.Instance.GetAgentButtonActionsGO(); //get ref from Manager about AgentActionsPanel.

        //NEEDS SLIDERS
        healthSlider = RefugeeCampGameManager.Instance.GetAgentHealthSliderGO().GetComponentInChildren<Slider>();
        hungerSlider = RefugeeCampGameManager.Instance.GetAgentHungerSliderGO().GetComponentInChildren<Slider>();
        emotionSlider = RefugeeCampGameManager.Instance.GetAgentHappinessSliderGO().GetComponentInChildren<Slider>();

        //NEEDS ATTRIBUTES DROP RATE VALUES
        hungerLevelDropRate = Random.Range(0.001f, 0.005f); //randomize dropRate for property Hunger
        healthLevelDropRate = Random.Range(0.001f, 0.005f); //randomize dropRate for property Health
        emotionLevelDropRate = Random.Range(0.001f, 0.005f); //randomize dropRate for property Emotion

        //INITIALIZE ATTRIBUTES VALUES
        InitStatsValues(); //Set values for the slider and progression over time.
    }

    void Update() {
        //Update Levels of Stats
        UpdateRefugeeStatuses();

        //Update Values of Refugee Stats and Color Indicators
        if (RefugeeCampGameManager.Instance.actionListActiveGO == this.gameObject && agentActionsPanel.activeInHierarchy) 
        {
            UpdateNeedsUI(); //Update Slider Values in UI accordingly
            objectIndicator.GetComponent<SpriteRenderer>().enabled = true;
            objectIndicatorMat.color = ColorChange(objectIndicatorMat.color, objectIndicatorColor); //SELECTED STATE (STATIC COLOR)
        }
        else{
            if (refStats.GetHealthStatus() != RefugeeStats.Health.Healthy || refStats.GetHungerStatus() != RefugeeStats.Hunger.Full) // HUNGRY OR SICK STATE (FLASH RED COLOR)
            {
                objectIndicator.GetComponent<SpriteRenderer>().enabled = true;
                objectIndicatorMat.color = ColorFlash(Color.red); //Flash Color
            }
            else if (refugeeNavigationScriptRef.canWander) //WANDERING STATE (FLASH GREEN COLOR)
            {
                objectIndicator.GetComponent<SpriteRenderer>().enabled = true;
                objectIndicatorMat.color = ColorFlash(Color.green); //Flash Color
            }
            else if (refugeeNavigationScriptRef.isSocializing){ //SOCIALIZING STATE (FLASH BLUE COLOR)
                objectIndicator.GetComponent<SpriteRenderer>().enabled = true;
                objectIndicatorMat.color = ColorFlash(Color.blue); //Flash Color
            }
            else //DEFAULT NORMAL STATE (DISABLE INDICATOR)
            {
                ColorReset(); //Reset
                objectIndicator.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
        // /////////////////////////////////

        //Case Scenario - Agent attempts to Socialize
        if (refStats.GetHealthStatus() != RefugeeStats.Health.Dead && refugeeNavigationScriptRef.destinations.Count == 0 && refugeeNavigationScriptRef.isAvailable && refStats.GetHome() != null){
            
            //Requesting Socializing
            if(socializingStatus == -1) socializingStatus = AttemptSocializing(!isInCooldown); //-1 means (not socializing) , 1 means (socializing)

            //Moving to Agent
            if(socializingStatus == 1 && refugeeNavigationScriptRef.socializingDestination != null) refugeeNav.SetDestination(refugeeNavigationScriptRef.socializingDestination.transform.position);
        }

        //Cooldown Process when Socializing is over.
        if (isInCooldown) CoolDown();

        //Calculate Happiness
        if((int)Time.time > updateHappinessTimestamp) CalculateHappiness();
    }


    //Agent Happiness//
    private void CalculateHappiness(){
        agentHappiness = (healthLevel + hungerLevel + emotionLevel) / 3;
        updateHappinessTimestamp += updateHappinessFrequency; //update next timestamp
    }

    public float GetAgentHappiness()
    {
        return agentHappiness;
    }
    // ///////////////

    //ACTIONS
    // //////////////////////////////
    public void Heal(GameObject hospital) {
        if (GetComponent<RefugeeStats>().GetHealthStatus() != RefugeeStats.Health.Healthy) {
            if (!init) {
                init = true;
                refugeeNavigationScriptRef.ContinueNavigation = false;
                hospitalTime = (int)Mathf.Floor(Time.time) + hospitalTime;
                hospital.GetComponent<HospitalStats>().AddPatient(gameObject);
            }

            if ((int)Mathf.Floor(Time.time) >= (hospitalTime / (0.01f + RefugeeCampGameManager.Instance.tSpeedVar)))
            {
                init = false;
                GetComponent<RefugeeStats>().SetHealthStatus(RefugeeStats.Health.Healthy);
                healthLevel = 1.0f;
                refugeeNavigationScriptRef.RemoveDestination(hospital);
                refugeeNavigationScriptRef.ContinueNavigation = true;
                hospitalTime = 10.0f;
                hospital.GetComponent<HospitalStats>().RemovePatient(gameObject);
            }
        }
        else
        {
            init = false;
            refugeeNavigationScriptRef.RemoveDestination(hospital);
            refugeeNavigationScriptRef.ContinueNavigation = true;
            hospitalTime = 10.0f;
            hospital.GetComponent<HospitalStats>().RemovePatient(gameObject);
        }
    }

    public void Eat(GameObject foodCourt) {
        if (GetComponent<RefugeeStats>().GetHungerStatus() != RefugeeStats.Hunger.Full) {
            if (!init) {
                init = true;
                refugeeNavigationScriptRef.ContinueNavigation = false;
                foodCourtTime = (int)Mathf.Floor(Time.time) + foodCourtTime;
                foodCourt.GetComponent<FoodCourtStats>().AddResident(gameObject);
            }

            if ((int)Mathf.Floor(Time.time) >= (foodCourtTime / (0.01f + RefugeeCampGameManager.Instance.tSpeedVar))) {
                init = false;
                GetComponent<RefugeeStats>().SetHungerStatus(RefugeeStats.Hunger.Full);
                hungerLevel = 1.0f;
                refugeeNavigationScriptRef.RemoveDestination(foodCourt);
                refugeeNavigationScriptRef.ContinueNavigation = true;
                foodCourtTime = 10.0f;
                foodCourt.GetComponent<FoodCourtStats>().RemoveResident(gameObject);
            }
        }
        else {
            init = false;
            refugeeNavigationScriptRef.RemoveDestination(foodCourt);
            refugeeNavigationScriptRef.ContinueNavigation = true;
            foodCourtTime = 10.0f;
            foodCourt.GetComponent<FoodCourtStats>().RemoveResident(gameObject);
        }
    }

    public int Socialize(GameObject target) {
        //Initialize time for socializing
        if (!init) {
            init = true;
            refugeeNavigationScriptRef.ContinueNavigation = false;
            socializingTime = (int)Mathf.Floor(Time.time) + socializingTime;
            refugeeNavigationScriptRef.isSocializing = true;
        }

        //Socializing Interrupted
        if (healthLevel < 0.5f || hungerLevel < 0.5f) {
            return ResetSocializing();
        }
        else {
            //Debug.Log("Remaining Time : " + (socializingTime / (0.01f + RefugeeCampGameManager.Instance.tSpeedVar) - (int)Mathf.Floor(Time.time)));
            //Any Action wishing to happen when agents remain in place could be done triggering or doing anything in this part. (E.G: Emoji Bubble or text Dialogues etc.) 
            // Motion Driven actions should happen only in child nodes and reset

            //Socializing Finished
            if (((socializingTime / (0.01f + RefugeeCampGameManager.Instance.tSpeedVar)) - (int)Mathf.Floor(Time.time)) < 0.0f || target == null){
                emotionLevel = 1.0f;
                return ResetSocializing();
            }
        }
        return 0;
    }
    // //////////////////////////////

    //Properties Updates/Init
    // ///////////////////////
    void InitStatsValues() {
        //Health Meters
        if (refStats.GetHealthStatus() == RefugeeStats.Health.Dead) healthLevel = 0;
        if (refStats.GetHealthStatus() == RefugeeStats.Health.Healthy) healthLevel = 1.0f;
        if (refStats.GetHealthStatus() != RefugeeStats.Health.Dead && refStats.GetHealthStatus() != RefugeeStats.Health.Healthy) healthLevel = 0.5f;

        //Hunger Meters
        if (refStats.GetHungerStatus() == RefugeeStats.Hunger.Starving) hungerLevel = 0;
        if (refStats.GetHungerStatus() == RefugeeStats.Hunger.Full) hungerLevel = 1.0f;
        if (refStats.GetHungerStatus() != RefugeeStats.Hunger.Full && refStats.GetHungerStatus() != RefugeeStats.Hunger.Starving) hungerLevel = 0.5f;

        //Emotion Meters
        if (refStats.GetEmotionStatus() == RefugeeStats.EmotionStatus.Depressed) emotionLevel = 0;
        if (refStats.GetEmotionStatus() == RefugeeStats.EmotionStatus.Happy) emotionLevel = 1.0f;
        if (refStats.GetEmotionStatus() != RefugeeStats.EmotionStatus.Happy && refStats.GetEmotionStatus() != RefugeeStats.EmotionStatus.Depressed) emotionLevel = 0.5f;
    }

    void UpdateRefugeeStatuses() {

        //Calculate Stats Over Time
        if (Random.Range(0, 100) > decayProbabilityModifier) healthLevel -= (1.0f * healthLevelDropRate) * Time.deltaTime * RefugeeCampGameManager.Instance.tSpeedVar;
        if (Random.Range(0, 100) > decayProbabilityModifier) hungerLevel -= (1.0f * hungerLevelDropRate) * Time.deltaTime * RefugeeCampGameManager.Instance.tSpeedVar;
        if (Random.Range(0, 100) > decayProbabilityModifier) emotionLevel -= (1.0f * emotionLevelDropRate) * Time.deltaTime * RefugeeCampGameManager.Instance.tSpeedVar;

        //Limit Min values to 0 (ui limit)
        if (healthLevel < 0) healthLevel = 0;
        if (hungerLevel < 0) hungerLevel = 0;
        if (emotionLevel < 0) emotionLevel = 0;


        //Change Health Status Accordingly
        if (healthLevel == 0) {
            refStats.SetHealthStatus(RefugeeStats.Health.Dead);
            refugeeNavigationScriptRef.CheckHealth(refStats.GetHealthStatus().GetHashCode());
        }
        if (healthLevel > 0 && healthLevel <= 0.5f) {
            //If it's changed only. Then randomize and get a new trait.
            if (refStats.GetHealthStatus().GetHashCode() >= 5 || refStats.GetHealthStatus().GetHashCode() < 1)
            {
                int rndHealthStatus = Random.Range(1, 5);
                RefugeeStats.Health healthStatus = (RefugeeStats.Health)rndHealthStatus;
                refStats.SetHealthStatus(healthStatus);
            }
            //Apply Check on existing state
            if (RefugeeCampGameManager.Instance.medicineResourceNum > 0) refugeeNavigationScriptRef.CheckHealth(refStats.GetHealthStatus().GetHashCode());
        }
        if (healthLevel > 0.5f) {
            refStats.SetHealthStatus(RefugeeStats.Health.Healthy);
            refugeeNavigationScriptRef.CheckHealth(refStats.GetHealthStatus().GetHashCode());
        }
        // ///////////////////////////////// HEALTH CONDITIONS /////////////////////////////

        //Change Hunger Status Accordingly
        if (hungerLevel == 0) {
            refStats.SetHungerStatus(RefugeeStats.Hunger.Starving);
            if (RefugeeCampGameManager.Instance.foodResourceNum > 0) refugeeNavigationScriptRef.CheckHunger(refStats.GetHungerStatus().GetHashCode());
        }
        if (hungerLevel > 0 && hungerLevel <= 0.5f) {
            //If it's changed only. Then randomize and get a new trait.
            if (refStats.GetHungerStatus().GetHashCode() >= 5 || refStats.GetHungerStatus().GetHashCode() < 1) {
                int rndHungerStatus = Random.Range(1, 5);
                RefugeeStats.Hunger nutritionStatus = (RefugeeStats.Hunger)rndHungerStatus;
                refStats.SetHungerStatus(nutritionStatus);
            }
            //Apply Check on existing state
            if (RefugeeCampGameManager.Instance.foodResourceNum > 0) refugeeNavigationScriptRef.CheckHunger(refStats.GetHungerStatus().GetHashCode());
        }
        if (hungerLevel > 0.5f) {
            refStats.SetHungerStatus(RefugeeStats.Hunger.Full);
            refugeeNavigationScriptRef.CheckHunger(refStats.GetHungerStatus().GetHashCode());
        }
        // ///////////////////////////////// HUNGER CONDITIONS /////////////////////////////

        //Change Emotion Status Accordingly
        if (emotionLevel == 0) {
            refStats.SetEmotionStatus(RefugeeStats.EmotionStatus.Depressed);
            //refugeeNavigationScriptRef.CheckEmotions(refStats.GetEmotionStatus().GetHashCode());
        }
        if (emotionLevel > 0 && emotionLevel <= 0.5f) {
            //If it's changed only. Then randomize and get a new trait.
            if (refStats.GetEmotionStatus().GetHashCode() >= 4 || refStats.GetEmotionStatus().GetHashCode() < 1) {
                int rndEmoStatus = Random.Range(1, 4);
                RefugeeStats.EmotionStatus emStatus = (RefugeeStats.EmotionStatus)rndEmoStatus;
                refStats.SetEmotionStatus(emStatus);
            }
            //Apply Check on existing state
            //refugeeNavigationScriptRef.CheckEmotions(refStats.GetEmotionStatus().GetHashCode());
        }
        if (emotionLevel > 0.5f) {
            refStats.SetEmotionStatus(RefugeeStats.EmotionStatus.Happy);
            //refugeeNavigationScriptRef.CheckEmotions(refStats.GetEmotionStatus().GetHashCode());
        }
        // ///////////////////////////////// EMOTION CONDITIONS /////////////////////////////


    }

    void UpdateNeedsUI() {
        healthSlider.value = healthLevel;
        hungerSlider.value = hungerLevel;
        emotionSlider.value = emotionLevel;
    }
    // ///////////////////////

    //Color Custom Functions
    // //////////////////////////

    Color ColorChange(Color sourceColor, Color targetColor) {
        return sourceColor = Color.Lerp(sourceColor, targetColor, 2.0f * Time.deltaTime);
    } 

    Color ColorFlash(Color currentColor){
        return new Color(currentColor.r, currentColor.g, currentColor.b, (Time.time % objectIndicatorColorFlashSpeed) + 0.1f); //flash per sec
    }

    void ColorReset(){
        objectIndicatorMat.color = objectIndicatorColor; //objectIndicatorColor is a constant
    }
    // /////////////////////////

    // Socializing Register/Check Functions

    //Check for Agents available for socializing
    int AttemptSocializing(bool canSocialize = false){
        int status = -1;

        //Socializing Routine Check
        if (canSocialize)
        {
            if (Physics.Linecast(transform.position, refugeeNav.destination, out RaycastHit hitInfo))
            {
                Debug.DrawLine(transform.position, refugeeNav.destination, Color.red, 0.1f); //DEBUG SHOW DESTINATION
                GameObject candidateAgent = hitInfo.collider.gameObject; //acquire GO to check if Agent is hitting other agent

                //Socializing Criteria
                if (candidateAgent.tag.Contains("Refugee"))
                {
                    //Acquire candidate agent script properties for further checking
                    RefugeeNavigation candidateRefugeeNav = candidateAgent.GetComponent<RefugeeNavigation>();
                    RefugeeBehavior candidateRefugeeBehavior = candidateAgent.GetComponent<RefugeeBehavior>();
                    RefugeeStats candidateRefugeeStats = candidateAgent.GetComponent<RefugeeStats>(); 

                    //Check if Agent Hit is a newcomer and not busy doing top priority stuff
                    if (candidateRefugeeStats.GetHome() != null && candidateRefugeeNav.destinations.Count == 0)
                    {
                        //Check if Agent Hit had not recently socialized and is currently wandering and being available(not home) 
                        if (!candidateRefugeeBehavior.isInCooldown && candidateRefugeeNav.canWander && candidateRefugeeNav.isAvailable)
                        {
                            //Candidate to Socialize
                            refugeeNavigationScriptRef.canWander = false; //Stop Wander Mode
                            refugeeNavigationScriptRef.socializingDestination = candidateAgent; //Target hit
                            status = 1; //return status flag.
                        }
                    }
                }
            }
        }
        return status;
    }

    //Receive Request to Socialize
    public bool ReceivePing(GameObject entity){
        if (entity.tag.Contains("Refugee") && (refugeeNavigationScriptRef.socializingDestination == null || refugeeNavigationScriptRef.socializingDestination == entity)){
            refugeeNavigationScriptRef.socializingDestination = entity;
            refugeeNavigationScriptRef.canWander = false;
            socializingStatus = 1;
            return true; //success response
        }
        return false; //failure response
    }

    public int ResetSocializing(){
        init = false;
        socializingTime = 10.0f;
        socializingStatus = -1;
        isInCooldown = true;
        refugeeNavigationScriptRef.ContinueNavigation = true;      
        refugeeNavigationScriptRef.isSocializing = false;
        refugeeNavigationScriptRef.socializingDestination = null;       
        refugeeNav.isStopped = false;
        return -1;
    }

    void CoolDown(){
        if (timestamp == 0.0f) timestamp = (int)(Time.time + socializingCD + (emotionLevel * 100 / socializingBias)); //assign a unique timestamp as reset.

        if((int)Time.time >= (int)timestamp){
            timestamp = 0.0f; //reset timestamp
            isInCooldown = false; //disable flag
        }
    }
    // ///////////////////////////////////////////////////

    // Going Home (Random Times) Routine (Check/Reset) Functions

    //Debug and if agent needs treament (food/hospital or allocation) / trigger from navigation script
    public bool ResetStayingHomeCooldown()
    {
        stayingHomeTimestamp = 0.0f;
        stayingHomeCooldownTimestamp = stayingHomeCooldownTimestamp = (int)Time.time + stayingHomeCooldown;
        refugeeNavigationScriptRef.isAvailable = true;
        refugeeNavigationScriptRef.canWander = true;
        return true;
    }

    //Use to check if it's time to allow the agent to go back wandering / trigger from navigation script.
    public bool CanLeavehome(){
        //Stay Home Finished
        if ((int)Time.time * RefugeeCampGameManager.Instance.tSpeedVar >= stayingHomeTimestamp){
            return ResetStayingHomeCooldown();
        }
        else{
            //Stay Home Interrupted
            if (healthLevel < 0.5f || hungerLevel < 0.5f){
                return ResetStayingHomeCooldown();
            }

            //Idling in Home
            refugeeNavigationScriptRef.isAvailable = false;
            refugeeNavigationScriptRef.canWander = false;
            return false;
        }
    }

    //Enable the timestamp and check within navigation script / trigger from navigation script
    public bool SetStayingHomeCooldown(float goHomeTimestamp){
        float tempRand = (Random.Range(0.0f, 1.0f) * 100);

        //Probabilistic entry check for agent when the CD per request to go home is done.
        if (tempRand < stayingHomeFrequency){
            stayingHomeTimestamp = goHomeTimestamp + stayingTime; //Timestamp + offset
            refugeeNav.SetDestination(refStats.GetHome().transform.position);
            refugeeNavigationScriptRef.canWander = false;
            return false;
        }
        else{
            refugeeNavigationScriptRef.canWander = true;
            return true;
        }
    }

    public float GetStayingHomeCooldown(){
        //Debug.Log("Time : " + (int)Time.time);
        if ((int)Time.time >= stayingHomeCooldownTimestamp) return stayingHomeCooldownTimestamp = (int)Time.time + stayingHomeCooldown;
        else return 0.0f;
    }
    // //////////////////////////////////////////////////
}

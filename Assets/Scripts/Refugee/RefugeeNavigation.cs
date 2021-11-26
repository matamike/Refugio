using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEngine.AI;


public class RefugeeNavigation : MonoBehaviour{
    private RefugeeStats refugeeStats = null; //Dependency (NPC Stats)
    private RefugeeBehavior refBehavior = null; //Dependency (Socializing Behavior Switch)
    private NavMeshAgent nav; //Agent for moving to destinations.
    public GameObject objFocusGO; //trackball target.
    private LineRenderer lineRend; // indicator for home allocation.
    private float agentSpeed; //variable speed with slow and fast interpolations depending on the game state.

    [SerializeField]
    public bool ContinueNavigation{ set; get; } //flag to allow agent to continue navigating to the rest of the destinations according to the path plan.

    public bool manualHomeAllocation = false; //flag to check if we need to allocate agents manually or automatically.

    private bool isManualTargeting = false; // flag when we allocate agents from waiting area to a tent
    public GameObject[] structureTargets; // all the potential static structures in the camp
    public List<GameObject> destinations; // static destinations in camp
    public GameObject socializingDestination; //agent to track down and socialize
    private bool isAgentAccepted = false; //enabled once an agent is admitted to the camp -> becomes false right after
    public bool canWander = false; //wandering mode flag
    public bool isSocializing = false; //flag for agent socializing
    public bool isAvailable = true; //flag for agent being home.

    //Family Size (not 0 when a family spawns)
    private int familyOffsetCount = 0;


    void Start(){
        ContinueNavigation = false; //able to navigate by default.
        lineRend = GetComponent<LineRenderer>();
        lineRend.enabled = false;
        objFocusGO = GameObject.Find("CameraFocus");
        nav = this.gameObject?.GetComponent<NavMeshAgent>();
        agentSpeed = nav.speed;
        structureTargets = GameObject.FindGameObjectsWithTag("Structure");
        destinations = new List<GameObject>();
        refugeeStats = this.gameObject?.GetComponent<RefugeeStats>();
        refBehavior = this.gameObject?.GetComponent<RefugeeBehavior>();
    }

    void Update(){
        //When agents first arrives and goes (ONLY ONCE)
        if (isAgentAccepted){
            CheckHealth(refugeeStats.GetHealthStatus().GetHashCode()); //Check the Health of the Refugee
            CheckHunger(refugeeStats.GetHungerStatus().GetHashCode()); //Check the Hunger level of the Refugee
            CheckHouseStatus(gameObject.GetComponent<RefugeeStats>().familyMemberID); //Check if is Individual or Fammily
            isAgentAccepted = false; //reset flag
            ContinueNavigation = true; //able to navigate by default.

            //Update variables for analytics.
            if (refugeeStats.RefugeeProfession.Contains("Unemployed")) RefugeeCampGameManager.Instance.childrenAccepted += 1;
            if (refugeeStats.RefugeeGender == 0 && !refugeeStats.RefugeeProfession.Contains("Unemployed")) RefugeeCampGameManager.Instance.menAccepted += 1;
            if (refugeeStats.RefugeeGender == 1 && !refugeeStats.RefugeeProfession.Contains("Unemployed")) RefugeeCampGameManager.Instance.womenAccepted += 1;
        }
        // ////////////////////////////////////////////

        Navigate();//Navigate to Target
        AddManualLocation(isManualTargeting); //Happens only when Agent is manually allocated from Waiting Area -> Tent
    }

    void CleanupRefugeeRegistry(){
        GameObject home = GetComponent<RefugeeStats>().GetHome();

        //Remove from Home and update dialogues
        if (home)
        {
            if (home.TryGetComponent(out TentStats tStats))
            {
                tStats.RemoveResident(gameObject);
                tStats.UpdateOccupants();
            }
            if (home.TryGetComponent(out WaitingAreaStats wStats))
            {
                wStats.RemoveResident(gameObject);
                wStats.UpdateOccupants();
            }
        }

        //Remove from FoodCourt or Hospital if the agent exists before destroying and update dialogues
        HospitalStats[] hospitals = FindObjectsOfType<HospitalStats>();
        FoodCourtStats[] foodCourts = FindObjectsOfType<FoodCourtStats>();
        foreach (HospitalStats hStat in hospitals)
        {
            hStat.RemovePatient(gameObject);
            hStat.UpdateOccupants();
        }
        foreach (FoodCourtStats fStat in foodCourts)
        {
            fStat.RemoveResident(gameObject);
            fStat.UpdateOccupants();
        }

        //Update Game Manager
        RefugeeCampGameManager.Instance.UpdateCommunity();
    }


    void OnTriggerEnter(Collider other){
        //Register Tenant(Waiting Area or Tent) - Feature adapting to agent changing housing while transitioning from one place to another now.
        if (other.gameObject.name.Contains("Waiting") || other.gameObject.name.Contains("Tent")){
            if (destinations.Contains(other.gameObject)) destinations.Remove(other.gameObject); //case the agent was allocated automatically or manually through Waiting Area
        }

        //Register Patient (Hospital)
        if (other.gameObject.name.Contains("Hospital") && refugeeStats.GetHealthStatus() != RefugeeStats.Health.Healthy &&
            RefugeeCampGameManager.Instance.medicineResourceNum > 0 &&
            other.gameObject.GetComponent<HospitalStats>().occupants.Count < other.gameObject.GetComponent<HospitalStats>().maxCapacity)
                refBehavior.Heal(other.gameObject);

        //Register Customer (Food Court)
        if (other.gameObject.name.Contains("Food") && refugeeStats.GetHungerStatus() != RefugeeStats.Hunger.Full &&
            RefugeeCampGameManager.Instance.foodResourceNum > 0 &&
            other.gameObject.GetComponent<FoodCourtStats>().occupants.Count < other.gameObject.GetComponent<FoodCourtStats>().maxCapacity)
                refBehavior.Eat(other.gameObject);


        //Register Encountered Agent (Refugee)
        if (other.gameObject.tag.Contains("Refugee") && other.gameObject.Equals(socializingDestination)){
            //Debug.Log("Reached Socializing Agent");
            if (!other.gameObject.GetComponent<RefugeeBehavior>().ReceivePing(gameObject)) refBehavior.ResetSocializing();
        }
    }

    void OnTriggerStay(Collider other){
        //Awaiting timer to finish countdown and proceed with wandering / socializing
        if (other.gameObject.name.Contains("Waiting") || other.gameObject.name.Contains("Tent")){
            if (other.gameObject.Equals(refugeeStats.GetHome()) && !isAvailable){
                refBehavior.CanLeavehome(); //returns bool if ready or not to leave home.
            }
        }

        //Continue Healing process (until done)
        if (other.gameObject.name.Contains("Hospital")){
            if (other.gameObject.GetComponent<HospitalStats>().occupants.Contains(gameObject)) refBehavior.Heal(other.gameObject);
            else RemoveDestination(other.gameObject);
        }

        //Continue Eating process (until done)
        if (other.gameObject.name.Contains("Food")){
            if (other.gameObject.GetComponent<FoodCourtStats>().occupants.Contains(gameObject)) refBehavior.Eat(other.gameObject);
            else RemoveDestination(other.gameObject);
        }

        //Proceed with Socializing of the 2 agents.
        if (other.gameObject.tag.Contains("Refugee") && other.gameObject.Equals(socializingDestination)){
            if (socializingDestination != null){
                //Debug.Log("Socializing");
                nav.isStopped = true;
                refBehavior.Socialize(socializingDestination);
            }
            else refBehavior.ResetSocializing(); //maybe not needed
        }
    }

    void OnTriggerExit(Collider other){
        //Deregister Socializing agent and flags
        if (other.gameObject.Equals(socializingDestination)) refBehavior.ResetSocializing(); //maybe not needed
    }

    //USER DEFINED FUNCTIONS

    //Wander Mode (Agents after settling in a House (Waiting Area / Tent) will wander around the camp/ Socializing or simply walking)
    public void Wander()
    {
        if (GetComponent<RefugeeStats>().GetHome() != null)
        {
            Vector3 homePos = GetComponent<RefugeeStats>().GetHome().transform.position;
            nav.SetDestination(homePos + new Vector3(Random.Range(-100.0f, 100.0f), 0.0f, Random.Range(-100.0f, 100.0f)));
        }

        //Random Trigger Go Home
        if (isAvailable && refBehavior.GetStayingHomeCooldown() >= (int)Time.time){
            isAvailable = refBehavior.SetStayingHomeCooldown((int)Time.time);
        }

    }

    //Main Navigation Check
    void Navigate(){
        nav.speed = agentSpeed * RefugeeCampGameManager.Instance.tSpeedVar * RefugeeCampGameManager.Instance.targetSpeedModifier; //Maintain the agent speed

        //Destinations Exist (Destinations added to visit Hospital / Food Court and reallocate to housing facility ONLY)
        if (destinations.Count > 0){
            canWander = false;
            if (ContinueNavigation) nav.SetDestination(destinations[0].transform.position);
         
            //DEAD
            if (refugeeStats.GetHealthStatus() == RefugeeStats.Health.Dead){
                if (Vector3.Distance(transform.position, destinations[0].transform.position) < 1.0f){
                    CleanupRefugeeRegistry();
                    Destroy(gameObject);
                }
            }
        }
        else{
            if (refugeeStats.GetHome() != null){

                //Enable wander mode when agents are not required to fulfill hunger or medical state and not socializing already
                if (!canWander && destinations.Count == 0 && socializingDestination == null && isAvailable) canWander = true;

                //Case when game is paused or review refugee entries.
                if (canWander && RefugeeCampGameManager.Instance.tSpeedVar > 0) Wander();
            }
        }
    }

    //Navigation Routines
    public void AddNextLocation(GameObject nextTarget){
        if(!destinations.Contains(nextTarget))
            destinations.Add(nextTarget);
    }

    public void AddManualLocation(bool flag = false){

        isManualTargeting = flag;
        if (isManualTargeting){
            Camera.main.GetComponent<CameraControl>().SetInteractionFlag(false);

            if (!lineRend.enabled) lineRend.enabled = true; //Enable Line Renderer to Visualize Destination 
            gameObject.GetComponent<RefugeeInteractor>().DisableActionMenu(); //Close Menu On Interaction. (Global)
            lineRend.SetPosition(0, transform.position); //Set Start Of Line

            //Set Up for Destination Direction of Line
            float mousePosX = Input.mousePosition.x;
            float mousePosY = Input.mousePosition.y;
            Ray originWorldPos = Camera.main.GetComponent<Camera>().ScreenPointToRay(new Vector3(mousePosX, mousePosY, 0.1f));

            //Force acquire objFocus from Camera
            objFocusGO = Camera.main.GetComponent<CameraControl>().camFocus;
            objFocusGO.SetActive(true); 
            lineRend.SetPosition(1, objFocusGO.transform.position); //Set End of Line


            //Raycast for Target Acquisition
            if (Physics.Raycast(originWorldPos.origin, originWorldPos.direction * 40.0f, out RaycastHit hitInfo)){
                if (hitInfo.collider.name.Contains("Tent") && (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))){
                    lineRend.SetPosition(1, hitInfo.collider.gameObject.transform.position); //Set End of Line On target
                    if (hitInfo.collider.gameObject?.GetComponent<TentStats>().occupants.Count < hitInfo.collider.gameObject?.GetComponent<TentStats>().maxCapacity && GetComponent<RefugeeStats>().GetHomeStatus() != RefugeeStats.HomeStatus.Resident){
                        AddNextLocation(hitInfo.collider.gameObject); //Add to Navigation Plan
                        GetComponent<RefugeeStats>().SetHomeStatus(RefugeeStats.HomeStatus.Resident, hitInfo.collider.gameObject); //Update Stats
                        hitInfo.collider.gameObject?.GetComponent<TentStats>().AddResident(gameObject); //Update Tent Residence
                        lineRend.enabled = false; //disable line renderer after executing
                        WaitingAreaStats waStats = FindObjectOfType<WaitingAreaStats>();
                        if (waStats != null) waStats.RemoveResident(gameObject);
                    }
                    isManualTargeting = false;
                }
                if(!hitInfo.collider.name.Contains("Tent") && (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))) {
                    refBehavior.ResetStayingHomeCooldown(); //Reset home staying process on Allocation
                    refBehavior.ResetSocializing(); //Reset socializing process on Allocation
                    lineRend.enabled = false;
                    isManualTargeting = false;
                }
            }
        }
        else{
            if (lineRend.enabled){
                lineRend.enabled = false;
            }
            Camera.main.GetComponent<CameraControl>().SetInteractionFlag(true);
        }
    }

    public void RemoveDestination(GameObject target){
        if (destinations.Contains(target)){
            destinations.Remove(target);
        }
    }
    // /////////////////////

    //Attritutes Check///////////////////////////////

    //Seek a Hospital
    public void CheckHealth(int alertLevel){
        if (alertLevel != RefugeeStats.Health.Healthy.GetHashCode() && RefugeeCampGameManager.Instance.medicineResourceNum > 0){
            for (int i = 0; i < structureTargets.Length; i++){
                if (structureTargets[i].name.Contains("Hospital")){
                    if (!destinations.Contains(structureTargets[i]) && structureTargets[i].GetComponent<HospitalStats>().occupants.Count < structureTargets[i].GetComponent<HospitalStats>().maxCapacity){
                        destinations.Add(structureTargets[i]);
                        break;
                    }
                }
            }
        }
    }

    //Seek a Food Court
    public void CheckHunger(int alertLevel){
        if (alertLevel != RefugeeStats.Hunger.Full.GetHashCode() && RefugeeCampGameManager.Instance.foodResourceNum > 0){
            for (int i = 0; i < structureTargets.Length; i++){
                if (structureTargets[i].name.Contains("FoodCourt")){
                    if (!destinations.Contains(structureTargets[i]) && structureTargets[i].GetComponent<FoodCourtStats>().occupants.Count < structureTargets[i].GetComponent<FoodCourtStats>().maxCapacity)
                    {
                        destinations.Add(structureTargets[i]);
                        break;
                    }
                }
            }
        }
    }

    //Register Home
    public void CheckHouseStatus(int familyId = -1){
        GameObject[] structures = GameObject.FindGameObjectsWithTag("Structure");
        List<GameObject> tentList = new List<GameObject>();
        List<GameObject> waitingAreaList = new List<GameObject>();

        //Acquire Family Size offset to calculate capacity offset for availability in tents.
        if(familyId != -1 && familyOffsetCount == 0) familyOffsetCount = FindObjectOfType<SpawnManager>().GetRefugeesSpawnNumber();

        //Acquire List Of Tents.
        for (int i = 0; i < structures.Length; i++){
            if (structures[i].name.Contains("Tent") && ((structures[i]?.GetComponent<TentStats>().occupants.Count + familyOffsetCount) < structures[i]?.GetComponent<TentStats>().maxCapacity)){
                tentList.Add(structures[i]); 
            }
        }
        
        //Acquire List of Waiting Areas (if more than 1)
        for (int j = 0; j < structures.Length; j++){
            if (structures[j].name.Contains("WaitingArea")){
                waitingAreaList.Add(structures[j]);
            }
        }


        //Go to Waiting Area if No Housing or slots in housing are all occupied
        if (waitingAreaList.Count > 0 && tentList.Count == 0 && refugeeStats.GetHome() == null){
            int waitingAreaIndex = 0;
            if (waitingAreaList.Count > 1) waitingAreaIndex = Random.Range(0, waitingAreaList.Count + 1);
            destinations.Add(waitingAreaList[waitingAreaIndex]);
            //Declare Temporary Housing
            GetComponent<RefugeeStats>().SetHomeStatus(RefugeeStats.HomeStatus.Homeless, waitingAreaList[waitingAreaIndex]);
            return;
        }


        //Pick Random Tent -- Not Occupied and Auto Home Allocation is enabled
        if (tentList.Count > 0 && !manualHomeAllocation){
            
            //Refugee - Solo
            if (familyId == -1) {
                //Assign Destination
                int index = Random.Range(0, tentList.Count); //random index in list
                destinations.Add(tentList[index]); //add destination to queue
                //Declare the Home Tent
                GetComponent<RefugeeStats>().SetHomeStatus(RefugeeStats.HomeStatus.Resident, tentList[index]);
                tentList[index]?.GetComponent<TentStats>().AddResident(gameObject);
                return;
            }
            else {//Refugee - Family

                //Assign Destination
                if (refugeeStats.GetHome() == null) {
                
                    //Set the 1st's family member spot as Tenant
                    int index = Random.Range(0, tentList.Count); //random index in list
                    GetComponent<RefugeeStats>().SetHomeStatus(RefugeeStats.HomeStatus.Resident, tentList[index]);
                    tentList[index]?.GetComponent<TentStats>().AddResident(gameObject);

                    //Family Tent Assignment (assign same place for the rest of the family members awaiting allocation)
                    foreach (GameObject familyMember in FindObjectOfType<SpawnManager>().GetRefugeesFamily()){
                        if (familyMember.GetComponent<RefugeeStats>().GetHomeStatus() == RefugeeStats.HomeStatus.Homeless){
                            familyMember.GetComponent<RefugeeStats>().SetHomeStatus(RefugeeStats.HomeStatus.Resident, tentList[index]);
                            tentList[index]?.GetComponent<TentStats>().AddResident(familyMember);
                        }
                    }
                    return;
                }
            }
        }
    }
    // //////////////////////////////////////////////

    //Upon instantiation in the scene we check if the agent is accepted or rejected and Destroy the instance or procceed as usual.

    //Set Accepted flag
    public void SetAgentAccepted(bool flag){
        isAgentAccepted = flag;
    }

    //Get Accepted flag
    public bool GetAgentAccepted(){
        return isAgentAccepted;
    }
    // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
}

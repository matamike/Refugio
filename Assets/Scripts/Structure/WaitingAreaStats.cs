using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingAreaStats : MonoBehaviour{

    public int[] upgradeLevels = new int[2] { 1, 2 }; //level index
    public int[] capacityLevels = new int[2] { 80, 120 }; //capacity per level 
    public int[] upgradeLevelCosts = new int[2] { 4500, 6500 }; //upgrade cost per level
    public int[] upgradeUtilBaseCosts = new int[3] {1, 300, 450 }; //utility cost per level

    public Mesh[] upgradeMeshes = new Mesh[2];


    public int currentLevel; //default 0 (base)
    public int maxCapacity; //default 40 (base)

    public List<GameObject> occupants = new List<GameObject>(); //tenants
    public Queue<GameObject> excessOccupants = new Queue<GameObject>(); //exceeding limits occupants

    public int flushInterval = 10; //refresh occupants offset timestamp check. //30
    private float timeStamp = 0.0f; //timestamp

    public int utilitiesIntervalPayment = 45; // apply utility costs to Game Manager //65
    private float utilityTimestamp = 0.0f; //utility payment timestamp

    public int baseUtilCost = 35; //(base cost)
    private int totalUtilCosts = 0; //init 0 for payment of utilities

    //Visual Parts for Upgrades.
    public GameObject meshParent;
    public List<GameObject> submeshes = new List<GameObject>();

    //NavMesh Obstacles Child
    public GameObject navMeshObstaclesParent;

    void Awake(){
        //Find ChildHolder
        meshParent = gameObject.transform?.Find("RenderMesh").gameObject;

        //Find NavMeshObstacles Container
        navMeshObstaclesParent = gameObject.transform?.Find("NavMeshObstacles").gameObject;

        maxCapacity = 40;
        currentLevel = 0;
    }

    void Start(){
        timeStamp = (int)Mathf.Floor(Time.time);
        utilityTimestamp = utilitiesIntervalPayment;
    }

    void Update(){
        //Time dependent routines
        if ((int)Mathf.Floor(Time.time) >= timeStamp) RefreshOccupants(); //Renew List and Remove any null or missing references.

        if ((int)Mathf.Floor(Time.time) >= utilityTimestamp) ApplyUtilityCosts(); //Charge the amount of money for utilities
    }

    private void OnTriggerEnter(Collider other){
        if (other.gameObject.tag == "Refugee"){   
            if(other.gameObject.GetComponent<RefugeeStats>().GetHome() == null || other.gameObject.GetComponent<RefugeeStats>().GetHome() == this.gameObject)
            {//if (other.gameObject.GetComponent<RefugeeStats>().GetHomeStatus() is RefugeeStats.HomeStatus.Homeless){ //maybe change
                AddResident(other.gameObject);
                UpdateOccupants();
                UpdateUtilityCosts();
            }
        }
    }

    private void OnTriggerExit(Collider other){
        if (other.gameObject.tag == "Refugee" && other.gameObject.GetComponent<RefugeeStats>().GetHome() != this.gameObject){ //maybe change
            RemoveResident(other.gameObject);
            UpdateOccupants();
            UpdateUtilityCosts();
        }
    }



    //USER DEFINED FUNCTIONS (Waiting Area Usage)

    //Upgrade Handling  Function
    public void Transaction(){
        if (currentLevel == 0){
            if (RefugeeCampGameManager.Instance.money >= upgradeLevelCosts[currentLevel]){

                RefugeeCampGameManager.Instance.UpdateMoney("-", upgradeLevelCosts[currentLevel]); // subtract money for upgrade
                maxCapacity = capacityLevels[currentLevel]; //update max capacity



                //TODO PUT THE CODE FOR MESH UPGRADES WHEN MODELS ARE READY
                //Assigns (Temporarily Primitives) new mesh to the object according to the level
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                this.gameObject.GetComponent<MeshFilter>().mesh = go.GetComponent<MeshFilter>().sharedMesh;
                Destroy(gameObject.GetComponent<Collider>());
                gameObject.AddComponent(go.GetComponent<Collider>().GetType());
                Destroy(go);
                ///////////////////////////////////////////////////////////////////////////////

                //Update UI elements in Tent Object
                currentLevel += 1; //assign next level
                return;
            }
        }
        if (currentLevel == 1){
            if (RefugeeCampGameManager.Instance.money >= upgradeLevelCosts[currentLevel]){
                RefugeeCampGameManager.Instance.money -= upgradeLevelCosts[currentLevel]; // subtract money for upgrade
                maxCapacity = capacityLevels[currentLevel]; //update max capacity


                //TODO PUT THE CODE FOR MESH UPGRADES WHEN MODELS ARE READY
                //Assigns (Temporarily Primitives) new mesh to the object according to the level
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                this.gameObject.GetComponent<MeshFilter>().mesh = go.GetComponent<MeshFilter>().sharedMesh;
                Destroy(gameObject.GetComponent<Collider>());
                gameObject.AddComponent(go.GetComponent<Collider>().GetType());
                Destroy(go);
                ///////////////////////////////////////////////////////////////////////////////

                //Update UI elements in Tent Object
                currentLevel += 1; //assign next level
                return;
            }
        }
        return;
    }

    //Agents Management 

    // Add New Incoming Residents in Waiting area [regular pool / excess pool]
    public void AddResident(GameObject resident){
        //Add residents to normal capacity
        if (occupants.Count <= maxCapacity && !occupants.Contains(resident)) occupants.Add(resident);
        
        //Add resident to exceeding capacity
        if(occupants.Count > maxCapacity && !occupants.Contains(resident)){
            if (!excessOccupants.Contains(resident)) excessOccupants.Enqueue(resident);
        }

        //Update UI properties
        if (RefugeeCampGameManager.Instance.actionListActiveGO == this.gameObject && RefugeeCampGameManager.Instance.GetStructureButtonActionsGO().activeInHierarchy){
            UpdateOccupants();
            UpdateUtilityCosts();
        }
    }

    public void RemoveResident(GameObject resident){

        //Remove from Regular pool
        if (occupants.Contains(resident)){
            occupants.RemoveAt(occupants.IndexOf(resident));
        }

        //Remove from Excess pool
        if (excessOccupants.Contains(resident)){
            int excessCounter = excessOccupants.Count;
            List<GameObject> excessResidents = new List<GameObject>();

            for(int iter=0; iter < excessCounter; iter++) excessResidents.Add(excessOccupants.Dequeue());
            excessResidents.Remove(resident);

            foreach (GameObject excRes in excessResidents) excessOccupants.Enqueue(excRes);
        }

        if (RefugeeCampGameManager.Instance.actionListActiveGO == this.gameObject && RefugeeCampGameManager.Instance.GetStructureButtonActionsGO().activeInHierarchy){
            UpdateOccupants();
            UpdateUtilityCosts();
        }
    }

    //Allocate Automatically excess residents to regular pool if there's spot every X seconds.
    private void RefreshOccupants(){
        GameObject[] occupantsArr = occupants.ToArray();
        if(occupantsArr.Length != occupants.Count){
            occupants.Clear();
            foreach (GameObject go in occupantsArr){
                if (go.GetComponent<RefugeeStats>().GetHome().name.Contains("Waiting")) occupants.Add(go);
                else continue;
            }
        }
        

        //Scenario to Allocate excess number of occupants to normal pool (if there's available spot)
        if (occupants.Count < maxCapacity && excessOccupants.Count > 0){
            int freeSpots = maxCapacity - occupants.Count; // free spots in normal pool

            for (int i = 0; i < freeSpots; i++){
                if (excessOccupants.Count > 0) occupants.Add(excessOccupants.Dequeue());
            }
        }

        //Update Timestamp
        timeStamp = (int)Mathf.Floor(Time.time) + flushInterval;
    }

    private void ApplyUtilityCosts(){       
        //Debug.Log("Applying Utility Costs");
        totalUtilCosts =  upgradeUtilBaseCosts[currentLevel] + upgradeUtilBaseCosts[currentLevel] + baseUtilCost;
        //NotificationManager.Instance.CreateNotification("Applying Utility Costs. Cost: " + totalUtilCosts.ToString(), "Manager");
        RefugeeCampGameManager.Instance.UpdateMoney("-", totalUtilCosts);
        //occupants.Count *
        //Update Timestamp
        utilityTimestamp = (int)Mathf.Floor(Time.time) + utilitiesIntervalPayment;
    }
    //////////////////////////////////////////////////////////////////////////////////////////////

    //UI Update Functions
    public void UpdateOccupants(){
        if (RefugeeCampGameManager.Instance.actionListActiveGO != null){
            if (RefugeeCampGameManager.Instance.actionListActiveGO.name.Contains("Waiting")){
                RefugeeCampGameManager.Instance.GetOccupantsText().text = "Occupants: " + RefugeeCampGameManager.Instance.actionListActiveGO.GetComponent<WaitingAreaStats>().occupants.Count.ToString() + " | "
                                                  + RefugeeCampGameManager.Instance.actionListActiveGO.GetComponent<WaitingAreaStats>().maxCapacity.ToString();
            }
        }
    }

    public void UpdateUtilityCosts(){
        if (RefugeeCampGameManager.Instance.actionListActiveGO != null){
            if (RefugeeCampGameManager.Instance.actionListActiveGO.name.Contains("Waiting"))
                if (RefugeeCampGameManager.Instance.actionListActiveGO.GetComponent<WaitingAreaStats>().currentLevel < RefugeeCampGameManager.Instance.actionListActiveGO.GetComponent<WaitingAreaStats>().upgradeUtilBaseCosts.Length)
                {
                    totalUtilCosts = upgradeUtilBaseCosts[currentLevel] + upgradeUtilBaseCosts[currentLevel] + baseUtilCost;
                    //RefugeeCampGameManager.Instance.actionListActiveGO.GetComponent<WaitingAreaStats>().occupants.Count *
                    RefugeeCampGameManager.Instance.GetUtilitiesCostText().text = "Utilities: " + totalUtilCosts.ToString();
                }
        }
    }

    //////////////////////////////////

    //Waiting Area Mesh/Collider Functions (UPGRADE SECTION)
    public void SetUpgradeMesh(GameObject[] goMeshes)
    {
        if (meshParent.transform.childCount > 0)
        {
            Destroy(meshParent.transform.GetChild(0).gameObject);
        }

        for (int i = 0; i < goMeshes.Length; i++)
        {
            goMeshes[i].transform.SetParent(meshParent.transform);
        }
    }

    public void SetUpgradeCollider(GameObject goCol)
    {
        //Validate that object mesh used has a mesh collider before acquiring it's geometry.
        if (goCol.GetComponent<Collider>().GetType() == typeof(MeshCollider))
        {
            MeshCollider mshCol = gameObject.GetComponent<Collider>() as MeshCollider;
            mshCol.sharedMesh = goCol.GetComponent<MeshCollider>().sharedMesh;
        }
    }

    public void SetNavMeshObstaclesMesh(GameObject replaceNavMeshObstacles)
    {
        GameObject temp = navMeshObstaclesParent;
        Destroy(temp);

        replaceNavMeshObstacles.transform.SetParent(gameObject.transform); //Assign new NavmeshObstacles
        navMeshObstaclesParent = replaceNavMeshObstacles.transform.gameObject;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodCourtStats : MonoBehaviour{

    public int[] upgradeLevels = new int[2] { 1, 2 }; //level index
    public int[] capacityLevels = new int[2] { 20, 40 }; //capacity per level
    public int[] upgradeLevelCosts = new int[2] { 2500, 4500 }; //upgrade cost per level
    public int[] upgradeUtilBaseCosts = new int[3] {1, 200, 350 }; //utility cost per level

    public int currentLevel; //default 0 (base)
    public int maxCapacity; //default 10 (base)

    public List<GameObject> occupants = new List<GameObject>(); //customers
    public int flushInterval = 10; //refresh occupants offset timestamp check. //30
    private float timeStamp = 0.0f; //timestamp

    public int utilitiesIntervalPayment = 45; // apply utility costs to Game Manager //65
    private float utilityTimestamp = 0.0f; //utility payment timestamp

    public int baseUtilCost = 20; //too nice :P (base cost)
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

        maxCapacity = 10;
        currentLevel = 0;
    }

    void Start(){
        utilityTimestamp = utilitiesIntervalPayment;
    }

    void Update(){
        //Time dependent routines
        if ((int)Mathf.Floor(Time.time) >= utilityTimestamp) ApplyUtilityCosts(); //Charge the amount of money for utilities
    }


    private void OnTriggerEnter(Collider other){
        if (other.gameObject.tag == "Refugee"){
            if (other.gameObject.GetComponent<RefugeeStats>().GetHungerStatus() != RefugeeStats.Hunger.Full){
                UpdateOccupants();
                UpdateUtilityCosts();
            }
        }
    }

    private void OnTriggerExit(Collider other){
        if (other.gameObject.tag == "Refugee"){
            UpdateOccupants();
            UpdateUtilityCosts();
        }
    }


    //USER DEFINED FUNCTIONS (Food Court Usage)

    public void Transaction(){

        if (currentLevel == 0){
            if (RefugeeCampGameManager.Instance.money >= upgradeLevelCosts[currentLevel]){
                RefugeeCampGameManager.Instance.UpdateMoney("-", upgradeLevelCosts[currentLevel]); // subtract money for upgrade
                maxCapacity = capacityLevels[currentLevel]; //update max capacity

                //Assigns New Mesh & Collider to the object according to the level
                GameObject go = Instantiate(Resources.Load("StructureUpgrades/Food_Court/Upgrades/Su_FoodCourt_2")) as GameObject;
                go.transform.position = transform.position; //align world position with invoker instance obj.

                List<GameObject> upgradeMesh = new List<GameObject>();
                upgradeMesh.Add(go.transform.Find("RenderMesh").GetChild(0).gameObject);

                SetUpgradeMesh(upgradeMesh.ToArray()); //Set New Set of Meshes
                SetUpgradeCollider(go); //Setup New Collider
                SetNavMeshObstaclesMesh(go.transform.Find("NavMeshObstacles").gameObject); //Setup New NavMeshObstacles
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

                //Assigns New Mesh & Collider to the object according to the level
                GameObject go = Instantiate(Resources.Load("StructureUpgrades/Food_Court/Upgrades/Su_FoodCourt_3")) as GameObject;
                go.transform.position = transform.position; //align world position with invoker instance obj.

                List<GameObject> upgradeMesh = new List<GameObject>();
                upgradeMesh.Add(go.transform.Find("RenderMesh").GetChild(0).gameObject);

                SetUpgradeMesh(upgradeMesh.ToArray()); //Set New Set of Meshes
                SetUpgradeCollider(go); //Setup New Collider
                SetNavMeshObstaclesMesh(go.transform.Find("NavMeshObstacles").gameObject); //Setup New NavMeshObstacles
                Destroy(go);
                ///////////////////////////////////////////////////////////////////////////////

                //Update UI elements in Tent Object
                currentLevel += 1; //assign next level
                return;
            }
        }
        return;
    }

    public void AddResident(GameObject resident){
        if (occupants.Count < maxCapacity && !occupants.Contains(resident) && RefugeeCampGameManager.Instance.foodResourceNum > 0)
        {
            occupants.Add(resident);
            RefugeeCampGameManager.Instance.UpdateFoodResource("-", 1);
        }
        if (RefugeeCampGameManager.Instance.actionListActiveGO == this.gameObject && RefugeeCampGameManager.Instance.GetStructureButtonActionsGO().activeInHierarchy)
        {
            UpdateOccupants();
            UpdateUtilityCosts();
        }
    }

    public void RemoveResident(GameObject resident){
        if (occupants.Contains(resident)) occupants.Remove(resident);

        if (RefugeeCampGameManager.Instance.actionListActiveGO == this.gameObject && RefugeeCampGameManager.Instance.GetStructureButtonActionsGO().activeInHierarchy){
            UpdateOccupants();
            UpdateUtilityCosts();
        }
    }

    private void RefreshOccupants(){
        GameObject[] occupantsArr = occupants.ToArray(); //Copy list to Array
        occupants.Clear(); //Clear List

        //Update List
        for (int i = 0; i < occupantsArr.Length; i++){
            if (occupantsArr[i] != null) AddResident(occupantsArr[i]);
        }

        //Update Timestamp
        timeStamp = (int)Mathf.Floor(Time.time) + flushInterval;
    }

    private void ApplyUtilityCosts(){
        //Debug.Log("Applying Utility Costs");
        totalUtilCosts = upgradeUtilBaseCosts[currentLevel] + upgradeUtilBaseCosts[currentLevel] + baseUtilCost;
        //NotificationManager.Instance.CreateNotification("Applying Utility Costs. Cost: " + totalUtilCosts.ToString(), "Manager");
        RefugeeCampGameManager.Instance.UpdateMoney("-", totalUtilCosts);
        //occupants.Count *
        //Update Timestamp
        utilityTimestamp = (int)Mathf.Floor(Time.time) + utilitiesIntervalPayment;
    }

    public void UpdateOccupants(){
        if (RefugeeCampGameManager.Instance.actionListActiveGO != null){
            if (RefugeeCampGameManager.Instance.actionListActiveGO.name.Contains("Food")){
                //Label Set
                RefugeeCampGameManager.Instance.GetOccupantsText().text = "Occupants: " + RefugeeCampGameManager.Instance.actionListActiveGO.GetComponent<FoodCourtStats>().occupants.Count.ToString() + " | "
                                                  + RefugeeCampGameManager.Instance.actionListActiveGO.GetComponent<FoodCourtStats>().maxCapacity.ToString();
            }
        }
    }

    public void UpdateUtilityCosts(){
        if (RefugeeCampGameManager.Instance.actionListActiveGO != null){
            if (RefugeeCampGameManager.Instance.actionListActiveGO.name.Contains("Food")){
                if (RefugeeCampGameManager.Instance.actionListActiveGO.GetComponent<FoodCourtStats>().currentLevel < RefugeeCampGameManager.Instance.actionListActiveGO.GetComponent<FoodCourtStats>().upgradeUtilBaseCosts.Length)
                    RefugeeCampGameManager.Instance.GetUtilitiesCostText().text = "Utilities: " + ( upgradeUtilBaseCosts[currentLevel] + upgradeUtilBaseCosts[currentLevel] + baseUtilCost).ToString();
                //(RefugeeCampGameManager.Instance.actionListActiveGO.GetComponent<FoodCourtStats>().occupants.Count *
            }
        }
    }

    //Tent Mesh/Collider Functions (UPGRADE SECTION)
    public void SetUpgradeMesh(GameObject[] goMeshes){
        if (meshParent.transform.childCount > 0){
            Destroy(meshParent.transform.GetChild(0).gameObject);
        }

        for (int i = 0; i < goMeshes.Length; i++){
            goMeshes[i].transform.SetParent(meshParent.transform);
        }
    }

    public void SetUpgradeCollider(GameObject goCol){
        //Validate that object mesh used has a mesh collider before acquiring it's geometry.
        if (goCol.GetComponent<Collider>().GetType() == typeof(MeshCollider)){
            MeshCollider mshCol = gameObject.GetComponent<Collider>() as MeshCollider;
            mshCol.sharedMesh = goCol.GetComponent<MeshCollider>().sharedMesh;
        }
    }

    public void SetNavMeshObstaclesMesh(GameObject replaceNavMeshObstacles){
        GameObject temp = navMeshObstaclesParent;
        Destroy(temp);

        replaceNavMeshObstacles.transform.SetParent(gameObject.transform); //Assign new NavmeshObstacles
        navMeshObstaclesParent = replaceNavMeshObstacles.transform.gameObject;
    }

}

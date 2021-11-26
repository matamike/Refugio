using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
/*
public class SpawnRefugees : Monobehavior{
    public GameObject spawnObject; //Link to Refugee Prefab.
    private GameObject tempRefGO = null; //Refugee Object
    private GameObject propertySelectionPoolGO;
    private int counterID = 0; //works as Refugee ID
    public float duration;

    //Pop Up Reference to store
    GameObject popupIncomingRefugees;
    private GameObject refugeeID; //Link to Refugee ID Prefab.
    private GameObject cardPile; //Link to Pile of Cards Background setup.
    GameObject displayedActiveGO = null;

    //Agreements Tab
    private GameObject agreementsTab = null;


    [SerializeField]
    private bool spawn = false; //Spawn Flag

    [SerializeField]
    private bool entryCheck = true; //Enter camp without Check

    private int refugeesSpawnNumber; //holder of the random number of refugees to spawn.
    public bool hasActivated = false;
    [SerializeField]
    private Queue<GameObject> refugees = new Queue<GameObject>();

    public GameObject[] familyPendingRefugees;
    //tweakable properties
    public float spawnTimeOffset;
    public int minSpawnSize;
    public int maxSpawnSize;

    public int familyMinSpawnSize;
    public int familyMaxSpawnSize;

    public bool isFamily = false;

    //Time related variables
    public float timestamp = 10.0f;

    //public Light[] debugLights;

    public int targetTimeScale = 1;

    void Awake()
    {
        refugeeID = GameObject.Find("RefugeeIDCard");
        cardPile = GameObject.Find("BlurredPile");
        propertySelectionPoolGO = GameObject.Find("RefugeePropertySelectionPoolGO");
        if (refugeeID.activeInHierarchy) refugeeID.SetActive(false);
        if (cardPile.activeInHierarchy) cardPile.SetActive(false);
    }


    void Start()
    {
        targetTimeScale = RefugeeCampGameManager.Instance.UIClockController.speedModifierValue;
        duration = 30.0f; //  1 minute
        //timestamp = (int)Mathf.Floor(Time.time); //init with the first tick for the first run
        if (spawnObject is null) spawn = false; //Disable spawner if no GO is attached.
    }

    void Update()
    {
        if (!entryCheck)
        {
            if ((int)Mathf.Floor(Time.time) >= timestamp && spawn) SpawnRefugee();
        }
        else
        {
            if ((int)Mathf.Floor(Time.time) >= timestamp && spawn)
            {
                //Initialize the process for Incoming Refugees
                if (!hasActivated)
                {
                    //debugLights[0].color = Color.green; //DEBUG
                    ActivateRefugeeIncomingProcess();

                    //Trigger The Dialogue to Pop up
                    //IncomingRefugeesAlertPopUp();

                    //Generate Number of Agents and decide individuals or family randomly.
                    //GenerateRefugeesNumber(); 

                    //Set number of Refugees awaiting confimation
                    //SetIncomingRefugeesTextCount(refugeesSpawnNumber);

                    //Button Setup
                    //IncomingRefugeesAlertPopUpButtonSetUp();
                }
                // Init/Update Timer
                UpdateIncomingRefugeesEntryTimer();

                // Reduce Time Scale close to 0
                if (RefugeeCampGameManager.Instance.tSpeedVar != 0) RefugeeCampGameManager.Instance.tSpeedVar = Mathf.MoveTowards(RefugeeCampGameManager.Instance.tSpeedVar, 0, 1.0f * Time.deltaTime);
            }
            else
            {
                // Reset Time Scale to last one used
                if (RefugeeCampGameManager.Instance.tSpeedVar != 1) RefugeeCampGameManager.Instance.tSpeedVar = Mathf.MoveTowards(RefugeeCampGameManager.Instance.tSpeedVar, 1, 0.25f * Time.deltaTime);
            }
        }
    }

    //Pause Time -TODO REMOVE
    //void PauseTime(){

    //}

    // Clear Containers of Properties for incoming Refugees
    void ClearIncomingRefugeesStats()
    {

        refugees.Clear(); //Clear the queue of any remaining refugees
    }

    //Use for every refugee to move the index after review them (Accept/Dismiss) 
    void UpdateCard(GameObject refugee)
    {
        //Update the individual refugee info
        RefugeeStats refStats = refugee?.GetComponent<RefugeeStats>();
        TextMeshProUGUI[] refugeeIDTextProperties = refugeeID.GetComponentsInChildren<TextMeshProUGUI>();

        //Image icon = refugeeID.GetComponentsInChildren<Image>(); //TODO REMOVE (Maybe not use)

        foreach (TextMeshProUGUI txt in refugeeIDTextProperties)
        {
            //if (txt.gameObject.name.Contains("NameValue")) txt.GetComponentInChildren<TextMeshProUGUI>().text = refStats.RefugeeName;
            if (txt.gameObject.name.Contains("FullNameValue")) txt.GetComponentInChildren<TextMeshProUGUI>().text = refStats.RefugeeName.Trim() + " " + refStats.RefugeeSurname.Trim();
            if (txt.gameObject.name.Contains("GenderValue"))
            {
                if (refStats.RefugeeGender == 0) txt.GetComponentInChildren<TextMeshProUGUI>().text = "F";
                else txt.GetComponentInChildren<TextMeshProUGUI>().text = "M";
            }
            if (txt.gameObject.name.Contains("PlaceOfBirthValue")) txt.GetComponentInChildren<TextMeshProUGUI>().text = refStats.RefugeeBirthplace;
            if (txt.gameObject.name.Contains("DateOfBirthValue")) txt.GetComponentInChildren<TextMeshProUGUI>().text = refStats.RefugeeBirthDate.x.ToString("00") + " " + refStats.RefugeeBirthDate.y.ToString("00") + " " + refStats.RefugeeBirthDate.z;
            if (txt.gameObject.name.Contains("ProfessionValue")) txt.GetComponentInChildren<TextMeshProUGUI>().text = refStats.RefugeeProfession;
            if (txt.gameObject.name.Contains("ConditionsValue")) txt.GetComponentInChildren<TextMeshProUGUI>().text = refStats.GetConditionsText(); //under discussion TODO Later
        }

        //Family ????
    }

    //Enable the Process of Refugee Entry 
    void ActivateRefugeeIncomingProcess()
    {
        //Flag to prevent constant calls in Update
        hasActivated = true;

        //Trigger The Dialogue to Pop up
        IncomingRefugeesAlertPopUp();
    }

    //Enable Pop up for Entry Check of Refugees
    void IncomingRefugeesAlertPopUp()
    {
        popupIncomingRefugees = RefugeeCampGameManager.Instance.GetSpawnRefugeesNotificationPopUp();
        agreementsTab = RefugeeCampGameManager.Instance.GetAgreementsAndRulesGO();
        if (!popupIncomingRefugees.activeInHierarchy) popupIncomingRefugees.SetActive(true); //Set Active
        if (!agreementsTab.activeInHierarchy) agreementsTab.SetActive(true); //Set Active
        //debugLights[1].color = Color.green; //DEBUG

        //Generate Number of Agents and decide individuals or family randomly. (Async Operation)
        GenerateRefugeesNumber();

        //Set number of Refugees awaiting confimation (Async Operation)
        SetIncomingRefugeesTextCount(refugeesSpawnNumber);

        //Button Setup (Async Operation)
        IncomingRefugeesAlertPopUpButtonSetUp();
    }

    void GenerateRefugeesNumber()
    {
        //Decide whether we should spawn a Family or not
        float num = Random.Range(0.0f, 1.0f);

        if (num > 0.8f) isFamily = true; //20% chance to get a Family
        else isFamily = false; // 80% chance to get a pack of individuals
        // //////////////////////////////////////////////////////////////

        //Create Agents (Scenario of Invididuals or Families)
        if (!isFamily)
        {
            //INDIVIDUALS ONLY    
            GenerateIncomingAgents(refugeesSpawnNumber);
        }
        else
        {
            //FAMILY ONLY
            GenerateIncomingAgentsFamily(refugeesSpawnNumber);
        }



        //debugLights[2].color = Color.green; //DEBUG
        // //////////////////////////////////////////////////////////////
    }

    //Pop up Dialog Button Setup(Review, Accept All, Reject All)
    async void IncomingRefugeesAlertPopUpButtonSetUp()
    {
        bool isDone = false;

        if (popupIncomingRefugees?.GetComponentsInChildren<Button>() != null)
        {
            Button[] incomingRefugeesPopUpButtons = popupIncomingRefugees.GetComponentsInChildren<Button>();
            foreach (Button button in incomingRefugeesPopUpButtons)
            {
                if (button.gameObject.name.Contains("Accept")) button.onClick.AddListener(AcceptAll);
                if (button.gameObject.name.Contains("Reject")) button.onClick.AddListener(DismissAll);
                if (button.gameObject.name.Contains("Review")) button.onClick.AddListener(() => { Review(); button.onClick.RemoveAllListeners(); });
            }

            isDone = true;
        }

        while (!isDone) await Task.Yield();
    }

    //Create Agents and Assign Properties (INDIVIDUALS ONLY)
    async void GenerateIncomingAgents(int count)
    {
        GameObject x = null;
        bool isDone = false;

        //Experimental 
        refugeesSpawnNumber = Random.Range(minSpawnSize, maxSpawnSize + 1); //Random in range number
        count = refugeesSpawnNumber;
        // /////

        //Instantiate and Add to Queue 
        for (int i = 0; i < count; i++)
        {
            refugees.Enqueue(x = Instantiate(spawnObject, this.transform.position + new Vector3(Random.Range(0.2f, 1.2f), 0.0f, Random.Range(0.2f, 1.2f)), Quaternion.identity)); //Create Agent

            //Fill properties
            if (x?.GetComponent<RefugeeStats>())
            {
                RefugeeStats refStats = x.GetComponent<RefugeeStats>();
                refStats.RefugeeGender = Random.Range(0, 2); // Random Pick Gender for Individuals
                //Call Generator (For Individuals Only)

                //Male
                if (refStats.RefugeeGender == 0)
                {
                    List<(string, string)> id = propertySelectionPoolGO?.GetComponent<RefugeePropertiesSelectionPool>().GenerateProfile(0);
                    for (int j = 0; j < id.Count; j++)
                    {
                        if (id[j].Item1.Contains("Name")) refStats.RefugeeName = " " + id[j].Item2 + " ";
                        if (id[j].Item1.Contains("Surname")) refStats.RefugeeSurname = " " + id[j].Item2 + " ";
                        if (id[j].Item1.Contains("Birthplace")) refStats.RefugeeBirthplace = id[j].Item2;
                        if (id[j].Item1.Contains("Profession")) refStats.RefugeeProfession = id[j].Item2;
                    }
                    refStats.RefugeeBirthDate = new Vector3(Random.Range(System.DateTime.MinValue.Date.Day, System.DateTime.MaxValue.Date.Day + 1),
                                        Random.Range(System.DateTime.MinValue.Date.Month, System.DateTime.MaxValue.Date.Month + 1),
                                        Random.Range(1921, 2022));
                }

                //Female
                if (refStats.RefugeeGender == 1)
                {
                    List<(string, string)> id = propertySelectionPoolGO?.GetComponent<RefugeePropertiesSelectionPool>().GenerateProfile(1);
                    for (int j = 0; j < id.Count; j++)
                    {
                        if (id[j].Item1.Contains("Name")) refStats.RefugeeName = id[j].Item2;
                        if (id[j].Item1.Contains("Surname")) refStats.RefugeeSurname = id[j].Item2;
                        if (id[j].Item1.Contains("Birthplace")) refStats.RefugeeBirthplace = id[j].Item2;
                        if (id[j].Item1.Contains("Profession")) refStats.RefugeeProfession = id[j].Item2;
                    }
                    refStats.RefugeeBirthDate = new Vector3(Random.Range(System.DateTime.MinValue.Date.Day, System.DateTime.MaxValue.Date.Day + 1),
                                        Random.Range(System.DateTime.MinValue.Date.Month, System.DateTime.MaxValue.Date.Month + 1),
                                        Random.Range(1921, 2022));
                }
            }
            else print("Agent is Missing <RefugeeStats> member");

            if (refugees.Count == count) isDone = true;
        }

        while (!isDone) await Task.Yield();
    }

    //Create Agents and Assign Properties (FAMILY ONLY)
    async void GenerateIncomingAgentsFamily(int count)
    {
        int numOfKids = count - 2; //Kids count 
        bool isDone = false;

        //Generate PlaceHolders for Family.
        GameObject father = null; //mother GO
        GameObject mother = null; //father GO
        List<GameObject> kids = new List<GameObject>(); //Kids GO List

        //for (int k = 0; k < numOfKids; k++) kids.Add(null); //init kids null.

        GameObject x = null; //temp instantiated GO holder

        //Experimental 
        refugeesSpawnNumber = Random.Range(familyMinSpawnSize, familyMaxSpawnSize + 1); //Random in range number
        count = refugeesSpawnNumber;
        // /////

        //Instantiate and Add to Queue 
        for (int i = 0; i < count; i++)
        {
            refugees.Enqueue(x = Instantiate(spawnObject, this.transform.position + new Vector3(Random.Range(0.2f, 1.2f), 0.0f, Random.Range(0.2f, 1.2f)), Quaternion.identity)); //Create Agent

            //Fill properties
            if (x?.GetComponent<RefugeeStats>())
            {

                if (father is null && x != null)
                {
                    RefugeeStats refStats = x.GetComponent<RefugeeStats>();
                    refStats.RefugeeGender = 0;

                    //Male (Father)
                    List<(string, string)> id = propertySelectionPoolGO?.GetComponent<RefugeePropertiesSelectionPool>().GenerateFamilyProfile(0, false, true);
                    for (int j = 0; j < id.Count; j++)
                    {
                        if (id[j].Item1.Contains("Name")) refStats.RefugeeName = id[j].Item2;
                        if (id[j].Item1.Contains("Surname")) refStats.RefugeeSurname = id[j].Item2;
                        if (id[j].Item1.Contains("Birthplace")) refStats.RefugeeBirthplace = id[j].Item2;
                        if (id[j].Item1.Contains("Profession")) refStats.RefugeeProfession = id[j].Item2;
                    }
                    refStats.RefugeeBirthDate = new Vector3(Random.Range(System.DateTime.MinValue.Date.Day, System.DateTime.MaxValue.Date.Day + 1),
                                        Random.Range(System.DateTime.MinValue.Date.Month, System.DateTime.MaxValue.Date.Month + 1),
                                        Random.Range(1921, 2022));
                    father = x;
                    father.GetComponent<RefugeeStats>().familyMemberID = RefugeeCampGameManager.Instance.familyCounter;
                    x = null;
                }

                if (mother is null && x != null)
                {
                    RefugeeStats refStats = x.GetComponent<RefugeeStats>();
                    refStats.RefugeeGender = 1;

                    //Female (Mother)
                    List<(string, string)> id = propertySelectionPoolGO?.GetComponent<RefugeePropertiesSelectionPool>().GenerateFamilyProfile(1, false, true);
                    for (int j = 0; j < id.Count; j++)
                    {
                        if (id[j].Item1.Contains("Name")) refStats.RefugeeName = id[j].Item2;
                        if (id[j].Item1.Contains("Surname")) refStats.RefugeeSurname = id[j].Item2;
                        if (id[j].Item1.Contains("Birthplace")) refStats.RefugeeBirthplace = id[j].Item2;
                        if (id[j].Item1.Contains("Profession")) refStats.RefugeeProfession = id[j].Item2;
                    }
                    refStats.RefugeeBirthDate = new Vector3(Random.Range(System.DateTime.MinValue.Date.Day, System.DateTime.MaxValue.Date.Day + 1),
                                        Random.Range(System.DateTime.MinValue.Date.Month, System.DateTime.MaxValue.Date.Month + 1),
                                        Random.Range(1921, 2022));
                    mother = x;
                    mother.GetComponent<RefugeeStats>().familyMemberID = RefugeeCampGameManager.Instance.familyCounter;
                    x = null;
                }

                if (father != null && mother != null && x != null)
                {
                    RefugeeStats refStats = x.GetComponent<RefugeeStats>();
                    refStats.RefugeeGender = Random.Range(0, 2); // Random Pick Gender for Individuals

                    //Boy
                    if (refStats.RefugeeGender == 0)
                    {
                        List<(string, string)> id = propertySelectionPoolGO?.GetComponent<RefugeePropertiesSelectionPool>().GenerateFamilyProfile(0, true, false);
                        for (int j = 0; j < id.Count; j++)
                        {
                            if (id[j].Item1.Contains("Name")) refStats.RefugeeName = id[j].Item2;
                            if (id[j].Item1.Contains("Surname")) refStats.RefugeeSurname = id[j].Item2;
                            if (id[j].Item1.Contains("Birthplace")) refStats.RefugeeBirthplace = id[j].Item2;
                            if (id[j].Item1.Contains("Profession")) refStats.RefugeeProfession = id[j].Item2;
                        }
                        refStats.RefugeeBirthDate = new Vector3(Random.Range(System.DateTime.MinValue.Date.Day, System.DateTime.MaxValue.Date.Day + 1),
                                            Random.Range(System.DateTime.MinValue.Date.Month, System.DateTime.MaxValue.Date.Month + 1),
                                            Random.Range(1921, 2022));

                        x.GetComponent<RefugeeStats>().familyMemberID = RefugeeCampGameManager.Instance.familyCounter;
                        kids.Add(x);
                        x = null;
                    }

                    //Girl
                    if (refStats.RefugeeGender == 1)
                    {
                        List<(string, string)> id = propertySelectionPoolGO?.GetComponent<RefugeePropertiesSelectionPool>().GenerateFamilyProfile(1, true, false);
                        for (int j = 0; j < id.Count; j++)
                        {
                            if (id[j].Item1.Contains("Name")) refStats.RefugeeName = id[j].Item2;
                            if (id[j].Item1.Contains("Surname")) refStats.RefugeeSurname = id[j].Item2;
                            if (id[j].Item1.Contains("Birthplace")) refStats.RefugeeBirthplace = id[j].Item2;
                            if (id[j].Item1.Contains("Profession")) refStats.RefugeeProfession = id[j].Item2;
                        }
                        refStats.RefugeeBirthDate = new Vector3(Random.Range(System.DateTime.MinValue.Date.Day, System.DateTime.MaxValue.Date.Day + 1),
                                            Random.Range(System.DateTime.MinValue.Date.Month, System.DateTime.MaxValue.Date.Month + 1),
                                            Random.Range(1921, 2022));


                        x.GetComponent<RefugeeStats>().familyMemberID = RefugeeCampGameManager.Instance.familyCounter;
                        kids.Add(x);
                        x = null;
                    }
                }
            }
            else print("Agent is Missing <RefugeeStats> member");

            if (i == count) isDone = true;
        }
        familyPendingRefugees = refugees.ToArray();

        while (!isDone) await Task.Yield();
    }

    //Update the text with the random amount of refugees to generate.
    async void SetIncomingRefugeesTextCount(int num)
    {
        bool isDone = false;

        TextMeshProUGUI[] texts = popupIncomingRefugees.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts != null)
        {
            foreach (TextMeshProUGUI txt in texts)
            {
                if (txt.gameObject.name.Contains("Number") && !isFamily)
                {
                    txt.text = num.ToString() + " new refugees";
                    isDone = true;
                    break;
                }
                if (txt.gameObject.name.Contains("Number") && isFamily)
                {
                    txt.text = "Family with " + num.ToString() + " new refugees";
                    isDone = true;
                    break;
                }
            }
        }

        while (!isDone) await Task.Yield();

        //debugLights[3].color = Color.green; //DEBUG
    }

    //Update the Timer player needs to act on it.
    void UpdateIncomingRefugeesEntryTimer()
    {
        if (popupIncomingRefugees?.GetComponentInChildren<Slider>() != null)
        {
            Slider slider = popupIncomingRefugees.GetComponentInChildren<Slider>();

            slider.value = Mathf.MoveTowards(slider.value, 0.0f, Time.deltaTime / duration);
            if (slider.value <= 0.0f) DismissAll(); //Remove remaining entries or all (to have an updated list of instances of refugees) 
        }
    }


    //Accept All Agents
    void AcceptAll()
    {
        if (displayedActiveGO != null) displayedActiveGO?.GetComponent<RefugeeNavigation>().SetAgentAccepted(true); //Case where Review is open and GO is not null

        //Enable Navigation when AcceptedALL
        while (refugees.Count != 0)
        {
            GameObject temp = refugees?.Dequeue();

            //Enable Navigation
            if (temp != null && temp?.GetComponent<RefugeeNavigation>()) temp?.GetComponent<RefugeeNavigation>().SetAgentAccepted(true);
            temp.name = "Refugee " + temp.GetComponent<RefugeeStats>().RefugeeName + " " + temp.GetComponent<RefugeeStats>().RefugeeSurname;

            //Empty GameObject
            temp = null;
        }

        //Clear List of incoming Refugees
        ClearIncomingRefugeesStats();

        //Clear button Listeners
        Button[] incomingRefugeesPopUpButtons = popupIncomingRefugees?.GetComponentsInChildren<Button>();
        foreach (Button button in incomingRefugeesPopUpButtons) button.onClick.RemoveAllListeners();

        //Reset Timer Slider
        Slider slider = popupIncomingRefugees?.GetComponentInChildren<Slider>();
        if (slider.value != 1.0f) slider.value = 1.0f;

        //Disable GO
        popupIncomingRefugees.SetActive(false);
        agreementsTab?.SetActive(false);
        if (refugeeID.activeInHierarchy) refugeeID.SetActive(false);

        //Update Timestamp
        targetTimeScale = RefugeeCampGameManager.Instance.UIClockController.speedModifierValue;
        timestamp = (int)Mathf.Floor(Time.time) + (spawnTimeOffset/targetTimeScale);

        hasActivated = false;

        RefugeeCampGameManager.Instance.familyCounter += 1; //Increase Family Counter In any case after accepting all

        //foreach (Light l in debugLights) l.color = Color.red;
    }

    //Remove Any Entries from List and continue the main loop
    void DismissAll()
    {
        if (displayedActiveGO != null) Destroy(displayedActiveGO); //Case where Review is open and GO is not null 

        //Destroy Each Instantiated Agent
        while (refugees.Count != 0)
        {
            GameObject temp = refugees.Dequeue();
            Destroy(temp);
        }

        //Clear List of incoming Refugees
        ClearIncomingRefugeesStats();

        //Clear button Listeners
        Button[] incomingRefugeesPopUpButtons = popupIncomingRefugees?.GetComponentsInChildren<Button>();
        foreach (Button button in incomingRefugeesPopUpButtons) button.onClick.RemoveAllListeners();

        //Reset Timer Slider
        Slider slider = popupIncomingRefugees?.GetComponentInChildren<Slider>();
        if (slider.value != 1.0f) slider.value = 1.0f;

        //Disable GO
        popupIncomingRefugees.SetActive(false);
        agreementsTab?.SetActive(false);
        if (refugeeID.activeInHierarchy) refugeeID.SetActive(false);
        if (cardPile.activeInHierarchy) cardPile.SetActive(false);

        //Update Timestamp
        targetTimeScale = RefugeeCampGameManager.Instance.UIClockController.speedModifierValue;
        timestamp = (int)Mathf.Floor(Time.time) + (spawnTimeOffset / targetTimeScale);

        //Reset process activation flag
        hasActivated = false;

        RefugeeCampGameManager.Instance.familyCounter += 1; //Increase Family Counter In any case after dismissing all

        //foreach (Light l in debugLights) l.color = Color.red;
    }

    void Review()
    {

        //Activate Refugee ID Card
        if (!refugeeID.activeInHierarchy) refugeeID.SetActive(true);
        if (!cardPile.activeInHierarchy) cardPile.SetActive(true);

        //Add Listeners to Accept/Reject Buttons for the GameObject in Queue
        Button[] actions = null;
        actions = refugeeID?.GetComponentsInChildren<Button>();

        foreach (Button button in actions)
        {
            if (button.gameObject.name.Contains("Accept"))
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => { Accept(); });
            }
            if (button.gameObject.name.Contains("Reject"))
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => { Dismiss(); });
            }
        }

        displayedActiveGO = refugees.Dequeue(); //Acquire the 1st Refugee in the Queue
        UpdateCard(displayedActiveGO); // Change the information    
    }

    //Accept Single Refugee
    void Accept()
    {
        RefugeeStats stats = displayedActiveGO?.GetComponent<RefugeeStats>();

        if (stats != null)
        {
            //Debug Print(Accepted) Agent properties.
            stats.DebugRefugeeIDProperties();

            //Enable Navigation
            if (displayedActiveGO != null && displayedActiveGO?.GetComponent<RefugeeNavigation>())
            {
                displayedActiveGO?.GetComponent<RefugeeNavigation>().SetAgentAccepted(true);
                displayedActiveGO.name = "Refugee " + displayedActiveGO.GetComponent<RefugeeStats>().RefugeeName + " " + displayedActiveGO.GetComponent<RefugeeStats>().RefugeeSurname;
                displayedActiveGO = null;
            }
        }

        //Assign Next Agent in Queue to display or terminate
        if (refugees.Count > 0)
        {
            displayedActiveGO = refugees.Dequeue();
            UpdateCard(displayedActiveGO);
        }
        else DismissAll();
    }

    //Dismiss Single Refugee
    void Dismiss()
    {
        RefugeeStats stats = displayedActiveGO?.GetComponent<RefugeeStats>();

        if (stats != null)
        {
            Destroy(displayedActiveGO); //Destroy the individual Refugee
            displayedActiveGO = null; //reset holder
        }

        //Assign Next Agent in Queue to display or terminate
        if (refugees.Count > 0)
        {
            displayedActiveGO = refugees.Dequeue();
            UpdateCard(displayedActiveGO);
        }
        else DismissAll();
    }


    public int GetRefugeesSpawnNumber()
    {
        return refugeesSpawnNumber;
    }

    public GameObject[] GetRefugeesFamily()
    {
        return familyPendingRefugees;
    }


    //Legacy Code /////////////////////////////////

    //Automated Way to Add Refugees (OLD) - Legacy
    void SpawnRefugee()
    {
        int iter = Random.Range(minSpawnSize, maxSpawnSize + 1); //Random Number Selection of Refugees
        for (int i = 0; i < iter; i++)
        {
            tempRefGO = Instantiate(spawnObject, this.transform.position + new Vector3(Random.Range(0.2f, 1.2f), 0.0f, Random.Range(0.2f, 1.2f)), Quaternion.identity); //Spawn Refugees
            tempRefGO.name = "Refugee" + counterID.ToString();
            counterID += 1; //increment the ID
        }
        timestamp = (int)Mathf.Floor(Time.time) + spawnTimeOffset;
    }
}

*/
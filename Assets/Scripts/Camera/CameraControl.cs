using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class CameraControl : MonoBehaviour{
    //Reference to keep
    GameObject craftStation = null; //Object Hit that allows to Craft a Structure (Used in Raycast)
    
    //Objects
    GameObject cam;
    [HideInInspector] public GameObject camFocus; //Object Hovering Over
    [HideInInspector] public GameObject interactingGO;
    [HideInInspector] public string camFocusTargetTag; //get the tag of the GO we look or interacting with.
    [HideInInspector] public float rayLength;
    public List<GameObject> overrideGOs = new List<GameObject>();
    [SerializeField]
    public List<GameObject> disabledStructureGOs = new List<GameObject>();

    [Range(0.0f, 100.0f)]
    public float focusStoppingDistance = 0.0f; 

    //numeric logical 
    public float mouseDragspeed = 80f; //pan and scan speed
    public bool isFocusing = false;
    public bool dragDirNegative = false;
    public AudioClip tent_WaitingSfx, hospitalSfx, foodCourtSfx;

    private Vector3 initialPosition;
    bool isPositionReseting = false;
    bool canOpenInteraction = true;
    void Start(){
        //Object allocation
        cam = GameObject.Find("Main Camera");
        camFocus = GameObject.Find("CameraFocus");
        overrideGOs.Add(GameObject.FindObjectOfType<WaitingAreaStats>().gameObject);
        initialPosition = transform.position;


        //Property assignments
        camFocus.transform.position = Vector3.zero;
        rayLength = 40.0f;
    }


    void Update(){
        PanAndScan(); //MMB Drag Camera Movement  
        DisableColliderOverlay(); //F1 Disable/Enable A structure overlay collider to control manually an agent. 

        //Open/Close Agreemenets Book (Always open when Review is Active)
        if (Input.GetKeyDown(KeyCode.M) && !RefugeeCampGameManager.Instance.GetSpawnRefugeesNotificationPopUp().activeInHierarchy){
            if (RefugeeCampGameManager.Instance.GetAgreementsAndRulesGO().activeInHierarchy) RefugeeCampGameManager.Instance.GetAgreementsAndRulesGO().SetActive(false);
            else RefugeeCampGameManager.Instance.GetAgreementsAndRulesGO().SetActive(true);
        }

        //Reset Camera to center
        if (Input.GetKeyDown(KeyCode.R) || ((Input.GetMouseButton(2) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D)) && isPositionReseting)) {
            isPositionReseting = !isPositionReseting;
        }
        if (isPositionReseting){
            transform.position = Vector3.Slerp(transform.position, initialPosition, 1.0f * Time.deltaTime);
            if (Vector3.Distance(transform.position, initialPosition) < 0.5f) isPositionReseting = false;
        }
        // /////////////////////////////////////////

        //Focus object when hovering over with mouse cursor (Move Camera to Align in X-Axis with targeted object)
        if (Input.GetKeyDown(KeyCode.U) && !isFocusing && camFocusTargetTag.Contains("Structure")) isFocusing = true;
        if (isFocusing) StartCoroutine(CameraTransitionFocus());

        //Action Menu UI Interaction (Structures & Agents)
        if (Input.GetMouseButtonDown(0) && canOpenInteraction){

            //Reset Active Object's Action Panel
            if (interactingGO != null){
                if (interactingGO.gameObject.tag == "Structure" || interactingGO.gameObject.tag == "Refugee")
                    ResetFocusedObject();

                //Enable Action Menus for Refugees or Structures.
                interactingGO?.GetComponent<StructureInteractor>()?.EnableActionMenu(true, interactingGO); //Structures
                //interactingGO?.GetComponent<RefugeeInteractor>()?.EnableActionMenu(true, interactingGO); // Refugees

                //UPDATE ACTION PANEL MENU TEXTS
                if (interactingGO.gameObject.name.Contains("Tent")){
                    interactingGO.gameObject.GetComponent<AudioSource>().PlayOneShot(tent_WaitingSfx);
                    interactingGO.gameObject.GetComponent<TentStats>().UpdateOccupants();
                    interactingGO.gameObject.GetComponent<TentStats>().UpdateUtilityCosts();
                }

                if (interactingGO.gameObject.name.Contains("Hospital")){
                    interactingGO.gameObject.GetComponent<AudioSource>().PlayOneShot(hospitalSfx);
                    interactingGO.gameObject.GetComponent<HospitalStats>().UpdateOccupants();
                    interactingGO.gameObject.GetComponent<HospitalStats>().UpdateUtilityCosts();
                }

                if (interactingGO.gameObject.name.Contains("Food")){
                    interactingGO.gameObject.GetComponent<AudioSource>().PlayOneShot(foodCourtSfx);
                    interactingGO.gameObject.GetComponent<FoodCourtStats>().UpdateOccupants();
                    interactingGO.gameObject.GetComponent<FoodCourtStats>().UpdateUtilityCosts();
                }
                if (interactingGO.gameObject.name.Contains("Waiting")){
                    interactingGO.gameObject.GetComponent<AudioSource>().PlayOneShot(tent_WaitingSfx);
                    interactingGO.gameObject.GetComponent<WaitingAreaStats>().UpdateOccupants();
                    interactingGO.gameObject.GetComponent<WaitingAreaStats>().UpdateUtilityCosts();
                }
            }
        }       
    }

    void FixedUpdate(){
        HighlightInfo();
    }

    public void SetInteractionFlag(bool flag){
        canOpenInteraction = flag;
    }

    public bool GetInteractionFlag(){
        return canOpenInteraction;
    }

    //Reset Active Object's Action Panel and entity (If Active)
    void ResetFocusedObject(){

        //Disable Upgrade Dialogue upon change
        if (RefugeeCampGameManager.Instance.GetUpgradeDialogueGO().activeInHierarchy) RefugeeCampGameManager.Instance.GetUpgradeDialogueGO().SetActive(false);

        //Routine Check on Interaction to swap between Structures and Refugees.
        if (RefugeeCampGameManager.Instance.actionListActiveGO != null){
            GameObject tempActionListActiveGo = RefugeeCampGameManager.Instance.actionListActiveGO; // Get Active Object

            if (tempActionListActiveGo.tag.Contains("Structure")){
                tempActionListActiveGo?.GetComponent<StructureInteractor>().DisableActionMenu();
                RefugeeCampGameManager.Instance.actionListActiveGO = null;
            }

            if (tempActionListActiveGo.tag.Contains("Refugee")){
                //tempActionListActiveGo?.GetComponent<RefugeeInteractor>().DisableActionMenu();
                RefugeeCampGameManager.Instance.actionListActiveGO = null;
            }
        }
    }

    //Enables Hover Over object Overlay with Info about each type of Object
    void HighlightInfo(){
        float mousePosX = Input.mousePosition.x;
        float mousePosY = Input.mousePosition.y;
        Ray originWorldPos = cam.GetComponent<Camera>().ScreenPointToRay(new Vector3(mousePosX, mousePosY, 0.1f));
        Debug.DrawRay(originWorldPos.origin, originWorldPos.direction * rayLength, Color.yellow);

        if (Physics.Raycast(originWorldPos.origin, originWorldPos.direction * rayLength, out RaycastHit hitInfo) && !isFocusing ){

            if (!hitInfo.collider.gameObject.name.Contains("Camera"))
            {
                camFocus.SetActive(true);
                camFocus.transform.position = hitInfo.point; //track the focus of the camera in world.
                camFocusTargetTag = hitInfo.transform.gameObject.tag; //get object tag.

                //Object Focus Highlight Text
                if (camFocusTargetTag.Contains("Structure")){
                    SetObjectFocus(hitInfo.transform.gameObject);
                    interactingGO = hitInfo.transform.gameObject;
                    hitInfo.transform.gameObject.GetComponent<ObjectHighlight>().Highlight(true);
                }
                else if (camFocusTargetTag.Contains("Refugee")){
                    if (hitInfo.collider.gameObject.GetComponent<RefugeeStats>().refName != "" || hitInfo.collider.gameObject.GetComponent<RefugeeStats>().refSurname != ""){
                        SetObjectFocus(hitInfo.transform.gameObject);
                        interactingGO = hitInfo.transform.gameObject;
                        hitInfo.transform.gameObject.GetComponent<ObjectHighlight>().Highlight(true);
                    }
                }
                else
                {
                    interactingGO = null;
                    SetObjectFocus(null);
                }
                /////////////////////////////

                //Crafting Focus Color Highlight
                if (camFocusTargetTag.Contains("Craft") && hitInfo.collider.gameObject?.GetComponent<Renderer>().material != null)
                {
                    if (craftStation is null) craftStation = hitInfo.collider.gameObject;
                    craftStation?.GetComponent<CraftStructure>().ColorFade(true);
                }
                else
                {
                    if (craftStation != null)
                    {
                        if (craftStation?.GetComponent<Renderer>().material.GetColor("_Color").a > 0) craftStation.GetComponent<CraftStructure>().shouldReset = true;
                        else craftStation = null;
                    }
                }
                /////////////////////////////////
            }
        }
        else
        {
            interactingGO = null;
            camFocus.SetActive(false);
            SetObjectFocus(null);
        }
    }

    //Disables /Enables Collider Target --F1 button interaction (MAYBE REMOVE)
    void DisableColliderOverlay(){
        
        // Disable / Enable Waiting Area 
        if (Input.GetKeyDown(KeyCode.F1)){
            foreach(GameObject go in overrideGOs){
                if (go.name.Contains("WaitingArea")){
                    GameObject renderMesh = go.transform.Find("RenderMesh").gameObject;
                    MeshRenderer[] meshes = renderMesh.GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer m in meshes) m.enabled = !m.enabled;
                }
            }
        }
    }

    
    //Disables /Enables All Structures Colliders (TODO EXPLORE IF IT CAUSES ANY ISSUE WHEN AGENTS VISIT STRUCTURES)
    private void DisableColliderOverlayOnMoving(bool flag=default){

        //Find any structures
        if(disabledStructureGOs.Count == 0 || disabledStructureGOs.Count != GameObject.FindGameObjectsWithTag("Structure").Length){
            GameObject[] structures = GameObject.FindGameObjectsWithTag("Structure");
            disabledStructureGOs.Clear(); //flush everything.
            foreach(GameObject structure in structures){
                if(!disabledStructureGOs.Contains(structure)) disabledStructureGOs.Add(structure);
            }

            if (disabledStructureGOs.Count == 0) return; //return if no structures are found.
        }

        // Disable/Enable Colliders
        foreach (GameObject go in disabledStructureGOs){
            if(go != null){
                if (go.TryGetComponent(out MeshCollider meshCol)) meshCol.enabled = !flag;
                if (go.TryGetComponent(out SphereCollider sphCol)) sphCol.enabled = !flag;
                if (go.TryGetComponent(out CapsuleCollider capsCol)) capsCol.enabled = !flag;
            }
        }
        
        
    }
    //Move the Trackball Object with cursor
    void SetObjectFocus(GameObject objOfFocus){
        if (!(objOfFocus is null)) camFocus.transform.position = objOfFocus.transform.position;
    }

    //Routine that transitions from current X Pos of Camera to Highlighted Object
    IEnumerator CameraTransitionFocus(){
        if (Vector2.Distance(new Vector2(transform.position.x,transform.position.z),new Vector2(camFocus.transform.position.x,camFocus.transform.position.z)) > focusStoppingDistance) //Mathf.Abs(transform.position.x - (camFocus.transform.position.x + focusStoppingDistance))
            transform.position = Vector3.Lerp(transform.position, new Vector3(camFocus.transform.position.x, transform.position.y, camFocus.transform.position.z), 1.0f * Time.deltaTime);
        else isFocusing = false;

        yield return new WaitUntil(()=> !isFocusing); //Mathf.Abs(transform.position.x - camFocus.transform.position.x) + Mathf.Abs(transform.position.z - camFocus.transform.position.z)
    }

    private Vector3? _worldDragDeltaAux = null;
    //Mouse Input (Pan and Scan) //MMB (Change if needed)
    void PanAndScan(){
        float xAxis = Input.GetAxis("Horizontal");
        float yAxis = Input.GetAxis("Vertical");
        float rAxis = Input.GetAxis("Rotational");
        float zAxis = Input.GetAxis("Mouse ScrollWheel"); //Zoom In/Out Functionality (Optional)
        Vector3 worldDragDelta = Vector3.zero;
        if (Input.GetMouseButton(2)) {
            DisableColliderOverlayOnMoving(true);
            if (dragDirNegative) {  
                xAxis += Input.GetAxis("Mouse X");
                yAxis += Input.GetAxis("Mouse Y");
            } else {
                if (_worldDragDeltaAux == null) {
                    _worldDragDeltaAux = camFocus.transform.position;
                } else {
                    worldDragDelta = ((Vector3) _worldDragDeltaAux - camFocus.transform.position);
                    worldDragDelta.y = 0;
                }
            }        
        } else if (Input.GetMouseButton(1)) {
            DisableColliderOverlayOnMoving(true);
            rAxis += 2 * Input.GetAxis("Mouse X");
        } else {
            DisableColliderOverlayOnMoving(false);
            _worldDragDeltaAux = null;
        }

        if (!gameObject.GetComponent<CameraLimiter>().GetControlLockStatus()){
            float cameraDistance = Vector3.Distance(transform.position, camFocus.transform.position);

            Vector3 camFw = new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z).normalized;
            Vector3 camSw = new Vector3(cam.transform.forward.z, 0, -cam.transform.forward.x).normalized;

            cam.transform.position += (camFw * yAxis + camSw * xAxis) * (mouseDragspeed * Time.deltaTime);
            cam.transform.position += (worldDragDelta * (1/cameraDistance)); //smoothing factor based on distance


            Vector3 rotPos = cam.transform.position + cam.transform.position.y * Mathf.Tan(Mathf.PI / 2 - Mathf.Deg2Rad * cam.transform.eulerAngles.x) * camFw;
            cam.transform.RotateAround(rotPos, Vector3.up, 100f * rAxis * Time.deltaTime);
        }
    }
}
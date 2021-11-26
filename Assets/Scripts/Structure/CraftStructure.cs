using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CraftStructure : MonoBehaviour {
    private string tentID;
    
    [Header("Structure")]
    public GameObject craftStructure;
    public float craftPrice;
    
    [Header("References")]
    public TextMeshProUGUI priceTag;
    public Material matReference; 

    private Renderer craftStructureMat;
    private GameObject craftUIGO;

    [HideInInspector]
    public bool shouldReset = false;

    public GameObject hoverInfo = null;
    private GameObject cam;
    void Awake(){
        cam = Camera.main.gameObject;
        tentID = Random.value.ToString();
        craftUIGO = gameObject.transform?.GetChild(0).gameObject;
        priceTag.text = craftPrice.ToString();      
    }

    void Start(){
        craftStructureMat = GetComponent<Renderer>();
        craftStructureMat.material = new Material(matReference);
    }

    void Update(){
      if(shouldReset)ColorFade();
    }

    public void Craft(){
        if (Time.timeScale > 0)
        {
            //Check if Money is enough to purchase
            if (RefugeeCampGameManager.Instance.money >= craftPrice)
            {
                RefugeeCampGameManager.Instance.UpdateMoney("-", craftPrice);  //Remove cost from money
                                                                               // GameObject x = Instantiate(craftStructure,new Vector3(transform.position.x, transform.position.y, transform.position.z), craftStructure.transform.rotation, this.transform.parent); //Spawn Structure
                GameObject x = Instantiate(craftStructure, transform.position, transform.rotation, transform.parent); //Spawn Structure

                x.name = craftStructure.name + tentID; //give unique name to Crafted Tents to avoid duplicates
                craftUIGO.SetActive(false); //Disable UI
                x.transform.SetParent(x.transform); //Remove from Parent
                GetComponent<Collider>().enabled = false; //Disable Collider
                GetComponent<MeshRenderer>().enabled = false; //Disable Collider

                //Update Refugees Structure List
                GameObject[] refugees = GameObject.FindGameObjectsWithTag("Refugee");
                GameObject[] updatedStructures = GameObject.FindGameObjectsWithTag("Structure");
                foreach (GameObject refugee in refugees)
                {
                    if (refugee.GetComponent<RefugeeNavigation>().structureTargets.Length != updatedStructures.Length)
                        refugee.GetComponent<RefugeeNavigation>().structureTargets = updatedStructures;
                }
            }
            else
            {
                //Debug.Log("Transaction Failed. Not enough money...");
                //Beep a sound that or nudge something (TODO ADD LATER)
            }
        }
    }

    //Want to Correct it (TODO)
    public void ColorFade(bool flag = false){
        Color tempColor = craftStructureMat.material.color;

        if (flag) craftStructureMat.material.SetColor("_Color", new Color(tempColor.r, tempColor.g, tempColor.b, Mathf.MoveTowards(tempColor.a, 1.0f, 4.0f * Time.deltaTime)));
        
        if (shouldReset){
            craftStructureMat.material.SetColor("_Color", new Color(tempColor.r, tempColor.g, tempColor.b, Mathf.MoveTowards(tempColor.a, 0.0f, 80.0f * Time.deltaTime)));
            if (tempColor.a == 0.0f) shouldReset = false;
        }

        // COOL PULSATING EFFECT - NOT RELEVANT :smile:
        // float step = 0.1f;
        // craftStructureMat.material.SetColor("_Color", new Color(tempColor.r, tempColor.g, tempColor.b, Mathf.PingPong(Time.time, 1.0f)));
    }


    //Price Tag Focus On Camera
    public void FocusCamera(){
        if(hoverInfo.activeInHierarchy) hoverInfo.transform.LookAt(cam.transform.position, Vector3.up);
    }
}

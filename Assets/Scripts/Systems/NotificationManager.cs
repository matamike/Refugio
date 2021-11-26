using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationManager : MonoBehaviour{

    //Singleton GameManager
    private static NotificationManager instance;
    public static NotificationManager Instance { get { return instance; } }

    //Donations Properties
    private GameObject incomingNotificationAreaGO;
    private GameObject notificationListGO;
    private GameObject notificationDialogueGO;
    private TextMeshProUGUI notificationButtonGOText, notificationDialogueGOText;
    private Button notificationDeleteButton, notificationReturnButton;
    public Button notificationAlertButton;
    public GameObject notificationButtonPrefabInteraction; //button we press to view a notification
    public List<GameObject> notificationsButtons = new List<GameObject>();

    public Queue<Notification> incomingNotifications = new Queue<Notification>(); //all unread donations 
    public List<Notification> activeNotifications = new List<Notification>(); //all read donations
    private int unreadNotificationsCount = 0; //number of unread donations


    public GameObject notificationStandaloneGO; //GO for initiating immediatelly instead of pushing the notifications
    public TextMeshProUGUI notificationStandaloneGOText; //GO for adding the message to standalone version of notification


    void Awake(){
        //Notification Manager Tasks
        //DontDestroyOnLoad(gameObject); //preserve Manager LifeTime
        SingletonCheck(); //Singleton   

        //Interactions With Donation Elements Members
        incomingNotificationAreaGO = GameObject.Find("NotificationListEntries");
        notificationListGO = GameObject.Find("NotificationList");
        notificationButtonGOText = GameObject.Find("NotificationAlertPopUp")?.GetComponentInChildren<TextMeshProUGUI>();
        notificationDialogueGO = GameObject.Find("NotificationDialoguePanel");
        notificationDialogueGOText = GameObject.Find("NotificationText")?.GetComponent<TextMeshProUGUI>();
        notificationDeleteButton = GameObject.Find("DeleteNotification")?.GetComponent<Button>();
        notificationReturnButton = GameObject.Find("ReturnNotification")?.GetComponent<Button>();
    }

    void Start()
    {
        activeNotifications.Clear();
        incomingNotifications.Clear();
        DisableGOs();
    }

    void Update(){
        ToggleIncomingNotificationsList(); //update Switch for Notification Pop up

        UpdateNotifications(); //updating incoming notifications when panel becomes active.
    }

    //Create Notification
    public void CreateNotification(string tempNotification, string entity){
        //Create and Assign new Notification to List
        Notification notification = new Notification(tempNotification, entity);
        incomingNotifications.Enqueue(notification);
    }

    public void CreateStandaloneNotification(string tempNotification, string entity){
        //Create and Assign new Notification to List
        Notification notification = new Notification(tempNotification, entity);
        incomingNotifications.Enqueue(notification);

        if (!IsStandaloneTextActive()){
            notificationStandaloneGO.SetActive(true); //activate standalone panel

            //change text
            notificationStandaloneGOText.text = tempNotification ;
        }
    }

    public void UpdateNotifications(){
        if (notificationListGO.activeInHierarchy && incomingNotifications.Count > 0){ //use .activeSelf if we wish to add them before enabling the list. keep .activeInHierarchy if we want to be added when we enable UI
            GameObject x = Instantiate(notificationButtonPrefabInteraction, notificationListGO.transform, false); //Temp Variable Holding all info for Button
            Notification notification = incomingNotifications.Dequeue();

            activeNotifications.Add(notification); //Add incoming Donations to List When Active (from queue).
            notificationsButtons.Add(x); //Create Instance of a Button for the List

            x.GetComponentInChildren<TextMeshProUGUI>().text = activeNotifications[activeNotifications.Count - 1].GetEntity() + " | Notification";// "Notification : " + activeNotifications[activeNotifications.Count - 1].GetEntity().Trim() + " with message: " + activeNotifications[activeNotifications.Count - 1].GetNotification(); //assign button with DonatorID
            x.gameObject.name = activeNotifications[activeNotifications.Count - 1].GetEntity() + " | Notification"; //gameobject name

            notificationButtonGOText.text = "Notifications"; // reset to default once unread notifications have been read.

            //Add Event Listener to Object 
            x?.GetComponent<Button>().onClick.AddListener(() => {
                notificationDialogueGO.SetActive(true);
                notificationDialogueGOText.text = "Notification : " + activeNotifications[activeNotifications.Count - 1].GetEntity().Trim() + " with message: " + activeNotifications[activeNotifications.Count - 1].GetNotification();//x.GetComponentInChildren<TextMeshProUGUI>().text;


                //Remove Any Listeners
                notificationDeleteButton.GetComponent<Button>().onClick.RemoveAllListeners();
                notificationReturnButton.GetComponent<Button>().onClick.RemoveAllListeners();
                notificationDeleteButton.GetComponent<Button>().onClick.AddListener(() => { DeleteNotification(x, notification); notificationDialogueGO.SetActive(false); });
                notificationReturnButton.GetComponent<Button>().onClick.AddListener(() => { notificationDialogueGO.SetActive(false); });
            });

            unreadNotificationsCount = 0; //No Unread donations
        }
        else
        {
            //Notify the number of unread donations
            if (unreadNotificationsCount < incomingNotifications.Count)
            {
                unreadNotificationsCount += 1; //increase count
                notificationButtonGOText.text = "Notifications (" + unreadNotificationsCount.ToString() + ")"; //alert the number of unread donations.
            }
        }
    }

    public void DeleteNotification(GameObject notificationButton, Notification n){
        if (notificationsButtons.Contains(notificationButton)) notificationsButtons.Remove(notificationButton);
        //Destroy GO upon list Removal
        if (GameObject.Find(n.GetEntity() + " | Notification") != null) Destroy(GameObject.Find(n.GetEntity() + " | Notification"));
    }

    //Toggle Listener State
    public void ToggleIncomingNotificationsList()
    {
        notificationAlertButton.onClick.RemoveAllListeners(); //Clear Listeners if any
        if (incomingNotificationAreaGO.activeSelf) notificationAlertButton.onClick.AddListener(() => { incomingNotificationAreaGO.SetActive(false); });
        else notificationAlertButton.onClick.AddListener(() => { incomingNotificationAreaGO.SetActive(true); });
    }

    public bool IsStandaloneTextActive(){
         return notificationStandaloneGO.activeInHierarchy;
    }

    //Singleton
    private void SingletonCheck()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    //Disable Common GOs InitUse
    void DisableGOs()
    {
        incomingNotificationAreaGO?.SetActive(false);
        notificationDialogueGO?.SetActive(false);
        notificationStandaloneGO?.SetActive(false);
    }
}

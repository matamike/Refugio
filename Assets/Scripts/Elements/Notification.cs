using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Notification{
    private string notificationText;
    private string entityBearer;

    public Notification(string notificationText, string entityBearer){
        this.notificationText = notificationText;
        this.entityBearer = entityBearer;
    }

    public string GetNotification(){
        return notificationText;
    }

    public string GetEntity(){
        return entityBearer;
    }
}

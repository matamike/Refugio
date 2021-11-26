using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class DonationEntities : MonoBehaviour{
    List<string> donatorNames = new List<string>();

    public void Awake(){
        ReadDonators();
    }

    private void ReadDonators(){
        TextAsset txtAsset = (TextAsset)Resources.Load("donationEntities");
        string donators = txtAsset.text;
        string[] stringArr = donators.Split('\n');
        foreach (string donator in stringArr)
        {
            donatorNames.Add(donator);
        }
    }

    //GET A RANDOM DONATOR NAME
    public string GetRandomDonator(){
        return donatorNames[Random.Range(0, donatorNames.Count)];
    }
}

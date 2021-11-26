using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Donation{
    private float donationAmount = default;
    private string donator = default;
    private string donationDescription = default;
    private Object agreement = null;
    private System.Type agreementType = null; 

    private float countDownTimer = 1.0f;

    //Constructor for Donations
    public Donation(float donationAmount = default,string donator = default, string donationDescription = default, Object caveAt = default){
        this.donationAmount = donationAmount;
        this.donator = donator;
        this.donationDescription = donationDescription;
        
        //In case Caveat is selected
        if(caveAt != default){
            agreement = caveAt; //assign object (reward or restriction)
            agreementType = caveAt.GetType(); // assign type

            //Assign description according to the type of caveat
            if (agreementType == typeof(Reward)) {
                Reward temp = agreement as Reward;
                donationDescription = temp.GetMessage();
            }
            if (agreementType == typeof(Restriction)){
                Restriction temp = agreement as Restriction;
                donationDescription = temp.GetMessage();
            }
            // /////////////////////////////////////////////////
        }
    }

    //Setters 
    public void SetDonationAmount(float amount){
        donationAmount = amount;
    }

    public void SetDonatorName(string donatorName){
        donator = donatorName;
    }

    public void SetDonationDescription(string description){
        donationDescription = description;
    }

    public void SetAgreement(Object caveAt){
        agreement = caveAt;
    }

    public void SetAgreementType(System.Type caveAtType){
        agreementType = caveAtType;
    }

    //Getters
    public float GetDonationAmount(){
        return donationAmount;
    }

    public string GetDonatorName(){
        return donator;
    }

    public string GetDonationDescription(){
        return donationDescription;
    }

    public Object GetAgreement(){
        return agreement;
    }

    public System.Type GetAgreementType(){
        return agreementType;
    }


    //Actions

    //Accept Donation
    public float AcceptDonation(){
        return GetDonationAmount();
    }

    //Reject Donation
    public float RejectDonation(){
        return 0;
    }

    // Slider/Timer
    public float CountDownDonation(){
        countDownTimer = Mathf.MoveTowards(countDownTimer, 0.0f, 0.1f * Time.deltaTime);
        return countDownTimer;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour{
    //Singleton GameManager
    private static SoundManager instance;
    public static SoundManager Instance { get { return instance; } }
    // ////////////////////
    public AudioClip[] ambience;
    private int currentClipID = 0;
    private AudioSource audioManager;
    
    void Awake(){
        audioManager = GetComponent<AudioSource>();
        DontDestroyOnLoad(gameObject); //preserve Manager LifeTime
        SingletonCheck(); //Singleton
    }

    void Update(){
        PlayAmbience();
    }

    public void PlayAmbience(){
        if (!audioManager.isPlaying && audioManager.clip.name == ambience[currentClipID].name){
            currentClipID += 1; //increase the count.

            if (currentClipID >= ambience.Length)
            {
                currentClipID = 0;
                audioManager.clip = ambience[currentClipID];
                audioManager.Play();
            }
            else
            {
                audioManager.clip = ambience[currentClipID];
                audioManager.Play();
            }
        }
        else return;        
    }

    //Singleton
    private void SingletonCheck()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }
}
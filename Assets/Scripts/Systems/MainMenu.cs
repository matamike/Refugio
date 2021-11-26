using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour{
    public Button start, exit;

    void Awake(){      
        start.onClick.AddListener(StartLevel);
        exit.onClick.AddListener(ExitGame);
    }

    void StartLevel(){
        SceneManager.LoadScene("SampleScene",LoadSceneMode.Single);
    }

    void ExitGame(){
        Application.Quit();
    }
}

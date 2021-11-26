using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIClockController : MonoBehaviour {
    public enum Month {
        January = 0,
        February = 1,
        March = 2,
        April = 3,
        May = 4,
        June = 5,
        July = 6,
        August = 7,
        September = 8,
        October = 9,
        November = 10,
        December = 11
    }

    [Header("Settings")]
    public ushort startYear = 2022;
    public Month startMonth = Month.January;
    [Space(5)]
    public ushort secondsPerMonth = 180;
    [Range(1,3)] 
    public ushort defaultSpeed = 1;

    [Header("References")]
    public TextMeshProUGUI yearText;
    public TextMeshProUGUI monthText;
    [Space(5)]
    public Image radialIndicator;
    public Image radialIndicatorBG;

    [Space(5)]
    public TextMeshProUGUI hoursText;
    public TextMeshProUGUI minutesText;
    public TextMeshProUGUI secondsText;

    private RefugeeCampGameManager.GameTime _gameTime;
    private int _cMonth = 0;
    private ushort _month;
    private ushort _year;

    //Properties for Play/Pause State
    public Toggle toggleTimeStateGO;
    public Sprite playSprite; 
    public Sprite pauseSprite;

    //Properties for Time Modifier
    public GameObject speedModifierGO;
    public int speedModifierValue { get; set; }

    public void Awake() {
        speedModifierValue = 1;
        _year = startYear;
        _month = (ushort) startMonth;

        UpdateText();
        UpdateRadial();
        UpdateTime();
    }

    public void Start(){      
        toggleTimeStateGO.onValueChanged.AddListener(delegate { ToggleGameState(); });
    }


    public void UpdateGameTime(RefugeeCampGameManager.GameTime gameTime) {
        _gameTime = gameTime;
        UpdateRadial();
        UpdateTime();

        int m = (int) gameTime.TotalSeconds / secondsPerMonth;

        if (m != _cMonth) {
            _cMonth = m;
            _month = (ushort) (_cMonth % 12 + startMonth);
            _year = (ushort) (_cMonth / 12 + startYear);
     
            ToggleRadialColor();
            UpdateText();
        }
    }

    private void UpdateText() {
        yearText.text = _year.ToString();
        monthText.text = ((Month) _month).ToString().Substring(0, 3) + ',';
    }

    private void UpdateRadial() {
        radialIndicator.fillAmount = (_gameTime.TotalSeconds % secondsPerMonth) / secondsPerMonth;
    }

    private void UpdateTime() {
        // hoursText.text = _gameTime.Hours.ToString("00");
        // minutesText.text = _gameTime.Minutes.ToString("00");
        // secondsText.text = _gameTime.Seconds.ToString("00");
    }
    
    private void ToggleRadialColor() {
        var tmp = radialIndicator.color;
        radialIndicator.color = radialIndicatorBG.color;
        radialIndicatorBG.color = tmp;
    }


    //Play/Pause States
    public void ToggleGameState(){
        Debug.Log("Changing Play/Pause State");
        List<Button> buttons = new List<Button>(FindObjectsOfType<Button>());
        
        

        //Pause
        if (Time.timeScale > 0)
        {
            toggleTimeStateGO.gameObject.GetComponent<Image>().sprite = pauseSprite;
            toggleTimeStateGO.transform.Find("GameObject").GetComponent<Image>().sprite = pauseSprite;
            Time.timeScale = 0;
            foreach(Button b in buttons){
                if (b.gameObject.activeInHierarchy && b.gameObject != this) b.interactable = false;
            }
            return;
        }

        //Play
        if (Time.timeScale == 0)
        {
            toggleTimeStateGO.gameObject.GetComponent<Image>().sprite = playSprite;
            toggleTimeStateGO.transform.Find("GameObject").GetComponent<Image>().sprite = playSprite;
            Time.timeScale = 1;
            foreach (Button b in buttons){
                if (b.gameObject.activeInHierarchy && b.gameObject != this) b.interactable = true;
            }

            return;
        }
    }

    //Simulation Speed Modifier (normal x1, double x2)
    public void ToggleGameSimulationSpeed(){
        if (speedModifierGO.GetComponent<TextMeshProUGUI>().text.Equals("x1")){
            speedModifierGO.transform.GetComponent<TextMeshProUGUI>().text = "x2";
            speedModifierValue = 2;
            RefugeeCampGameManager.Instance.targetSpeedModifier = speedModifierValue; //Donation Rate 
            FindObjectOfType<SpawnManager>().targetTimeScale = speedModifierValue; //Spawn Rate
            return;
        }
        if (speedModifierGO.GetComponent<TextMeshProUGUI>().text.Equals("x2")){
            speedModifierGO.GetComponent<TextMeshProUGUI>().text = "x1";
            speedModifierValue = 1;
            RefugeeCampGameManager.Instance.targetSpeedModifier = speedModifierValue; //Donation Rate
            FindObjectOfType<SpawnManager>().targetTimeScale = speedModifierValue; //Spawn Rate
            return;
        }
    }

    public int GetYear(){
        int year = _year;
        return year;
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CompletedAgreementCheck : MonoBehaviour{
    public TextMeshProUGUI donator;
    public TextMeshProUGUI caveAt;
    public TextMeshProUGUI score;

    public bool shouldFade = false;
    private float alpha = 1.0f;

    void Update() {
        if ((donator.color == Color.gray || caveAt.color == Color.gray || score.color == Color.gray) && !shouldFade){
            shouldFade = true;
        }

        if (shouldFade){
            alpha = Mathf.Lerp(alpha, 0.0f, 4.0f * Time.deltaTime);
            donator.color = new Color(donator.GetComponent<TextMeshProUGUI>().color.r,
                                                                donator.GetComponent<TextMeshProUGUI>().color.g,
                                                                donator.GetComponent<TextMeshProUGUI>().color.b,
                                                                alpha);
            caveAt.color = new Color(caveAt.GetComponent<TextMeshProUGUI>().color.r,
                                                        caveAt.GetComponent<TextMeshProUGUI>().color.g,
                                                        caveAt.GetComponent<TextMeshProUGUI>().color.b,
                                                        alpha);
            score.color = new Color(score.GetComponent<TextMeshProUGUI>().color.r,
                                                        score.GetComponent<TextMeshProUGUI>().color.g,
                                                        score.GetComponent<TextMeshProUGUI>().color.b,
                                                        alpha);
        }
    }

    void OnDisable(){
        if (shouldFade){
            Debug.Log("Removing Completed Agreements");
            this.gameObject.SetActive(false);
            AgreementsManager.Instance.ClearCompletedAgreement();
        }
    }
}

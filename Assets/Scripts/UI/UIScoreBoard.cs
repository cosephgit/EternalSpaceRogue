using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIScoreBoard : MonoBehaviour
{
    [SerializeField]TextMeshProUGUI scoreOutput;

    void Start()
    {
        UpdateScores();
    }

    public void UpdateScores(int ranking = -1)
    {
        string scoreGenerated = "";

        for (int i = 0; i < GameManager.instance.scores.Length; i++)
        {
            if (ranking == i)
            {
                scoreGenerated += "<color=yellow>";
            }
            scoreGenerated += (i+1) + ": " + GameManager.instance.scores[i];
            if (ranking == i)
            {
                scoreGenerated += "</color>";
            }
            if (i < GameManager.instance.scores.Length - 1)
                scoreGenerated += "\n";
        }

        scoreOutput.text = scoreGenerated;
    }
}

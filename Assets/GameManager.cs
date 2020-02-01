using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    float timeLeft = 180.0f;

    public Text timeText;
    public Text score;
    public ScoreSystem myScore;

    private float possibleEvents = 20f;

    void Update()
    {

        timeLeft -= Time.deltaTime ;

        timeText.text = "Time " + timeLeft / 60f;

        if (timeLeft < 0)
        {
            GameOver();
        }

        TallyScore();

    }

    void TallyScore()
    {   
        score.text = Mathf.Clamp(((myScore.playerScore / possibleEvents) * 100), 1, 100).ToString();
    }

    void GameOver()
    {
        timeText.text = "Time's up";
    }

}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    float timeLeft = 25.0f;

    public Text timeText; //label for time
    public Text score; //label for score
    private float finalScore;

    // canvases
    public Canvas mainCanvas;
    public Canvas endCanvas;

    // winstate text
    private string winText = "you sold the place!";
    private string loseText = "you might be stuck with this place";

    // timer crossouts
    public GameObject[] myCrosses;

    //grab score from score script
    public ScoreSystem myScore;

    private int i;

    private float possibleEvents = 20f;

    private void Start()
    {

        endCanvas.gameObject.SetActive(false);
        mainCanvas.gameObject.SetActive(true);

        foreach(GameObject cross in myCrosses)
        {
            cross.SetActive(false);
        }

        StartCoroutine(Crossout(timeLeft));

    }

    void Update()
    {

        Timer();
        TallyScore();

    }

    void Timer()
    {
        timeLeft -= Time.deltaTime;
        //timeText.text = "Time " + timeLeft / 60f;
        
        if (timeLeft < 0)
        {
            GameOver();
        }
    }

    public void RestartButton()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    void TallyScore()
    {
        finalScore = Mathf.Clamp(((myScore.playerScore / possibleEvents) * 100), 1, 100);
        score.text = finalScore.ToString();
    }

    void GameOver()
    {
        endCanvas.gameObject.SetActive(true);
        mainCanvas.gameObject.SetActive(false);

        if (finalScore < 60f)
        {
            timeText.text = loseText;
        } else
        {
            timeText.text = winText;
        }
            
    }

    IEnumerator Crossout(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime / 10);

            myCrosses[i].SetActive(true);

            if(i < 9)
                i++;
            

        }
    }

}

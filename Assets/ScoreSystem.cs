using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    public float playerScore;

    // Update is called once per frame
    void Update()
    {
        playerPoint();
        ghostPoint();
    }

    void playerPoint()
    {
        if (Input.GetKeyDown("space"))
        {
            playerScore += 1f;
        }
    }

    void ghostPoint()
    {
        if (Input.GetKeyDown("g"))
        {
            print("ddd");
            playerScore -= 1f;
        }
    }
}

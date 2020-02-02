using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public GameObject myStartUI;

    void Start()
    {
        myStartUI.SetActive(false);
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        while (true)
        {
            yield return new WaitForSeconds(16f);

            myStartUI.SetActive(true);

            if(Input.GetKeyDown("joystick button 0"))
                SceneManager.LoadScene("main");

        }
    }
}

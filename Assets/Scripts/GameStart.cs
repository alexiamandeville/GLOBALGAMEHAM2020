using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

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

            while (true)
            {
                if (Gamepad.current.buttonSouth.wasReleasedThisFrame)
                    SceneManager.LoadScene("main");

                yield return new WaitForEndOfFrame();
            }
        }
    }
}

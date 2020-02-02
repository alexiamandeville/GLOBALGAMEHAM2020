using System.Collections;
using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    public class TMPTextWriter : MonoBehaviour
    {

        TextMeshProUGUI txt;
        string story;

        void Awake()
        {
            txt = GetComponent<TextMeshProUGUI>();
            story = txt.text;
            txt.text = "";

            // TODO: add optional delay when to start
            StartCoroutine("PlayText");
        }

        IEnumerator PlayText()
        {
            foreach (char c in story)
            {
                txt.text += c;
                yield return new WaitForSeconds(0.05f);
            }
        }
    }
}
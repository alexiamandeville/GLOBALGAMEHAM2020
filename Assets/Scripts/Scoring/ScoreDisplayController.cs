using TMPro;
using UnityEngine;

namespace DefaultNamespace.Scoring
{
  public class ScoreDisplayController : MonoBehaviour
  {
    public TextMeshProUGUI flipperPctText;
    public TextMeshProUGUI ghostPctText;

    public void SetScore(float flipperScore, float ghostScore)
    {
      flipperPctText.text = $"{flipperScore:N0}";
      ghostPctText.text = $"{ghostScore:N0}";
    }
  }
}
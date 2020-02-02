using TMPro;
using UnityEngine;

namespace DefaultNamespace.Scoring
{
  public class ScoreDisplayController : MonoBehaviour
  {
    public TextMeshProUGUI flipperPctText;
    public TextMeshProUGUI ghostPctText;

    public void SetScore(float flipperPct, float ghostPct)
    {
      flipperPctText.text = $"{flipperPct:N0}%";
      ghostPctText.text = $"{ghostPct:N0}%";
    }
  }
}
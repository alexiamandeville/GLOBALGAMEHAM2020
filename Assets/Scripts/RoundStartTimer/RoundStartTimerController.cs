using TMPro;
using UnityEngine;

namespace DefaultNamespace.RoundStartTimer
{
  public class RoundStartTimerController : MonoBehaviour
  {
    public TextMeshProUGUI timerText;
    
    protected float TimeLeft = 0;
    
    public void SetTimeLeft(float val)
    {
      timerText.text = $"{val:N0}";
    }
  }
}
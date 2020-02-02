using UnityEngine;

namespace DefaultNamespace.RoundTimer
{
  public class RoundTimerController : MonoBehaviour
  {
    public GameObject[] dateCrosses;

    void Start()
    {
      Reset();
    }

    public void Reset()
    {
      foreach(GameObject cross in dateCrosses)
      {
        cross.SetActive(false);
      }
    }

    public void SetTimeLeft(float curTime, float maxTime)
    {
      var pct = (maxTime - curTime) / maxTime;

      for (var idx = 0; idx < dateCrosses.Length; idx++)
      {
        var o = dateCrosses[idx];

        var idxPct = ((float) idx / (float) dateCrosses.Length);
        o.SetActive(idxPct <= pct);
      }
    }
  }
}
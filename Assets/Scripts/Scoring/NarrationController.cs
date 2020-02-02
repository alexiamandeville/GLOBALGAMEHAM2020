using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace.Scoring
{
  public class NarrationController : MonoBehaviour
  {
    [Serializable]
    public struct ScoreMessages
    {
      public float MinPct;
      public float MaxPct;

      public string DisplayString;
    }

    public ScoreMessages[] scoreMessages;

    public TextMeshProUGUI textThingy;

    public string waitForGhostsText;

    public string defaultText;

    public void Reset()
    {
      if (textThingy == null)
      {
        return;
      }

      textThingy.text = defaultText;
    }

    public void ShowGhostText()
    {
      if (textThingy == null)
      {
        return;
      }

      textThingy.text = waitForGhostsText;
    }

    public void SetScorePct(float pct)
    {
      if (textThingy == null)
      {
        return;
      }
      
      foreach (var s in scoreMessages)
      {
        if (pct >= s.MinPct && pct <= s.MaxPct)
        {
          textThingy.text = s.DisplayString;
          break;
        }
      }
    }
  }
}
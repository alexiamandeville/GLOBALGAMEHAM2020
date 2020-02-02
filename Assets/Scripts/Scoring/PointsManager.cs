using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace.Scoring
{
  public class PointsManager
  {
    public Dictionary<PlayerType, int> PlayerTypeToPoints = new Dictionary<PlayerType, int>();
    
    public void ResetPoints()
    {
      PlayerTypeToPoints.Clear();
    }
    
    public void AddPoint(PlayerType type)
    {
      if (!PlayerTypeToPoints.ContainsKey(type))
      {
        PlayerTypeToPoints.Add(type, 0);
      }

      PlayerTypeToPoints[type]++;
      
      Debug.Log($"SCORE[{type}] => {PlayerTypeToPoints[type]}");
    }
    
    public float GetTotalScore(PlayerType type)
    {
      return PlayerTypeToPoints.ContainsKey(type) ? PlayerTypeToPoints[type] : 0;
    }

    public float GetScorePct(PlayerType type)
    {
      if (!PlayerTypeToPoints.ContainsKey(type))
      {
        return 0;
      }
      
      int totalScore = 0;
      foreach (var pt in PlayerTypeToPoints)
      {
        totalScore += pt.Value;
      }

      return (float) PlayerTypeToPoints[type] / (float) totalScore * 100;
    }

    public PlayerType GetWinner()
    {
      PlayerType t = PlayerType.INVALID;
      int maxVal = 0;
      foreach (var kv in PlayerTypeToPoints)
      {
        if (kv.Value > maxVal)
        {
          maxVal = kv.Value;
          t = kv.Key;
        }
      }

      return t;
    }
  }
}
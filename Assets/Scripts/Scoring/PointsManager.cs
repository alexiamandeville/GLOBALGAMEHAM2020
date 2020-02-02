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
    }
  }
}
using System;
using System.Collections.Generic;
using DefaultNamespace;
using TMPro;
using UnityEngine;

namespace Title
{
  public class TitleTextController : MonoBehaviour
  {
    [SerializeField] protected int playerNumber;
    [SerializeField] protected PlayerType playerType;
    [SerializeField] protected TextMeshProUGUI text;
    
    [Serializable]
    public struct DisplayStrings {
      public PlayerType playerType;
      public string displayString;
    }

    [SerializeField] protected DisplayStrings[] PlayerTypeDisplayStrings;

    protected PlayerType lastPlayerType = PlayerType.INVALID;

    public void SetPlayerType(PlayerType type)
    {
      playerType = type;
    }

    public void SetPlayerNumber(int num)
    {
      playerNumber = num;
    }

    void Update()
    {
      if (text == null)
      {
        return;
      }

      if (PlayerTypeDisplayStrings.Length == 0)
      {
        return;
      }

      if (lastPlayerType == playerType)
      {
        return;
      }

      foreach (var ds in PlayerTypeDisplayStrings)
      {
        if (ds.playerType == playerType)
        {
          SetPlayerString(ds.displayString);
        }
      }

      lastPlayerType = playerType;
    }

    void SetPlayerString(string display)
    {
      text.text = $"{display} player {playerNumber+1}";
    }
  }
}
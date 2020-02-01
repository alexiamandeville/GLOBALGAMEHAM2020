using System;
using Models;
using Title;
using UnityEngine;

namespace DefaultNamespace
{
  public class PlayerController : MonoBehaviour
  {
    public PlayerType playerType = PlayerType.INVALID;
    public int playerNumber = 0;
    
    [SerializeField] protected PlayerInput inputController;
    [SerializeField] protected TitleTextController titleTextController;
    [SerializeField] protected BaseModelController modelController;

    [SerializeField] protected GameObject flipperModel;
    [SerializeField] protected GameObject ghostModel;

    void Awake()
    {
      if (titleTextController == null)
      {
        return;
      }
      
      titleTextController.SetPlayerNumber(playerNumber);
      titleTextController.SetPlayerType(playerType);
      
      inputController.SetPlayerNumber(playerNumber);

      switch (playerType)
      {
        case PlayerType.GHOST:
          flipperModel.SetActive(false);
          ghostModel.SetActive(true);
          break;
        
        case PlayerType.FLIPPER:
          flipperModel.SetActive(true);
          ghostModel.SetActive(false);
          break;
        
        default:
          Debug.LogError($"PLAYER TYPE NOT SET, #{playerNumber}");
          break;
      }
    }
  }
}
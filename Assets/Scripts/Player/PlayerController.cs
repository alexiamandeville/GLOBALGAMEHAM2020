  using System;
using Models;
using Title;
using UnityEngine;

namespace DefaultNamespace
{
  public class PlayerController : MonoBehaviour
  {
    public FlipperType flipperType { get; protected set; } = FlipperType.INVALID;
    public PlayerType playerType { get; protected set; } = PlayerType.INVALID;
    protected int playerNumber = 0;
    protected int controllerId = 0;

    [SerializeField] protected PlayerInput inputController;
    [SerializeField] protected TitleTextController titleTextController;
    [SerializeField] protected BaseModelController modelController;

    [SerializeField] protected GameObject flipperModel;
    [SerializeField] protected GameObject ghostModel;
    
    [SerializeField] protected GameObject headLamp;

    void Start()
    {
      SetHeadlampActive(false);
    }

    public void InitPlayer(PlayerType type, FlipperType flipper, int controllerId, int playerId)
    {
      flipperType = flipper;
      playerType = type;
      playerNumber = playerId;
      this.controllerId = controllerId;

      titleTextController.SetPlayerNumber(playerNumber);
      titleTextController.SetPlayerType(playerType, flipperType);

      inputController.SetPlayerNumber(this.controllerId);

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

    public void SetHeadlampActive(bool isActive)
    {
      headLamp.SetActive(isActive);
    }
  }
}

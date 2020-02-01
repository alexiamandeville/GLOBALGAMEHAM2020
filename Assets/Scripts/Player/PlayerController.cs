  using System;
using Models;
using Title;
using UnityEngine;

namespace DefaultNamespace
{
  public class PlayerController : MonoBehaviour
  {
    public FlipperType flipperType = FlipperType.INVALID;
    public PlayerType playerType { get; protected set; } = PlayerType.INVALID;
    protected int playerNumber = 0;
    protected int controllerId = 0;

    [SerializeField] protected PlayerInput inputController;
    [SerializeField] protected TitleTextController titleTextController;
    [SerializeField] protected BaseModelController modelController;

    [SerializeField] protected GameObject flipperModel;
    [SerializeField] protected GameObject ghostModel;

    public void InitPlayer(PlayerType type, int controllerId, int playerId)
    {
      playerType = type;
      playerNumber = playerId;
      this.controllerId = controllerId;

      titleTextController.SetPlayerNumber(playerNumber);
      titleTextController.SetPlayerType(playerType);

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
  }
}

  using System;
using Models;
using Title;
  using UnityEditor;
  using UnityEngine;

namespace DefaultNamespace
{
  public class PlayerController : MonoBehaviour
  {
    public FlipperType flipperType { get; protected set; } = FlipperType.INVALID;
    public PlayerType playerType { get; protected set; } = PlayerType.INVALID;
    public LeGame.InteractionState curInteractionState { get; protected set; } = LeGame.InteractionState.INVALID;
    protected int playerNumber = 0;
    protected int controllerId = 0;

    [SerializeField] protected PlayerInput inputController;
    [SerializeField] protected TitleTextController titleTextController;
    [SerializeField] protected BaseModelController modelController;

    [SerializeField] protected Collider[] PlayerColliders;

    [SerializeField] protected GameObject flipperModel;
    [SerializeField] protected GameObject ghostModel;
    
    [SerializeField] protected GameObject headLamp;

    void Start()
    {
      SetHeadlampActive(false);
    }

    public void SetInteractionState(LeGame.InteractionState mode)
    {
      curInteractionState = mode;
      
      switch (mode)
      {
        case LeGame.InteractionState.NO_MOVEMENT:
          inputController.gameObject.SetActive(false);
          inputController.CanTeleport(false);
          break;
        
        case LeGame.InteractionState.NO_TELEPORT:
          inputController.gameObject.SetActive(true);
          inputController.CanTeleport(false);
          break;
        
        case LeGame.InteractionState.ALL_MOVE_ONLY_GHOSTS_INTERACT:
          inputController.gameObject.SetActive(true);
          if (playerType == PlayerType.GHOST)
          {
            inputController.CanTeleport(true);
          }
          else
          {
            inputController.CanTeleport(false);
          }
          break;
        
        case LeGame.InteractionState.ALL_INTERACTIONS:
          inputController.gameObject.SetActive(true);
          inputController.CanTeleport(true);
          break;
      }
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

          titleTextController.gameObject.layer = Layers.GhostNumber;
          
          SetColliderLayer(Layers.GhostNumber);
          break;

        case PlayerType.FLIPPER:
          flipperModel.SetActive(true);
          ghostModel.SetActive(false);
          
          titleTextController.gameObject.layer = Layers.FlipperNumber;
          SetColliderLayer(Layers.FlipperNumber);
          break;

        default:
          Debug.LogError($"PLAYER TYPE NOT SET, #{playerNumber}");
          break;
      }
    }

    void SetColliderLayer(int layer)
    {
      foreach (var playerCollider in PlayerColliders)
      {
        playerCollider.gameObject.layer = layer;
      }
    }

    public void SetHeadlampActive(bool isActive)
    {
      headLamp.SetActive(isActive);
    }
  }
}

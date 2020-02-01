using System.Collections;
using System.Collections.Generic;
using Facebook.SocialVR.Worlds.Shapeworld.Scripts.Utils.FSM;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR.Haptics;
using UnityScript.Macros;

namespace DefaultNamespace
{
  public partial class LeGame : MonoBehaviour
  {
    public const int MAX_PLAYERS = 4;
    
    public Dictionary<int, int> ControllerToPlayerMap = new Dictionary<int, int>();
    public Dictionary<int, GameObject> PlayerIdToSpawnedEntMap = new Dictionary<int, GameObject>();

    [Header("Reference to the prefab for a player to spawn")]
    [SerializeField] protected GameObject PlayerPrefabRef;

    [Header("Where to spawn all players")]
    [SerializeField] protected Transform PlayerRoot;
    
    public enum GameState
    {
      INVALID = 0,
      
      START,
      
      WAITING_FOR_PLAYERS,
      HAVE_ALL_PLAYERS,
      
      ROUND_START_COUNTDOWN,
      ROUND,
      ROUND_END,
    }
    protected SimpleFSM<GameState> fsm;

    public void Awake()
    {
      fsm = new SimpleFSM<GameState>(GameState.START).StartOn(this, this);
      fsm.onChanged += (prevState, currentState, stage) => {
        Debug.Log($"--GAME STATE-- {prevState} -> {currentState}:{stage}");
      };
    }
    
    // START
    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------
    void on__GameState__START__DID_ENTER(GameState prevState)
    {
      fsm.requestStateTransitionTo(GameState.WAITING_FOR_PLAYERS);
    }

    // WAITING_FOR_PLAYERS
    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------
    void SpawnPlayer(int gamepadId, int playerId)
    {
      var inst = Instantiate(PlayerPrefabRef, PlayerRoot, false);
      inst.transform.localPosition = Vector3.zero;
      inst.transform.localRotation = Quaternion.identity;

      var pc = inst.GetComponent<PlayerController>();
      PlayerType ptype = PlayerType.INVALID;
      if (playerId == 0 || playerId == 1)
      {
        // flippers
        ptype = PlayerType.FLIPPER;
      }
      else
      {
        // ghosts
        ptype = PlayerType.GHOST;
      }

      pc.InitPlayer(ptype, gamepadId, playerId);

      pc.gameObject.name = $"{ptype} ctrl:{gamepadId} pid:{playerId}";
    }
    
    IEnumerator on__GameState__WAITING_FOR_PLAYERS__ENTERING(GameState prevState)
    {
      while (true)
      {
        yield return null;
        
        // wait for all players
        if (ControllerToPlayerMap.Count >= MAX_PLAYERS)
        {
          break;
        }
      
        var controller = Gamepad.current;
        var controllers = Gamepad.all;
        if (controller.buttonSouth.wasReleasedThisFrame)
        {
          for (var idx = 0; idx < controllers.Count; idx++)
          {
            var c = controllers[idx];
            if (c == controller)
            {
              if (ControllerToPlayerMap.ContainsKey(idx))
              {
                // controller already registered
                break;
              }

              var playerId = ControllerToPlayerMap.Count;
              Debug.Log($"Registering controller {idx} for player {playerId}");
              ControllerToPlayerMap.Add(idx, playerId);

              SpawnPlayer(idx, playerId);
              
              break;
            }
          }
        }
      }
      
      yield break;
    }
  }
}
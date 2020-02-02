using System.Collections;
using System.Collections.Generic;
using DefaultNamespace.RoundStartTimer;
using Facebook.SocialVR.Worlds.Shapeworld.Scripts.Utils.FSM;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace DefaultNamespace
{
  public partial class LeGame : MonoBehaviour
  {
    public const int MAX_PLAYERS = 4;
    
    public Dictionary<int, int> ControllerToPlayerMap = new Dictionary<int, int>();
    public Dictionary<int, GameObject> PlayerIdToSpawnedEntMap = new Dictionary<int, GameObject>();

    public RoundStartTimerController roundTimerController;

    [Header("Reference to the prefab for a player to spawn")]
    [SerializeField] protected GameObject PlayerPrefabRef;

    [Header("Where to spawn all players")]
    [SerializeField] protected Transform[] PlayerSpawnRoot;
    
    public enum GameState
    {
      INVALID = 0,
      
      START,
      
      WAITING_FOR_PLAYERS,

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

//      if(SceneManager.GetSceneByName("ScroungeLayer") == null)
        SceneManager.LoadScene("ScroungeLayer", LoadSceneMode.Additive);
    }
    
    // START
    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------
    void HardReset()
    {
      roundTimerController.gameObject.SetActive(false);
    }
    
    void on__GameState__START__WILL_ENTER(GameState prevState)
    {
      HardReset();
    }
    
    void on__GameState__START__DID_ENTER(GameState prevState)
    {
      fsm.requestStateTransitionTo(GameState.WAITING_FOR_PLAYERS);
    }

    // WAITING_FOR_PLAYERS
    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------
    void SpawnPlayer(int gamepadId, int playerId)
    {
      var root = PlayerSpawnRoot[playerId];
      
      var inst = Instantiate(PlayerPrefabRef, root, false);
      inst.transform.localPosition = Vector3.zero;
      inst.transform.localRotation = Quaternion.identity;

      var pc = inst.GetComponent<PlayerController>();
      PlayerType ptype = PlayerType.INVALID;
      FlipperType ftype = FlipperType.INVALID;
      if (playerId == 0 || playerId == 1)
      {
        // flippers
        ptype = PlayerType.FLIPPER;
        ftype = playerId == 0 ? FlipperType.FIXER : FlipperType.CLEANER;
      }
      else
      {
        // ghosts
        ptype = PlayerType.GHOST;
      }

      pc.InitPlayer(ptype, ftype, gamepadId, playerId);

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

      if (ControllerToPlayerMap.Count == MAX_PLAYERS)
      {
        fsm.requestStateTransitionTo(GameState.ROUND_START_COUNTDOWN);

        yield break;
      }
      
      Debug.LogError("WUT");
    }
    
    // ROUND_START_COUNTDOWN
    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------

    const float TIMER_MAX = 10f;
    void on__GameState__ROUND_START_COUNTDOWN__WILL_ENTER(GameState prevState)
    {
      roundTimerController.SetTimeLeft(TIMER_MAX);
      roundTimerController.gameObject.SetActive(true);
    }
    
    IEnumerator on__GameState__ROUND_START_COUNTDOWN__ENTERING(GameState prevState)
    {
      float timeLeft = TIMER_MAX;
      while (timeLeft >= 0)
      {
        timeLeft -= Time.deltaTime;

        roundTimerController.SetTimeLeft(timeLeft);
        
        yield return null;
      }
    }
    
    void on__GameState__ROUND_START_COUNTDOWN__DID_ENTER(GameState prevState)
    {
      roundTimerController.gameObject.SetActive(false);
      roundTimerController.SetTimeLeft(TIMER_MAX);

      fsm.requestStateTransitionTo(GameState.ROUND);
    }
    
    // ROUND
    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------
  }
}
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace.RoundStartTimer;
using DefaultNamespace.RoundTimer;
using DefaultNamespace.Scoring;
using Facebook.SocialVR.Worlds.Shapeworld.Scripts.Utils.FSM;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DefaultNamespace
{
  public partial class LeGame : MonoBehaviour
  {
    public const int MAX_PLAYERS = 1; //4;
    
    public Dictionary<int, int> ControllerToPlayerMap = new Dictionary<int, int>();
    public Dictionary<int, GameObject> PlayerIdToSpawnedEntMap = new Dictionary<int, GameObject>();

    public RoundStartTimerController roundStartTimerController;
    public RoundTimerController roundTimerController;

    protected PointsManager pointManager;

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
    }
    
    // START
    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------
    void HardReset()
    {
      roundStartTimerController.gameObject.SetActive(false);
      roundTimerController.gameObject.SetActive(false);
      roundTimerController.Reset();
      
      pointManager.ResetPoints();
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

    const float ROUND_START_TIMER_MAX = 3f;
    void on__GameState__ROUND_START_COUNTDOWN__WILL_ENTER(GameState prevState)
    {
      roundStartTimerController.SetTimeLeft(ROUND_START_TIMER_MAX);
      roundStartTimerController.gameObject.SetActive(true);
    }
    
    IEnumerator on__GameState__ROUND_START_COUNTDOWN__ENTERING(GameState prevState)
    {
      float timeLeft = ROUND_START_TIMER_MAX;
      while (timeLeft >= 0)
      {
        timeLeft -= Time.deltaTime;

        roundStartTimerController.SetTimeLeft(timeLeft);
        
        yield return null;
      }
    }
    
    void on__GameState__ROUND_START_COUNTDOWN__DID_ENTER(GameState prevState)
    {
      roundStartTimerController.gameObject.SetActive(false);
      roundStartTimerController.SetTimeLeft(ROUND_START_TIMER_MAX);

      fsm.requestStateTransitionTo(GameState.ROUND);
    }
    
    // ROUND
    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------
    private const float ROUND_TIMER_MAX = 20f;

    private Interactable[] sceneInteractables;
    
    void on__GameState__ROUND__WILL_ENTER(GameState prevState)
    {
      roundTimerController.gameObject.SetActive(true);
      roundTimerController.Reset();

      sceneInteractables = FindObjectsOfType<Interactable>();
      foreach (var i in sceneInteractables)
      {
        i.OnBroken.AddListener(() =>
        {
          Debug.Log($"Item broken: {i.gameObject.name}");
          pointManager.AddPoint(PlayerType.GHOST);
        });
        
        i.OnFixed.AddListener(() =>
        {
          Debug.Log($"Item fixed: {i.gameObject.name}");
          pointManager.AddPoint(PlayerType.FLIPPER);
        });
      }
    }
    
    IEnumerator on__GameState__ROUND__ENTERING(GameState prevState)
    {
      float timeLeft = ROUND_TIMER_MAX;
      while (timeLeft >= 0)
      {
        timeLeft -= Time.deltaTime;

        roundTimerController.SetTimeLeft(timeLeft, ROUND_TIMER_MAX);
        
        yield return null;
      }
    }
    
    void on__GameState__ROUND__DID_ENTER(GameState prevState)
    {
      // roundStartTimerController.gameObject.SetActive(false);
      // roundStartTimerController.SetTimeLeft(ROUND_START_TIMER_MAX);
      //
      // fsm.requestStateTransitionTo(GameState.ROUND);
    }
  }
}
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace.RoundStartTimer;
using DefaultNamespace.RoundTimer;
using DefaultNamespace.Scoring;
using Facebook.SocialVR.Worlds.Shapeworld.Scripts.Utils.FSM;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace DefaultNamespace
{
  public partial class LeGame : MonoBehaviour
  {
    public const int MAX_PLAYERS = 4; //4;
    
    public Dictionary<int, int> ControllerToPlayerMap = new Dictionary<int, int>();
    public Dictionary<int, PlayerController> PlayerIdToSpawnedEntMap = new Dictionary<int, PlayerController>();

    public RoundStartTimerController roundStartTimerController;
    public RoundTimerController roundTimerController;
    public GameObject roundCanvas;
    public ScoreDisplayController scoreDisplayController;
    public NarrationController narrationController;
    public TextMeshProUGUI winTextRef;

    protected PointsManager pointManager = new PointsManager();

    [Header("Reference to the prefab for a player to spawn")]
    [SerializeField] protected GameObject PlayerPrefabRef;

    [Header("Where to spawn all players")]
    [SerializeField] protected Transform[] PlayerSpawnRoot;
    
    [SerializeField] protected GameObject EndGameCanvas;
    [SerializeField] protected GameObject PressToJoinCanvas;
    
    const float ROUND_START_TIMER_MAX = 20f;
    private const float ROUND_TIMER_MAX = 90f;
    private Interactable[] sceneInteractables;
    
    private string winText = "you sold the place!";
    private string loseText = "you might be stuck with this place";

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
      fsm.onChanged += (prevState, currentState, stage) =>
      {
        Debug.Log($"#### GAME STATE #### {prevState} -> {currentState}:{stage}");
      };

      interactFsm = new SimpleFSM<InteractionState>(InteractionState.NO_MOVEMENT).StartOn(this, this);
      interactFsm.onChanged += (prevState, currentState, stage) =>
      {
        Debug.Log($"-- INTERACTION STATE -- {prevState} -> {currentState}:{stage}");

        if (stage == StateStage.DID_ENTER)
        {
          foreach (var kv in PlayerIdToSpawnedEntMap)
          {
            kv.Value.SetInteractionState(currentState);
          }
        }
      };

//      if(SceneManager.GetSceneByName("ScroungeLayer") == null)
      SceneManager.LoadScene("ScroungeLayer", LoadSceneMode.Additive);
    }

    // START
    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------
    void HardReset()
    {
      Debug.LogWarning("---HARD RESET---");
      foreach (var t in PlayerIdToSpawnedEntMap)
      {
        Debug.Log($"Destroy {t.Value.gameObject.name}");
        Destroy(t.Value.gameObject);
      }

      ControllerToPlayerMap.Clear();
      PlayerIdToSpawnedEntMap.Clear();

      roundStartTimerController.gameObject.SetActive(false);
      
      roundTimerController.gameObject.SetActive(false);
      roundTimerController.Reset();
      
      pointManager.ResetPoints();

      EndGameCanvas.SetActive(false);
      roundCanvas.SetActive(false);
      PressToJoinCanvas.SetActive(false);
      
      interactFsm.requestStateTransitionTo(InteractionState.NO_MOVEMENT);
      narrationController.Reset();

      winTextRef.text = "";

      foreach (var interactable in sceneInteractables)
      {
        interactable.Reset();
      }
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
      pc.SetInteractionState(interactFsm.currentState);

      pc.gameObject.name = $"{ptype}{(ptype == PlayerType.FLIPPER ? $":{ftype}" : "")} ctrl:{gamepadId} pid:{playerId}";

      PlayerIdToSpawnedEntMap.Add(playerId, pc);
    }
    
    void on__GameState__WAITING_FOR_PLAYERS__WILL_ENTER(GameState prevState)
    {
      PressToJoinCanvas.SetActive(true);

      interactFsm.requestStateTransitionTo(InteractionState.NO_TELEPORT);
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
    
    void on__GameState__WAITING_FOR_PLAYERS__WILL_EXIT(GameState prevState)
    {
      PressToJoinCanvas.SetActive(false);
    }
    
    // ROUND_START_COUNTDOWN
    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------
    
    void on__GameState__ROUND_START_COUNTDOWN__WILL_ENTER(GameState prevState)
    {
      interactFsm.requestStateTransitionTo(InteractionState.ALL_MOVE_ONLY_GHOSTS_INTERACT);
      
      roundStartTimerController.SetTimeLeft(ROUND_START_TIMER_MAX);
      roundStartTimerController.gameObject.SetActive(true);
      
      narrationController.ShowGhostText();
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

    void on__GameState__ROUND__WILL_ENTER(GameState prevState)
    {
      narrationController.Reset();
      
      interactFsm.requestStateTransitionTo(InteractionState.ALL_INTERACTIONS);
      
      roundCanvas.SetActive(true);
      roundTimerController.gameObject.SetActive(true);
      roundTimerController.Reset();

      sceneInteractables = FindObjectsOfType<Interactable>();
      foreach (var i in sceneInteractables)
      {
        i.OnBroken.AddListener(() =>
        {
          pointManager.AddPoint(PlayerType.GHOST);

          var pct = pointManager.GetScorePct(PlayerType.FLIPPER);
          narrationController.SetScorePct(pct);
          
          Debug.Log($"Item broken: {i.gameObject.name}, new pct: {pct}");
        });
        
        i.OnFixed.AddListener(() =>
        {
          pointManager.AddPoint(PlayerType.FLIPPER);
          
          var pct = pointManager.GetScorePct(PlayerType.FLIPPER);
          narrationController.SetScorePct(pct);
          
          Debug.Log($"Item fixed: {i.gameObject.name}, new pct: {pct}");
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
      roundTimerController.gameObject.SetActive(false);
      roundCanvas.SetActive(false);
      
      fsm.requestStateTransitionTo(GameState.ROUND_END);
    }
    
    // ROUND_END
    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------

    void on__GameState__ROUND_END__WILL_ENTER(GameState prevState)
    {
      interactFsm.requestStateTransitionTo(InteractionState.NO_MOVEMENT);
      
      EndGameCanvas.SetActive(true);

      var flipScore = pointManager.GetTotalScore(PlayerType.FLIPPER);
      var ghostScore = pointManager.GetTotalScore(PlayerType.GHOST);

      if (flipScore > ghostScore)
      {
        winTextRef.text = winText;
      }
      else
      {
        winTextRef.text = loseText;
      }
      
      scoreDisplayController.SetScore(flipScore, ghostScore);
    }

    IEnumerator on__GameState__ROUND_END__ENTERING(GameState prevState)
    {
      while (true)
      {
        if (Gamepad.current.buttonSouth.wasReleasedThisFrame)
        {
          fsm.requestStateTransitionTo(GameState.START);
          break;
        }
        
        yield return null;
      }
    }
    
    void on__GameState__ROUND_END__DID_ENTER(GameState prevState)
    {
      EndGameCanvas.SetActive(false);
      scoreDisplayController.SetScore(0, 0);
    }
  }
}
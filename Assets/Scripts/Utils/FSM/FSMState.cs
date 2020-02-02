using System;
using System.Collections;
using System.Reflection;

namespace Facebook.SocialVR.Worlds.Shapeworld.Scripts.Utils.FSM {
  /// <summary>
  /// These are all the possible stages an individual state can be in.
  ///
  /// A state will transition through these stages as it's requested, and
  /// also as another one is requested in its stead.
  ///
  /// The flow for the stages are as follows:
  ///
  ///  -> Request State ABC
  ///     -> ABC: WILL_ENTER
  ///     -> ABC: ENTERING (coroutine, can run for arbitrary length)
  ///     -> ABC: DID_ENTER
  ///
  /// ABC will then remain in DID_ENTER until another state takes it place.
  ///
  ///  -> Request State XYZ after ABC is in DID_ENTER
  ///     -> ABC: WILL_EXIT
  ///     -> ABC: EXITING (coroutine, can run for arbitrary length)
  ///     -> ABC: DID_EXIT
  ///     -> ABC: FINALLY_EXITED
  ///     -> XYZ: WILL_ENTER
  ///     -> XYZ: ENTERING
  ///     -> XYZ: DID_ENTER
  /// </summary>
  public enum StateStage {
    INVALID = 0,

    WILL_ENTER,
    ENTERING,
    DID_ENTER,

    WILL_EXIT,
    EXITING,
    DID_EXIT,

    // Always called, even if transition did not occur.
    //
    // Useful for cleaning up, if an enter is cancelled half way through
    // or ditto on an interrupted exit, etc...
    FINALLY_EXITED,

    FIXED_UPDATE,
    UPDATE,
    LATE_UPDATE,
  }

  public class FSMState<TState> {
    public delegate void WillEnter(TState prevState);

    public delegate IEnumerator Entering(TState prevState);

    public delegate void DidEnter(TState prevState);

    public delegate void WillExit(TState nextState);

    public delegate IEnumerator Exiting(TState nextState);

    public delegate void DidExit(TState nextState);

    public delegate void FinallyExited(TState nextState);

    public delegate IEnumerator FixedUpdate(TState currentState, object waitObject);

    public delegate IEnumerator Update(TState currentState, object waitObject);

    public delegate IEnumerator LateUpdate(TState currentState, object waitObject);

    public WillEnter OnWillEnter = null;
    public Entering OnEntering = null;
    public DidEnter OnDidEnter = null;

    public WillExit OnWillExit = null;
    public Exiting OnExiting = null;
    public DidExit OnDidExit = null;
    public FinallyExited OnFinallyExited = null;

    public FixedUpdate OnFixedUpdate = null;
    public Update OnUpdate = null;
    public LateUpdate OnLateUpdate = null;

    public bool assignStageDelegate(StateStage stage, MethodInfo method, object owner) {
      switch (stage) {
        case StateStage.WILL_ENTER:
          OnWillEnter = createDelegateOfType<WillEnter>(method, owner);
          return true;
        case StateStage.ENTERING:
          OnEntering = createDelegateOfType<Entering>(method, owner);
          return true;
        case StateStage.DID_ENTER:
          OnDidEnter = createDelegateOfType<DidEnter>(method, owner);
          return true;

        case StateStage.WILL_EXIT:
          OnWillExit = createDelegateOfType<WillExit>(method, owner);
          return true;
        case StateStage.EXITING:
          OnExiting = createDelegateOfType<Exiting>(method, owner);
          return true;
        case StateStage.DID_EXIT:
          OnDidExit = createDelegateOfType<DidExit>(method, owner);
          return true;
        case StateStage.FINALLY_EXITED:
          OnFinallyExited = createDelegateOfType<FinallyExited>(method, owner);
          return true;

        case StateStage.FIXED_UPDATE:
          OnFixedUpdate = createDelegateOfType<FixedUpdate>(method, owner);
          return true;
        case StateStage.UPDATE:
          OnUpdate = createDelegateOfType<Update>(method, owner);
          return true;
        case StateStage.LATE_UPDATE:
          OnLateUpdate = createDelegateOfType<LateUpdate>(method, owner);
          return true;

        // unsupported, this should never happen
        default:
          return false;
      }
    }

    static TDelegate createDelegateOfType<TDelegate>(MethodInfo method, object obj) where TDelegate : class {
      return Delegate.CreateDelegate(typeof(TDelegate), obj, method) as TDelegate;
    }
  }
}

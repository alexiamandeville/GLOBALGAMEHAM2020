using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Facebook.SocialVR.Worlds.Shapeworld.Scripts.Utils.FSM.SafeCoroutine;
using UnityEngine;

namespace Facebook.SocialVR.Worlds.Shapeworld.Scripts.Utils.FSM {
  public class SimpleFSM<TState> : ISimpleFSM<TState> where TState : struct, IConvertible, IComparable {

    #region Constants

    public const string METHOD_SEPERATOR = "__";
    public const string METHOD_PREFIX = "on" + METHOD_SEPERATOR;

    protected static readonly object sWaitForUpdate = null;
    protected static readonly WaitForEndOfFrame sWaitForLateUpdate = new WaitForEndOfFrame();
    protected static readonly WaitForFixedUpdate sWaitForFixedUpdate = new WaitForFixedUpdate();

    #endregion

    #region  Yield Consts

    /// <summary>
    /// yield on this for your Update coroutine
    /// </summary>
    public readonly object WaitForUpdate = sWaitForUpdate;

    /// <summary>
    /// yield on this for your LateUpdate coroutine
    /// </summary>
    public readonly WaitForEndOfFrame WaitForLateUpdate = sWaitForLateUpdate;

    /// <summary>
    /// yield on this for your FixedUpdate coroutine
    /// </summary>
    public readonly WaitForFixedUpdate WaitForFixedUpdate = sWaitForFixedUpdate;

    #endregion

    /// <summary>
    /// Emits after a state, or stage, change has completed
    /// </summary>
    public OnChangeState<TState> onChanged { get; set; }

    /// <summary>
    /// The current state we're on, or transitioning into
    /// </summary>
    public TState currentState {
      get { return actualCurrentState; }
      protected set {
        prevState = actualCurrentState;
        actualCurrentState = value;
      }
    }

    /// <summary>
    /// The previous state we were on
    /// </summary>
    public TState prevState { get; protected set; }

    /// <summary>
    /// The current stage along the transition we are on for the currentState
    /// </summary>
    public StateStage currentStage { get; protected set; }

    /// <summary>
    /// If a ENTERING or EXITING transition is occuring, the state for which that
    /// is executing for
    /// </summary>
    public TState? transitionState { get; protected set; }

    /// <summary>
    /// True if we're in a coroutine transition
    /// </summary>
    public bool isInTransition {
      get { return transitionState.HasValue && currentTransitionRoutine != null; }
    }

    /// <summary>
    /// Queue up the given nextState to be transitioned to next
    /// </summary>
    /// <param name="nextState"></param>
    /// <returns>true iff successfully queued</returns>
    public bool requestStateTransitionTo(TState nextState) {
      return transitionToState(nextState, true);
    }

    /// <summary>
    /// Clear out the queue and transition to the given state immediately
    /// </summary>
    /// <param name="nextState"></param>
    /// <returns>true iff we're able to being the immediate transition</returns>
    public bool setStateImmediateTo(TState nextState) {
      return transitionToState(nextState, false);
    }

    /// <summary>
    /// A simple Finite State Machine that acts on the states given in TState.
    ///
    /// This implementation assumes that given a graph where each state is a node,
    /// every node has a bidirectional edge to every other node, and is thus fully
    /// connected.
    ///
    /// To begin the State Machine, you must first call StartOn() after construction
    /// is complete. See StartOn for more details.
    /// </summary>
    /// <param name="initalState">The state to start at</param>
    public SimpleFSM(TState initalState) {
      typeName = typeof(TState).Name;

      stateMethodMap = new Dictionary<TState, FSMState<TState>>();
      queuedStates = new Queue<TState>();
      queuedStates.Enqueue(initalState);
    }

    /// <summary>
    /// This initializes the machine which will run the coroutines, and the owner
    /// in which all the callbacks exist.
    ///
    /// It's possible for both to be one and the same. However it's much more
    /// powerful when they are seperated for better decoupling of code.
    ///
    /// Method bindings in the "owner" should take the format:
    ///
    /// on__[name of T]__[name of T.state]__[StateStage]
    ///
    /// Note: that the underscores are important here. This also means that the
    /// use of a double underscore __ is a special and restricted character
    /// combination so your own state enum should avoid names that contain it!
    /// This seperator is defined via the const METHOD_SEPERATOR.
    ///
    /// So for example, if you have the state enum:
    ///
    /// public enum MyEnumStates { INIT, EXIT, }
    ///
    /// and you want to know when INIT is WILL_ENTER, your method would look
    /// like:
    ///
    /// void on__MyEnumStates__INIT__WILL_ENTER(MyEnumStates previousState);
    ///
    /// Also note that this is CaSe SeNsItIvE!
    ///
    /// The return types and parameters for each stage are defined in FSMState
    /// as delegates.
    /// </summary>
    /// <param name="machine">Runs any coroutines</param>
    /// <param name="owner">The object that contains all the callbacks</param>
    /// <returns></returns>
    public virtual SimpleFSM<TState> StartOn(MonoBehaviour machine, object owner) {
      this.owner = owner;
      this.machine = machine;

      Debug.LogFormat("Init FSM for `{0}` of type: {1} on state: {2} with machine: {3}",
        this.owner.ToString(),
        typeName,
        currentState,
        this.machine.name.ToString());

      initStates();
      initBindings();

      startFSMUpdateRoutine();

      return this;
    }

    protected void emitOnChange() {
      if (onChanged == null) {
        return;
      }

      onChanged(prevState, currentState, currentStage);
    }

    /// <summary>
    /// If we're already transitioning to a state, we discard the old request
    /// </summary>
    /// <param name="nextState"></param>
    /// <param name="allowTransition">iff true, will execute exit/enter coroutines for the given new state and the current state</param>
    protected virtual bool transitionToState(TState nextState, bool allowTransition) {
      if (nextState.Equals(currentState) || (queuedStates.Contains(nextState) && allowTransition)) {
        // already in state
        // or
        // already queued up the newState, and dont need it immediately
        return false;
      }

      if (allowTransition) {
        // we just want to queue it up as the next state to jump into
        queuedStates.Enqueue(nextState);
      } else {
        FSMState<TState> tState;
        if (currentTransitionRoutine != null && transitionState.HasValue) {
          // already in transition so we need to stop the current transition, and let it try and clean itself up
          machine.StopCoroutine(currentTransitionRoutine);
          currentTransitionRoutine = null;

          // force-exit the transitioning state
          tState = this[transitionState.Value];
        } else {
          // force-exit the current state
          tState = this[currentState];
        }

        // exec tState if any
        // TODO track down why this can happen (i.e. be null)
        if (tState != null) {
          tryCallback(() => {
            currentStage = StateStage.FINALLY_EXITED;

            if (tState.OnFinallyExited != null) {
              tState.OnFinallyExited(nextState);
            }

            emitOnChange();
          });
        }

        if (queuedStates.Count > 0) {
          // clear out all existing queued states, don't need to exit them as they've never been started at this point
          queuedStates.Clear();
        }

        // set immediate instead of waiting for coroutine below
        currentState = nextState;
        currentStage = StateStage.WILL_ENTER;

        // start entering the next state immediately
        FSMState<TState> stateFsm = this[nextState];
        machine.StartCoroutine(enterState(currentState, stateFsm));
      }

      if (queuedStates.Count > 0) {
        startFSMUpdateRoutine();
      } else {
        stopFSMUpdateRoutine();
      }

      return true;
    }

    /// <summary>
    /// main update loop
    /// </summary>
    /// <returns></returns>
    protected IEnumerator onFSMUpdate() {
      while (true) {
        yield return null;

        if (currentTransitionRoutine != null) {
          // already in transition so wait
          continue;
        }

        if (queuedStates.Count == 0) {
          // nothing to do for queued states
          stopFSMUpdateRoutine();
          yield break;
        }

        TState nextState = queuedStates.Dequeue();

        // end current state
        if (didInit) {
          // if we haven't done our first init round yet, nothing to exit!
          yield return exitState(nextState, this[currentState]);
        }

        // begin next state
        yield return enterState(nextState, this[nextState]);
      }
    }

    /// <summary>
    /// Stage transitions for entering a queued state
    /// </summary>
    /// <param name="nextState"></param>
    /// <param name="fsmState"></param>
    /// <returns></returns>
    protected IEnumerator enterState(TState nextState, FSMState<TState> fsmState = null) {
      currentState = nextState;

      if (!didInit) {
        // at this point we're good enough to say we've been initialized
        // if another transition is called while this is still ongoing, we
        // can handle it gracefully
        //
        // it's up to whoever is using SimpleFSM to decide if they want to
        // wait first or not
        didInit = true;
      }

      currentStage = StateStage.WILL_ENTER;
      if (fsmState != null && fsmState.OnWillEnter != null) {
        tryCallback(() => { fsmState.OnWillEnter(nextState); });
      }

      emitOnChange();

      currentStage = StateStage.ENTERING;
      if (fsmState != null && fsmState.OnEntering != null) {
        transitionState = nextState;

        currentTransitionRoutine =
          machine.StartCoroutine(tryCoroutine(fsmState.OnEntering(nextState), "enter routine failed"));
        yield return currentTransitionRoutine;
        currentTransitionRoutine = null;

        transitionState = null;
      }

      emitOnChange();

      currentStage = StateStage.DID_ENTER;
      if (fsmState != null && fsmState.OnDidEnter != null) {
        tryCallback(() => { fsmState.OnDidEnter(nextState); });
      }

      emitOnChange();

      startStateUpdates(nextState, fsmState);
    }

    /// <summary>
    /// Stage transitions for exiting the current state for another queued state
    /// </summary>
    /// <param name="nextState"></param>
    /// <param name="fsmState"></param>
    /// <returns></returns>
    protected IEnumerator exitState(TState nextState, FSMState<TState> fsmState = null) {
      currentStage = StateStage.WILL_EXIT;
      if (fsmState != null && fsmState.OnWillExit != null) {
        tryCallback(() => { fsmState.OnWillExit(nextState); });
      }

      emitOnChange();

      currentStage = StateStage.EXITING;
      if (fsmState != null && fsmState.OnExiting != null) {
        transitionState = currentState;

        currentTransitionRoutine =
          machine.StartCoroutine(tryCoroutine(fsmState.OnExiting(nextState), "exit routine failed"));
        yield return currentTransitionRoutine;
        currentTransitionRoutine = null;

        transitionState = null;
      }

      emitOnChange();

      currentStage = StateStage.DID_EXIT;
      if (fsmState != null && fsmState.OnDidExit != null) {
        tryCallback(() => { fsmState.OnDidExit(nextState); });
      }

      emitOnChange();

      currentStage = StateStage.FINALLY_EXITED;
      if (fsmState != null && fsmState.OnFinallyExited != null) {
        tryCallback(() => { fsmState.OnFinallyExited(nextState); });
      }

      emitOnChange();
    }

    /// <summary>
    /// Begins any ongoing coroutines that the state has specified for their update, fixedupdate and lateupdate loops
    /// </summary>
    /// <param name="curState"></param>
    /// <param name="fsmState"></param>
    protected void startStateUpdates(TState curState, FSMState<TState> fsmState = null) {
      if (fsmState != null && fsmState.OnUpdate != null) {
        stopCurrentUpdateRoutine();

        currentUpdateRoutine =
          machine.StartCoroutine(tryCoroutine(fsmState.OnUpdate(curState, sWaitForUpdate), "exception on update"));
      }

      if (fsmState != null && fsmState.OnFixedUpdate != null) {
        stopCurrentFixedUpdateRoutine();

        currentFixedUpdateRoutine =
          machine.StartCoroutine(tryCoroutine(fsmState.OnFixedUpdate(curState, sWaitForFixedUpdate),
            "exception on fixed update"));
      }

      if (fsmState != null && fsmState.OnLateUpdate != null) {
        stopCurrentLateUpdateRoutine();

        currentLateUpdateRoutine =
          machine.StartCoroutine(tryCoroutine(fsmState.OnLateUpdate(curState, sWaitForLateUpdate),
            "exception on late update"));
      }
    }

    protected void stopStateUpdates() {
      stopCurrentUpdateRoutine();
      stopCurrentFixedUpdateRoutine();
      stopCurrentLateUpdateRoutine();
    }

    protected void stopCurrentUpdateRoutine() {
      if (currentUpdateRoutine != null) {
        machine.StopCoroutine(currentUpdateRoutine);
      }
    }

    protected void stopCurrentLateUpdateRoutine() {
      if (currentLateUpdateRoutine != null) {
        machine.StopCoroutine(currentLateUpdateRoutine);
      }
    }

    protected void stopCurrentFixedUpdateRoutine() {
      if (currentFixedUpdateRoutine != null) {
        machine.StopCoroutine(currentFixedUpdateRoutine);
      }
    }

    /// <summary>
    /// Start main update loop for manage FSM states/stages
    /// </summary>
    protected void startFSMUpdateRoutine() {
      if (updateFSMRoutine != null) {
        return;
      }

      updateFSMRoutine = machine.StartCoroutine(onFSMUpdate());
    }

    protected void stopFSMUpdateRoutine() {
      if (updateFSMRoutine == null) {
        return;
      }

      machine.StopCoroutine(updateFSMRoutine);
      updateFSMRoutine = null;

      // cleanup any state updates still running
      stopStateUpdates();
    }

    /// <summary>
    /// Attempt to run a coroutine and catch any exceptions thrown
    /// </summary>
    /// <param name="routine"></param>
    /// <param name="errorPrefixOnFail"></param>
    /// <returns></returns>
    protected IEnumerator tryCoroutine(IEnumerator routine, string errorPrefixOnFail) {
      if (routine == null) {
        yield break;
      }

      var safeRoutine = machine.startSafeCoroutine(routine);

      yield return safeRoutine.routine;

      try {
        safeRoutine.onComplete();
      } catch (SafeCoroutineException e) {
        Debug.LogErrorFormat("{0}: {1}", errorPrefixOnFail, e.ToString());
      }
    }

    /// <summary>
    /// Try to run a callback and catch any exceptions thrown
    /// </summary>
    /// <param name="cb"></param>
    protected void tryCallback(Action cb) {
      if (cb == null) {
        return;
      }

      try {
        cb();
      } catch (Exception e) {
        Debug.LogErrorFormat("Failed to execute callback: {0}", e);
      }
    }

    /// <summary>
    /// Enumerate all possible states
    /// </summary>
    protected void initStates() {
      var values = Enum.GetValues(typeof(TState));

      States.Clear();
      for (int i = 0; i < values.Length; i++) {
        States.Add((TState) values.GetValue(i));
      }
    }

    /// <summary>
    /// Bind to callback methods for states
    /// </summary>
    protected void initBindings() {
      MethodInfo[] methods = owner
        .GetType()
        .GetMethods(
          BindingFlags.Instance
          | BindingFlags.DeclaredOnly
          | BindingFlags.Public
          | BindingFlags.NonPublic);

      var splitArray = new string[] {METHOD_SEPERATOR,};
      foreach (var method in methods) {
        // make sure we're not autogen'd
        if (method.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true).Length > 0) {
          continue;
        }

        // check for prefix
        if (!method.Name.StartsWith(METHOD_PREFIX)) {
          continue;
        }

        // check for correct structure
        var parsedName = method.Name.Split(splitArray, StringSplitOptions.RemoveEmptyEntries);
        if (parsedName.Length != 4) {
          continue;
        }

        // match enum type name
        if (parsedName[1] != typeName) {
          continue;
        }

        // attempt to parse and match stage name
        StateStage stage;
        try {
          stage = (StateStage) Enum.Parse(typeof(StateStage), parsedName[3]);
        } catch (ArgumentException) {
          continue;
        }

        // attempt to parse and match enum type
        TState methodState;
        try {
          methodState = (TState) Enum.Parse(typeof(TState), parsedName[2]);
        } catch (ArgumentException) {
          continue;
        }

        FSMState<TState> state;
        if (!stateMethodMap.TryGetValue(methodState, out state)) {
          state = new FSMState<TState>();
          stateMethodMap.Add(methodState, state);
        }

        // finally, assign and keep a reference to the callback if possible
        try {
          if (!state.assignStageDelegate(stage, method, owner)) {
            Debug.LogWarningFormat("Unable to assign method delegate for: {0} on method: {1}", stage, method.Name);
          } else {
            Debug.LogFormat("{0} Assigned method.", method.Name);
          }
        } catch (Exception e) {
          Debug.LogWarningFormat("Exception while attempting to assign method delegate for: {0} on method: {1} error: {2}",
            stage,
            method.Name,
            e);
        }
      }
    }

    public override string ToString() {
      return $"SimpleFSM<{currentState}, {currentStage}>";
    }

    /// <summary>
    /// The list of states set to execute once the current once has finished its current
    /// transition, if any.
    /// </summary>
    protected Queue<TState> queuedStates { get; set; }

    protected bool didInit = false;

    protected TState actualCurrentState;

    protected readonly HashSet<TState> States = new HashSet<TState>();

    /// <summary>
    /// The object that holds the callbacks we wish to bind to
    /// </summary>
    protected object owner;

    /// <summary>
    /// The monobehaviour that we'll run coroutines on
    /// </summary>
    protected MonoBehaviour machine;

    protected string typeName;

    protected Dictionary<TState, FSMState<TState>> stateMethodMap;

    /// <returns>can be null</returns>
    protected FSMState<TState> this[TState key] {
      get {
        FSMState<TState> state = null;
        stateMethodMap.TryGetValue(key, out state);

        return state;
      }
    }

    protected Coroutine currentTransitionRoutine = null;
    protected Coroutine updateFSMRoutine = null;

    protected Coroutine currentUpdateRoutine = null;
    protected Coroutine currentLateUpdateRoutine = null;
    protected Coroutine currentFixedUpdateRoutine = null;
  }
}

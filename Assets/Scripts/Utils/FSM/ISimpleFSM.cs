using System;

namespace Facebook.SocialVR.Worlds.Shapeworld.Scripts.Utils.FSM {
  public delegate void OnChangeState<TState>(TState prevState, TState currentState, StateStage currentStage);

  public interface ISimpleFSM<TState> where TState : struct, IConvertible, IComparable {
    OnChangeState<TState> onChanged { get; set; }

    TState currentState { get; }
    TState prevState { get; }
    StateStage currentStage { get; }
    TState? transitionState { get; }

    bool requestStateTransitionTo(TState newState);
    bool setStateImmediateTo(TState newState);
  }
}

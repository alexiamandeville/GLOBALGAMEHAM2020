using System;

namespace Facebook.SocialVR.Worlds.Shapeworld.Scripts.Utils.FSM {
  interface IMappedFSM<TState> where TState : struct, IConvertible, IComparable {
    bool IsTransitionValid(TState from, TState to);
  }
}

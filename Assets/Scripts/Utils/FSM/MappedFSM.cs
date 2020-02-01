using System;
using System.Collections.Generic;

namespace Facebook.SocialVR.Worlds.Shapeworld.Scripts.Utils.FSM {
  public class MappedFSM<TState> : SimpleFSM<TState>, IMappedFSM<TState>
    where TState : struct, IConvertible, IComparable {
    private readonly Dictionary<TState, HashSet<TState>> stateMapLookup;

    public bool IsTransitionValid(TState from, TState to) {
      return stateMapLookup == null
             || (stateMapLookup.ContainsKey(from) && stateMapLookup[from].Contains(to));
    }

    protected override bool transitionToState(TState nextState, bool allowTransition) {
      if (!IsTransitionValid(currentState, nextState)) {
        return false;
      }

      return base.transitionToState(nextState, allowTransition);
    }

    /// <summary>
    /// This is a state machine where you can optionally exactly specify the edges between
    /// each node in the graph that represents the states of the FSM.
    ///
    /// If no map is given, defaults to SimpleFSM behaviour.
    /// </summary>
    /// <param name="stateMap">
    /// A mapping that tells which key TState you can start at to transition to the list of TStates next to it. Ex:
    ///
    /// enum A { ONE, TWO, THREE, }
    ///
    /// Dictionary[A, List[A]] = {
    ///     { ONE, List[TWO, THREE] },
    ///     { TWO, LIST[ONE,] }
    /// }
    ///
    /// This means from state ONE you can transition to TWO or THREE.
    /// From state TWO you can transition to ONE.
    /// From state THREE you can not transition any where else (exit state).
    ///
    /// If you do not provide this parameter, then we revert to SimpleFSM behaviour where all states can go to all other states.
    /// </param>
    public MappedFSM(TState initalState, Dictionary<TState, IEnumerable<TState>> stateMap = null) : base(initalState) {
      if (stateMap != null && stateMap.Count > 0) {
        stateMapLookup = new Dictionary<TState, HashSet<TState>>();

        foreach (var stateKey in stateMap.Keys) {
          if (!stateMapLookup.ContainsKey(stateKey)) {
            stateMapLookup.Add(stateKey, new HashSet<TState>());
          }

          foreach (var state in stateMap[stateKey]) {
            stateMapLookup[stateKey].Add(state);
          }
        }
      }
    }
  }
}

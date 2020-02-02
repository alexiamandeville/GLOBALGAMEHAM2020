using System;
using System.Collections.Generic;
using UnityEngine;

namespace Facebook.SocialVR.Worlds.Shapeworld.Scripts.Utils.FSM {
  /// <summary>
  /// This is a base class you can inherit from, to create a mapped state machine
  ///
  /// This allows you to create a state machine and have its callbacks all exist
  /// within the same unit of code, helping keep it all well organized.
  /// </summary>
  public abstract class SelfContainedFSM<TState> : MappedFSM<TState> where TState : struct, IConvertible, IComparable {
    public SelfContainedFSM<TState> StartOn(MonoBehaviour machine) {
      return (SelfContainedFSM<TState>) base.StartOn(machine, this);
    }

    public override SimpleFSM<TState> StartOn(MonoBehaviour machine, object owner) {
      return StartOn(machine);
    }

    public SelfContainedFSM(TState initalState, Dictionary<TState, IEnumerable<TState>> stateMap = null)
      : base(initalState, stateMap) {
    }
  }
}

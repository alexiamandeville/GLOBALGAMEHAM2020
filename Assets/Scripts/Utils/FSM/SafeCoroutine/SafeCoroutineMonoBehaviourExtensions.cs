using System.Collections;
using UnityEngine;

namespace Facebook.SocialVR.Worlds.Shapeworld.Scripts.Utils.FSM.SafeCoroutine {
  public static class SafeCoroutineMonoBehaviourExtensions {
    public static SafeCoroutine<object> startSafeCoroutine(
      this MonoBehaviour owner,
      IEnumerator innerRoutine
    ) {
      return startSafeCoroutine<object>(owner, innerRoutine);
    }

    public static SafeCoroutine<TReturnVal> startSafeCoroutine<TReturnVal>(
      this MonoBehaviour owner,
      IEnumerator innerRoutine
    ) {
      SafeCoroutine<TReturnVal> coObj = new SafeCoroutine<TReturnVal>();

      coObj.routine = owner.StartCoroutine(coObj.innerRoutine(innerRoutine));

      return coObj;
    }
  }
}

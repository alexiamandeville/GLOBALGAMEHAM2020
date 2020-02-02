using System;
using System.Collections;
using UnityEngine;

namespace Facebook.SocialVR.Worlds.Shapeworld.Scripts.Utils.FSM.SafeCoroutine {
  /// <summary>
  /// Provides a wrapper to coroutines that allows you to:
  ///    1) catch and handle exceptions thrown while it was running
  ///    2) As an added bonus, get a return value from the coroutine of the
  ///       type you specify
  ///
  /// Use MonoBehaviour::startSafeCoroutine to actually start one of these. Usage looks like:
  ///
  /// IEnumerator MyParentRoutine() {
  ///     var innerRoutine = this.startSafeCoroutine(MyInnerRoutine());
  ///     yield return innerRoutine.routine;
  ///     try {
  ///         if (innerRoutinel.value.GetType() == typeof(MyObject)) {
  ///             Debug.Log("Cool");
  ///         }
  ///     } catch (Exception e) {
  ///         Debug.Log("Got exception: " + e);
  ///     }
  /// }
  ///
  /// IEnumerator MyInnerRoutine() {
  ///     yield return new MyObject();
  ///     throw new Exception("RAWR");
  /// }
  ///
  /// This will print out two lines:
  ///  - "Cool"
  ///  - "Got exception: SafeCoroutineException: ... RAWR ..."
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class SafeCoroutine<T> {
    public Coroutine routine;

    public T value {
      get {
        onComplete();

        return Val;
      }
    }

    public void onComplete() {
      if (InnerException != null) {
        throw InnerException;
      }
    }

    public IEnumerator innerRoutine(IEnumerator coroutine) {
      while (true) {
        try {
          if (!coroutine.MoveNext()) {
            yield break;
          }
        } catch (Exception e) {
          this.InnerException = new SafeCoroutineException(e);
          yield break;
        }

        object yielded = coroutine.Current;
        if (yielded != null && yielded.GetType() == typeof(T)) {
          Val = (T) yielded;
          yield break;
        }

        yield return coroutine.Current;
      }
    }

    private T Val;
    private SafeCoroutineException InnerException;
  }
}

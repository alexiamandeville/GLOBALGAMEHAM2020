using System;

namespace Facebook.SocialVR.Worlds.Shapeworld.Scripts.Utils.FSM.SafeCoroutine {
  public class SafeCoroutineException : Exception {
    private Exception inner;

    public SafeCoroutineException(Exception inner) {
      this.inner = inner;
    }

    public override string ToString() {
      return $"Inner Exception: {inner.ToString()}\n------\nOuter Exception: {base.ToString()}";
    }
  }
}

using System;
using UnityEngine;

namespace DefaultNamespace
{
  public class FlipperPlayerInput : BasicPlayerInput
  {
    protected void Update()
    {
      if (currentGamepad == null || !currentGamepad.enabled)
      {
        return;
      }

      if (playerRootTransform == null)
      {
        return;
      }
      
      // check for left thumbstick to move around with
      var moveAxis = currentGamepad.leftStick.ReadValue();
      
      Debug.Log($"LEFT STICK {moveAxis.ToString("N5")}");
    }
  }
}
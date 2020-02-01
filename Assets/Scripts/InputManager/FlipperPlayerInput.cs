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
      
      // move in direction of thumbstick
      var curP = rb.transform.position;
      rb.MovePosition(new Vector3(
        curP.x - (moveAxis.x * moveSpeedMulti),
        curP.y,
        curP.z - (moveAxis.y * moveSpeedMulti)
      ));
    }
  }
}
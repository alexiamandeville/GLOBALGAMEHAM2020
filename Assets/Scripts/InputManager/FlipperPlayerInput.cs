using System;
using UnityEngine;

namespace DefaultNamespace
{
  public class FlipperPlayerInput : BasicPlayerInput
  {
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            CheckForFix();
            CheckForBreak();
        }

        private void CheckForFix()
        {
            if(currentGamepad.buttonSouth.wasPressedThisFrame)
                interactionChecker.TryToFix();
        }

        private void CheckForBreak()
        {
            if (currentGamepad.buttonNorth.wasPressedThisFrame)
                interactionChecker.TryToBreak();
        }
    }
}
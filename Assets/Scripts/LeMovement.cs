using Facebook.SocialVR.Worlds.Shapeworld.Scripts.Utils.FSM;

namespace DefaultNamespace
{
  public partial class LeGame
  {
    public enum InteractionState
    {
      INVALID = 0,
      
      NO_MOVEMENT,
      NO_TELEPORT,
      
      ALL_INTERACTIONS,
      ALL_MOVE_ONLY_GHOSTS_INTERACT,
    }
    protected SimpleFSM<InteractionState> interactFsm;
  }
}
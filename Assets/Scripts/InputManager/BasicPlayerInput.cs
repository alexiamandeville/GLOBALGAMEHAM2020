using UnityEngine;
using UnityEngine.InputSystem;

namespace DefaultNamespace
{
  public abstract class BasicPlayerInput : MonoBehaviour
  {
    [SerializeField] protected Transform playerRootTransform;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected int controllerNumber = 0;
    [SerializeField] protected float moveSpeedMulti;
    [SerializeField] protected BoxCollider playerGroundCollider;

    protected Gamepad currentGamepad;

    public virtual void Awake()
    {
      currentGamepad = Gamepad.all[controllerNumber];
    }
    
    public virtual void FixedUpdate()
    {
      UpdateCharacterPosition();
    }

    protected bool IsMovementEnabled()
    {
      if (currentGamepad == null || !currentGamepad.enabled)
      {
        return false;
      }

      if (playerRootTransform == null)
      {
        return false;
      }

      return true;
    }

    protected void UpdateCharacterPosition()
    {
      if (!IsMovementEnabled())
      {
        return;
      }
      
      // initial pos
      var curP = rb.transform.position;
      var newX = curP.x;
      var newY = curP.y;
      var newZ = curP.z;
      
      // check for left thumbstick to move around with
      var moveAxis = currentGamepad.leftStick.ReadValue();
      if (moveAxis.magnitude > 0)
      {
        var multi = Time.fixedDeltaTime * moveSpeedMulti;

        newX -= (moveAxis.x * multi);
        newZ -= (moveAxis.y * multi);
      }

      // make sure we're properly grounded
      var didHit = Physics.CheckSphere(
        playerGroundCollider.center,
        -.5f, layerMask: Layers.GroundMask, QueryTriggerInteraction.Collide);
      if (!didHit)
      {
        // add ze gravitas
        newY -= (0.1f);
      }
      else
      {
        Debug.Log("ON GROUND");
      }
      
      // apply final movement
      rb.MovePosition(new Vector3(newX, newY, newZ));
    }
  }
}
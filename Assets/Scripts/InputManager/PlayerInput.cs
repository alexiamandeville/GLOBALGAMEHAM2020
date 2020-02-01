
using System;
using Doors;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DefaultNamespace
{
  public class PlayerInput : MonoBehaviour
  {
    [SerializeField] protected Transform playerRootTransform;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected int playerNumber = 0;
    [SerializeField] protected float moveSpeedMulti;
    [SerializeField] protected SphereCollider groundCollider;
    [SerializeField] protected CapsuleCollider playerCollider;

    public Gamepad currentGamepad;
    protected bool IsCurrentlyOnGround;

    public void SetPlayerNumber(int num)
    {
      playerNumber = num;
    }

    public virtual void Start()
    {
      if (Gamepad.all.Count == 0)
      {
        // TODO do something useful here
        Debug.LogWarning("NO CONTROLLERS CONNECTED");
        return;
      }

      currentGamepad = Gamepad.all[playerNumber];
    }
    
    public virtual void FixedUpdate()
    {
      UpdateIsOnGround();
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

    protected void UpdateIsOnGround()
    {
      // make sure we're properly grounded
      IsCurrentlyOnGround = Physics.CheckSphere(
        // ffs the position is in WORLD SPACE
        groundCollider.transform.InverseTransformVector(groundCollider.transform.position),
        groundCollider.radius,
        layerMask: Layers.EnvironmentCollidersMask);
      
      // Debug.Log($"Is On Ground: {IsCurrentlyOnGround}");
    }

    protected void UpdateCharacterPosition()
    {
      if (!IsMovementEnabled())
      {
        return;
      }
      
      // initial pos
      var curP = rb.velocity;
      var newX = curP.x;
      var newY = curP.y;
      var newZ = curP.z;
      
      // check for left thumbstick to move around with
      var moveAxis = currentGamepad.leftStick.ReadValue();
      if (moveAxis.magnitude > 0)
      {
        var multi = /*Time.fixedDeltaTime **/ moveSpeedMulti;

        newX -= (moveAxis.x * multi);
        newZ -= (moveAxis.y * multi);
        
        // Debug.Log($"Player {controllerNumber}, Move{{ x:{newX:N3}, y:{newY:N3}, z:{newZ:N3} }}");
      }

      // apply final velocity
      rb.velocity = new Vector3(newX, newY, newZ);

      // rotate towards movement
      var v = rb.velocity;
      if (Mathf.Abs(v.x) > 0.01f || Mathf.Abs(v.z) > 0.01f)
      {
        var rot = Quaternion.LookRotation(rb.velocity).eulerAngles;
        rb.transform.eulerAngles = new Vector3(0f, rot.y, 0f);
      }
    }
    
    #region Collisions

    private void OnTriggerEnter(Collider other)
    {
      Debug.Log($"OnTriggerEnter with: {other.gameObject.name}");
      
      var door = other.gameObject.GetComponentInParent<Door>();
      if (door != null)
      {
        var newPoint = door.otherDoor.mySpawnPoint;
        playerRootTransform.position = newPoint.position;
        Debug.Log($"Teleport via door {other.gameObject.name}");
      }
    }

    #endregion
  }
}
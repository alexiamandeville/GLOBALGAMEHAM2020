
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
        [SerializeField] protected float rotationSpeedMulti;
        [SerializeField] protected float scroungeMoveSpeedMulti;
        [SerializeField] protected float scroungeDistVibrateScaleDelta = 0.3f;
        [SerializeField] protected float scroungeDistVibrateSpeed = 0.5f;
        [SerializeField] protected SphereCollider groundCollider;
        [SerializeField] protected CapsuleCollider playerCollider;
        [SerializeField] protected float doorCooldownSeconds = 0.2f;

        public Gamepad currentGamepad;
        protected bool IsCurrentlyOnGround;
        protected bool IsScrounging;
        protected float scroungeAccum;
        protected bool canTeleport;

        private ScroungeHotspot currScroungeSpot;

        protected float doorCooldown = 0;

        public void CanTeleport(bool canTeleport)
        {
            this.canTeleport = canTeleport;
        }

        public void SetPlayerNumber(int num)
        {
            playerNumber = num;
        }

        public virtual void FixedUpdate()
        {
            UpdateIsOnGround();
            UpdateCharacterPosition();
        }

        public virtual void Update()
        {
            if (doorCooldown > 0)
            {
                doorCooldown -= Time.deltaTime;
            }
            
            if (currentGamepad != null)
            {
                bool bWasScrounging = IsScrounging;
                IsScrounging = currentGamepad.rightShoulder.isPressed;
                if (bWasScrounging && !IsScrounging)
                    transform.localScale = Vector3.one;
            }

            if (currScroungeSpot != null)
            {
                if (IsScrounging)
                {
                    if (currScroungeSpot.IsConsumed)
                    {
                        transform.localScale = Vector3.one;
                    }
                    else
                    {
                        float distValue = 1.0f - currScroungeSpot.TryScrounge(transform.position, this);
                        scroungeAccum += Time.deltaTime * distValue * scroungeDistVibrateSpeed;

                        transform.localScale = Vector3.one * (1.0f + scroungeDistVibrateScaleDelta * Mathf.Cos(scroungeAccum));
                    }
                }
            }

        }

        protected bool IsMovementEnabled()
        {
            currentGamepad = Gamepad.all[playerNumber];

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
                var multi = /*Time.fixedDeltaTime **/ IsScrounging ? scroungeMoveSpeedMulti : moveSpeedMulti;

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
                rb.transform.rotation = Quaternion.Slerp(
                    rb.transform.rotation, 
                    Quaternion.Euler(new Vector3(0f, rot.y, 0f)),
                    Time.fixedDeltaTime * rotationSpeedMulti);
            }
        }

        #region Collisions

        private void OnTriggerEnter(Collider other)
        {
            if (!canTeleport || doorCooldown > 0)
            {
                return;
            }

            var door = other.gameObject.GetComponentInParent<Door>();
            if (door != null)
            {
                var newPoint = door.otherDoor.mySpawnPoint;
                playerRootTransform.position = newPoint.position;
                doorCooldown = doorCooldownSeconds;
                Debug.Log($"Teleport via door {other.gameObject.name}");
            }

            var room = other.gameObject.GetComponentInParent<Room>();
            if (room != null)
            {
                room.OnPlayerEntered();
            }

            if( currScroungeSpot == null )
            {
                currScroungeSpot = other.gameObject.GetComponentInParent<ScroungeHotspot>();
            }
        }


        private void OnTriggerExit(Collider other)
        {
            var room = other.gameObject.GetComponentInParent<Room>();
            if (room != null)
            {
                room.OnPlayerExited();
            }

            if (currScroungeSpot == other.gameObject.GetComponentInParent<ScroungeHotspot>())
                currScroungeSpot = null;
        }

		#endregion
	}
}
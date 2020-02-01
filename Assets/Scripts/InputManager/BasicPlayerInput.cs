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

    protected Gamepad currentGamepad;

    protected void Awake()
    {
      currentGamepad = Gamepad.all[controllerNumber];
    }
  }
}
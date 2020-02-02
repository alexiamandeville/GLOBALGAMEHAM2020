using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DefaultNamespace;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator = null;
    private PlayerInput playerInput = null;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerInput = GetComponentInParent<PlayerInput>();
    }

    private void Update()
    {
        WalkAnimation();
    }

    private void WalkAnimation()
    {
        Vector2 moveAxis = playerInput.currentGamepad.leftStick.ReadValue();
        bool isWalking = moveAxis == Vector2.zero ? false : true;
        animator.SetBool("Walk", isWalking);
    }
}

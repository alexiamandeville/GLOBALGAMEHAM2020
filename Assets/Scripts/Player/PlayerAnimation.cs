using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DefaultNamespace;

public class PlayerAnimation : MonoBehaviour
{
    public Rigidbody movementRigidBodyRoot;
    public float animMoveSpeedMulti = 0.1f;
    
    private Animator animator = null;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        WalkAnimation();
    }

    private void WalkAnimation()
    {
        var moveAxis = movementRigidBodyRoot.velocity;
        var isWalking = moveAxis.magnitude > 0;

        animator.speed = moveAxis.magnitude * animMoveSpeedMulti;
        animator.SetBool("Walk", isWalking);
    }
}

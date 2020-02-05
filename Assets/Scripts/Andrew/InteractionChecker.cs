using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DefaultNamespace;

public class InteractionChecker : MonoBehaviour
{
    [SerializeField] private LayerMask interactionMask = 0;
    [SerializeField] private float distance = 3.0f;

    private PlayerInput playerInput = null;
    private PlayerController playerController = null;

    private Interactable previousInteractable = null;
    private Interactable targetInteractable = null;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        targetInteractable = CheckForInteractable();
        LookAtTarget();

        InteractWithTarget();
        previousInteractable = targetInteractable;
    }

    private Interactable CheckForInteractable()
    {
        Interactable interactable = null;

        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);

        if(Physics.SphereCast(ray, 1.25f, out hit, distance, interactionMask))
        {
            GameObject hitObject = hit.collider.gameObject;
            interactable = hitObject.GetComponent<Interactable>();
        }

        return interactable;
    }

    private void LookAtTarget()
    {
        if (targetInteractable != previousInteractable)
        {
            if (previousInteractable)
                previousInteractable.LookAway(playerController);

            if (targetInteractable)
                targetInteractable.LookAt(playerController);
        }
    }

    private void InteractWithTarget()
    {
        if (targetInteractable)
        {
            if (playerInput.currentGamepad.buttonSouth.wasPressedThisFrame)
            {
                var istate = playerController.curInteractionState;
                
                if (playerController.playerType == PlayerType.FLIPPER)
                {
                    if (istate == LeGame.InteractionState.ALL_INTERACTIONS)
                    {
                        targetInteractable.Fix(playerController);
                    }
                }

                if (playerController.playerType == PlayerType.GHOST)
                {
                    if (istate == LeGame.InteractionState.ALL_MOVE_ONLY_GHOSTS_INTERACT
                        || istate == LeGame.InteractionState.ALL_INTERACTIONS)
                    {
                        targetInteractable.Break(playerController);
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, transform.forward * distance);
    }
}

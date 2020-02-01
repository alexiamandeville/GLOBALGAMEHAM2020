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

        if(Physics.SphereCast(ray, 0.75f, out hit, distance, interactionMask))
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
                if(playerController.playerType == PlayerType.FLIPPER)
                    targetInteractable.Fix(playerController);

                if (playerController.playerType == PlayerType.GHOST)
                    targetInteractable.Break(playerController);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, transform.forward * distance);
    }
}

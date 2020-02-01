using UnityEngine;

public class InteractionChecker : MonoBehaviour
{
    [SerializeField] private LayerMask interactionMask = 0;
    [SerializeField] private float distance = 3.0f;

    private Interactable previousInteractable = null;
    private Interactable targetInteractable = null;

    private void Update()
    {
        targetInteractable = CheckForInteractable();
        LookAtTarget();
        previousInteractable = targetInteractable;
    }

    private Interactable CheckForInteractable()
    {
        Interactable interactable = null;

        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out hit, distance, interactionMask))
        {
            GameObject hitObject = hit.collider.gameObject;
            interactable = hitObject.GetComponent<Interactable>();
        }

        return interactable;
    }

    private void LookAtTarget()
    {
        if(targetInteractable != previousInteractable)
        {
            if (previousInteractable)
                previousInteractable.LookAway();

            if (targetInteractable)
                targetInteractable.LookAt();
        }
    }

    public void TryToFix()
    {
        if (targetInteractable)
            targetInteractable.Fix();
    }

    public void TryToBreak()
    {
        if (targetInteractable)
            targetInteractable.Break();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, transform.forward * distance);
    }
}

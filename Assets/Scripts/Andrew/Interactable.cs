using UnityEngine;

public class Interactable : MonoBehaviour
{
    private SpriteRenderer spriteRenderer = null;

    protected virtual void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Interaction");
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public virtual void Fix()
    {
        Debug.Log(gameObject.name + " - " + "Fixed.");
    }

    public virtual void Break()
    {
        Debug.Log(gameObject.name + " - " + "Broken.");
    }

    public virtual void LookAt()
    {
        spriteRenderer.enabled = true;
        Debug.Log(gameObject.name + " - " + "Looked at.");
    }

    public virtual void LookAway()
    {
        spriteRenderer.enabled = false;
        Debug.Log(gameObject.name + " - " + "Looked away.");
    }
}

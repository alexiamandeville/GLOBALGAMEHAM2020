using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DefaultNamespace;

public class Interactable : MonoBehaviour
{
    public UnityEvent OnFixed = new UnityEvent();
    public UnityEvent OnBroken = new UnityEvent();
    public FlipperType flipperRequired = FlipperType.INVALID;

    protected bool isBroken = false;

    private ScoreSystem scoreSystem = null;
    private SpriteRenderer spriteRenderer = null;

    protected virtual void Awake()
    {
        scoreSystem = FindObjectOfType<ScoreSystem>();
        gameObject.layer = LayerMask.NameToLayer("Interaction");

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.enabled = false;
    }

    public virtual void Fix(PlayerController player)
    {
        if(isBroken && player.flipperType == flipperRequired)
        {
            isBroken = false;
            OnFixed.Invoke();

            if (scoreSystem)
                scoreSystem.playerScore += 1.0f;

            Debug.Log(gameObject.name + " - " + "Fixed.");
        }
    }

    public virtual void Break(PlayerController player)
    {
        if(!isBroken)
        {
            isBroken = true;
            OnBroken.Invoke();

            if (scoreSystem)
                scoreSystem.playerScore -= 1.0f;

            Debug.Log(gameObject.name + " - " + "Broken.");
        }
    }

    public virtual void LookAt(PlayerController player)
    {
        spriteRenderer.enabled = true;
        Debug.Log(gameObject.name + " - " + "Looked at.");
    }

    public virtual void LookAway(PlayerController player)
    {
        spriteRenderer.enabled = false;
        Debug.Log(gameObject.name + " - " + "Looked away.");
    }
}

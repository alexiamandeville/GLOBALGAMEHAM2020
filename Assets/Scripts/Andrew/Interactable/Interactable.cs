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

    [SerializeField] private GameObject breakIndicator = null;
    [SerializeField] private GameObject fixIndicator = null;

    protected virtual void Awake()
    {
        scoreSystem = FindObjectOfType<ScoreSystem>();
        gameObject.layer = LayerMask.NameToLayer("Interaction");

        breakIndicator.SetActive(false);
        fixIndicator.SetActive(false);
    }

    public virtual void Fix(PlayerController player)
    {
        if (CanBeFixed(player.flipperType))
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
        if (CanBeBroken())
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
        if (player.playerType == PlayerType.FLIPPER)
        {
            if (CanBeFixed(player.flipperType))
                fixIndicator.SetActive(true);
        }

        if (player.playerType == PlayerType.GHOST)
        {
            if (CanBeBroken())
                breakIndicator.SetActive(true);
        }

        Debug.Log(gameObject.name + " - " + "Looked at.");
    }

    public virtual void LookAway(PlayerController player)
    {
        if (player.playerType == PlayerType.FLIPPER)
            fixIndicator.SetActive(false);

        if (player.playerType == PlayerType.GHOST)
            breakIndicator.SetActive(false);

        Debug.Log(gameObject.name + " - " + "Looked away.");
    }

    private bool CanBeFixed(FlipperType flipperType)
    {
        return flipperType == flipperRequired && isBroken;
    }

    private bool CanBeBroken()
    {
        return !isBroken;
    }
}

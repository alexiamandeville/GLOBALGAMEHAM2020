using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DefaultNamespace;

public class Interactable : MonoBehaviour
{
    public UnityEvent OnFixed = new UnityEvent();
    public UnityEvent OnBroken = new UnityEvent();

    public bool startAsBroken = false;
    public FlipperType flipperRequired = FlipperType.INVALID;

    private bool isBroken = false;
    private ScoreSystem scoreSystem = null;

    [SerializeField] private GameObject breakIndicator = null;
    [SerializeField] private GameObject fixIndicator = null;

    private void Awake()
    {
        scoreSystem = FindObjectOfType<ScoreSystem>();
        gameObject.layer = LayerMask.NameToLayer("Interaction");

        breakIndicator.SetActive(false);
        fixIndicator.SetActive(false);
    }

    private void Start()
    {
        if (startAsBroken)
            Break(null);
    }

    public void Reset()
    {
        isBroken = false;
        OnFixed.Invoke();
    }

    public void Fix(PlayerController player)
    {
        if (CanBeFixed(player.flipperType))
        {
            isBroken = false;
            OnFixed.Invoke();

            // Debug.Log(gameObject.name + " - " + "Fixed.");
        }
    }

    public void Break(PlayerController player)
    {
        if (CanBeBroken())
        {
            isBroken = true;
            OnBroken.Invoke();

            // Debug.Log(gameObject.name + " - " + "Broken.");
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

        // Debug.Log(gameObject.name + " - " + "Looked at.");
    }

    public void LookAway(PlayerController player)
    {
        if (player.playerType == PlayerType.FLIPPER)
            fixIndicator.SetActive(false);

        if (player.playerType == PlayerType.GHOST)
            breakIndicator.SetActive(false);

        // Debug.Log(gameObject.name + " - " + "Looked away.");
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

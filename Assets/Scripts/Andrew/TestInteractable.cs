using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInteractable : Interactable
{
    private MeshRenderer meshRenderer = null;

    protected override void Awake()
    {
        base.Awake();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public override void Fix()
    {
        base.Fix();
        meshRenderer.material.color = Color.blue;
    }

    public override void Break()
    {
        base.Break();
        meshRenderer.material.color = Color.red;
    }
}

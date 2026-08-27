using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CustomerController))]
public sealed class CustomerPatienceVisual : MonoBehaviour
{
    private static readonly int BaseColorHash = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorHash = Shader.PropertyToID("_Color");
    private static readonly Color ExpiredColor = new(1f, 0.2f, 0.2f, 1f);

    private readonly List<Renderer> renderers = new();
    private CustomerController customer;
    private bool wasExpired;

    private void Awake()
    {
        customer = GetComponent<CustomerController>();
    }

    private void LateUpdate()
    {
        bool isExpired = customer != null && customer.DidPatienceExpire;
        if (isExpired)
        {
            RefreshRenderers();
            ApplyExpiredColor();
        }
        else if (wasExpired)
        {
            RestoreColor();
        }

        wasExpired = isExpired;
    }

    private void RefreshRenderers()
    {
        renderers.Clear();
        GetComponentsInChildren(false, renderers);
    }

    private void ApplyExpiredColor()
    {
        for (int i = 0; i < renderers.Count; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || renderer.sharedMaterial == null)
            {
                continue;
            }

            MaterialPropertyBlock properties = new();
            renderer.GetPropertyBlock(properties);
            if (renderer.sharedMaterial.HasProperty(BaseColorHash))
            {
                properties.SetColor(BaseColorHash, ExpiredColor);
                renderer.SetPropertyBlock(properties);
            }
            else if (renderer.sharedMaterial.HasProperty(ColorHash))
            {
                properties.SetColor(ColorHash, ExpiredColor);
                renderer.SetPropertyBlock(properties);
            }
        }
    }

    private void RestoreColor()
    {
        for (int i = 0; i < renderers.Count; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].SetPropertyBlock(null);
            }
        }
    }
}

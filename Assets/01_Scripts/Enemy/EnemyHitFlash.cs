using System.Collections;
using UnityEngine;

public class EnemyHitFlash : MonoBehaviour
{
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private const string EmissionKeyword = "_EMISSION";

    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private Color flashColor;

    private Renderer[] renderers;
    private MaterialPropertyBlock propertyBlock;
    private Coroutine flashRoutine;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();

        foreach (Renderer targetRenderer in renderers)
        {
            foreach (Material material in targetRenderer.sharedMaterials)
            {
                if (material != null && material.HasProperty(EmissionColorId))
                {
                    material.EnableKeyword(EmissionKeyword);
                }
            }
        }
    }

    public void Play()
    {
        if (!isActiveAndEnabled) return;

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashCo());
    }

    private IEnumerator FlashCo()
    {
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            float strength = 1f - elapsed / flashDuration;
            SetEmission(flashColor * strength);

            elapsed += Time.deltaTime;
            yield return null;
        }

        SetEmission(Color.black);
        flashRoutine = null;
    }

    private void SetEmission(Color color)
    {
        foreach (Renderer targetRenderer in renderers)
        {
            if (targetRenderer == null) continue;

            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(EmissionColorId, color);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    private void OnDisable()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        if (propertyBlock != null)
        {
            SetEmission(Color.black);
        }

        flashRoutine = null;
    }
}

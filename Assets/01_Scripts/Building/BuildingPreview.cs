using UnityEngine;

public class BuildingPreview : MonoBehaviour
{
    [SerializeField] private Material validMaterial;
    [SerializeField] private Material invalidMaterial;

    private Renderer previewRenderer;

    private void Awake()
    {
        previewRenderer = GetComponentInChildren<Renderer>();
    }

    public void SetPreview(bool isValid)
    {
        Material targetMaterial = isValid ? validMaterial : invalidMaterial;

        if (targetMaterial == null) return;

        previewRenderer.material = targetMaterial;
    }
}

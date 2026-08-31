using System.Collections.Generic;
using UnityEngine;

public class WorkshopExpansionPreview : MonoBehaviour
{
    [Header("확장 오브젝트")]
    [SerializeField] private WorkshopStageVisualController visualController;

    [Header("프리뷰 표시")]
    [SerializeField] private Material previewMaterial;

    // 프리뷰로 보여지는 벽/바닥 오브젝트
    private GameObject shownFloorRoot;
    private GameObject shownWallRoot;
    private GameObject shownObstacleRoot;

    // 렌더러별 기존 재질 저장
    private readonly Dictionary<Renderer, Material[]> originalMaterials = new();

    public bool Show(int stage)
    {
        Hide();

        if (visualController == null) return false;

        if (previewMaterial == null) return false;

        if (!visualController.TryGetStageRoots(stage, out shownFloorRoot, out shownWallRoot, out shownObstacleRoot))
        {
            shownFloorRoot = null;
            shownWallRoot = null;
            shownObstacleRoot = null;

            return false;
        }

        SetShownRootsActive(true);

        ApplyPreviewMaterial(shownFloorRoot);

        if (shownWallRoot != shownFloorRoot)
        {
            ApplyPreviewMaterial(shownWallRoot);
        }

        return true;
    }

    public void Hide()
    {
        RestoreOriginalMaterials();

        SetShownRootsActive(false);

        shownFloorRoot = null;
        shownWallRoot = null;
        shownObstacleRoot = null;
    }

    private void ApplyPreviewMaterial(GameObject root)
    {
        if (root == null) return;

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;

            // floor와 wall이 겹치는 경우 중복 저장 방지
            if (originalMaterials.ContainsKey(renderer))
            {
                continue;
            }

            Material[] currentMaterials = renderer.sharedMaterials;

            // 기존 재질을 딕셔너리에 렌더러별로 저장
            originalMaterials.Add(renderer, currentMaterials);

            Material[] previewMaterials = new Material[currentMaterials.Length];

            for (int i = 0; i < previewMaterials.Length; i++)
            {
                previewMaterials[i] = previewMaterial;
            }

            renderer.sharedMaterials = previewMaterials;
        }
    }

    private void RestoreOriginalMaterials()
    {
        foreach (KeyValuePair<Renderer, Material[]> pair in originalMaterials)
        {
            Renderer targetRenderer = pair.Key;

            if (targetRenderer != null)
            {
                targetRenderer.sharedMaterials = pair.Value;
            }
        }

        originalMaterials.Clear();
    }

    private void SetShownRootsActive(bool active)
    {
        if (shownFloorRoot != null)
        {
            shownFloorRoot.SetActive(active);
        }

        if (shownWallRoot != null)
        {
            shownWallRoot.SetActive(active);
        }

        if (shownObstacleRoot != null)
        {
            shownObstacleRoot.SetActive(!active);
        }
    }

    private void OnDisable()
    {
        Hide();
    }
}

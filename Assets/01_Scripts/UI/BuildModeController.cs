using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class BuildModeController : MonoBehaviour
{
    [SerializeField] private RectTransform buildMenu;
    [SerializeField] private Canvas buildMenuCanvas;
    [SerializeField] private List<BuildableArea> buildableAreas = new();
    [SerializeField] private InputManager inputManager;

    [Header("카메라 제어")]
    [SerializeField] private Transform player;
    [SerializeField] private CameraModeController cameraModeController;

    [Header("배치 시스템")]
    [SerializeField] private PlacementSystem placementSystem;

    [Header("편집 모드 버튼")]
    [SerializeField] private Button editModeButton;
    [SerializeField] private Button sellModeButton;

    [Header("판매 확인 UI")]
    [SerializeField] private Button sellConfirmButton;
    [SerializeField] private Button sellCancelButton;

    [Header("현재 배치 모드 표시")]
    [SerializeField] private GameObject currentPlacementMode;
    [SerializeField] private TMP_Text currentPlacementModeText;

    [Header("이벤트")]
    [SerializeField] private UnityEvent onBuildModeClosed;

    [Header("애니메이션")]
    [SerializeField] private float hiddenPadding = 20f;
    [SerializeField] private float duration = 0.35f;
    [SerializeField] private Ease openEase = Ease.OutBack;
    [SerializeField] private Ease closeEase = Ease.InCubic;

    private Vector2 visiblePos;
    private Vector2 hiddenPos;
    private Sequence anim;

    private BuildableArea activeArea;

    public bool IsBuildMode { get; private set; }

    private void Awake()
    {
        SetGridViewVisible(false);

        // 에디터에 배치한 위치를 최종 위치로 사용
        visiblePos = buildMenu.anchoredPosition;

        // 패널 높이만큼 화면 아래로 내린 위치
        hiddenPos = visiblePos + Vector2.down * (buildMenu.rect.height + hiddenPadding);

        buildMenu.anchoredPosition = hiddenPos;
        buildMenuCanvas.enabled = false;
        IsBuildMode = false;
    }

    private void OnEnable()
    {
        if (inputManager != null)
        {
            inputManager.OnCancelPressed += HandleCancelAction;
        }

        if (placementSystem != null)
        {
            placementSystem.ModeChanged += HandlePlacementModeChanged;

            editModeButton?.onClick.AddListener(placementSystem.ToggleRelocateMode);
            sellModeButton?.onClick.AddListener(placementSystem.ToggleSellMode);

            sellConfirmButton?.onClick.AddListener(HandleSellConfirmClicked);
            sellCancelButton?.onClick.AddListener(HandleSellCancelClicked);

            HandlePlacementModeChanged(placementSystem.CurrentMode);
        }
    }

    private void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.OnCancelPressed -= HandleCancelAction;
        }

        if (placementSystem != null)
        {
            placementSystem.ModeChanged -= HandlePlacementModeChanged;

            editModeButton?.onClick.RemoveListener(placementSystem.ToggleRelocateMode);
            sellModeButton?.onClick.RemoveListener(placementSystem.ToggleSellMode);

            sellConfirmButton?.onClick.RemoveListener(placementSystem.ConfirmSell);
            sellCancelButton?.onClick.RemoveListener(placementSystem.CancelCurrentAction);
        }

        anim?.Kill();
        anim = null;
    }

    public void ToggleBuildMode()
    {
        if (IsBuildMode)
        {
            AudioManager.Instance.PlaySFX(ESFXType.UI_Open);
            CloseBuildMode();
        }
        else
        {
            AudioManager.Instance.PlaySFX(ESFXType.UI_Close);
            OpenBuildMode();
        }
    }

    public void OpenBuildMode()
    {
        if (IsBuildMode) return;

        if (TryFindPlayerArea(out activeArea))
        {
            cameraModeController?.EnterEdgeScroll(activeArea.CameraBounds);
        }

        IsBuildMode = true;
        placementSystem.SetBuildModeActive(true);

        anim?.Kill();

        buildMenu.gameObject.SetActive(true);
        buildMenuCanvas.enabled = true;

        anim = DOTween.Sequence().
            Join(buildMenu.DOAnchorPos(visiblePos, duration)
            .SetEase(openEase))
            .SetUpdate(true);

        SetGridViewVisible(true);
    }

    public void CloseBuildMode()
    {
        if (!IsBuildMode && !buildMenu.gameObject.activeSelf)
        {
            return;
        }

        cameraModeController?.FollowPlayer();
        activeArea = null;

        IsBuildMode = false;
        placementSystem.SetBuildModeActive(false);

        onBuildModeClosed?.Invoke();

        anim?.Kill();

        anim = DOTween.Sequence().
            Join(buildMenu.DOAnchorPos(hiddenPos, duration).SetEase(closeEase))
            .OnComplete(() => 
            {
                buildMenuCanvas.enabled = false;
                anim = null;
            })
            .SetUpdate(true);

        SetGridViewVisible(false);
    }

    private void SetGridViewVisible(bool visible)
    {
        foreach (BuildableArea area in buildableAreas)
        {
            if (area == null) continue;

            area.SetGridVisible(visible);
        }
    }

    private void HandlePlacementModeChanged(PlacementMode mode)
    {
        bool isEditMode =
            mode == PlacementMode.RelocateSelect ||
            mode == PlacementMode.RelocatePlacement;

        bool isSellMode =
            mode == PlacementMode.SellSelect ||
            mode == PlacementMode.SellConfirm;

        bool showCurrentMode = isEditMode || isSellMode;

        if (currentPlacementMode != null)
        {
            currentPlacementMode.SetActive(showCurrentMode);
        }

        if (currentPlacementModeText != null)
        {
            if (isEditMode)
            {
                currentPlacementModeText.text = "재배치 모드";
            }

            if (isSellMode)
            {
                currentPlacementModeText.text = "판매 모드";
            }
        }

        SetSellConfirmVisible(mode == PlacementMode.SellConfirm);
    }

    private void SetSellConfirmVisible(bool visible)
    {
        if (sellConfirmButton != null)
        {
            sellConfirmButton.gameObject.SetActive(visible);
        }

        if (sellCancelButton != null)
        {
            sellCancelButton.gameObject.SetActive(visible);
        }
    }

    private bool TryFindPlayerArea(out BuildableArea foundArea)
    {
        foundArea = null;

        if (player == null) return false;

        foreach (BuildableArea area in buildableAreas)
        {
            if (area != null && area.ContainsWorldXZ(player.position))
            {
                foundArea = area;
                return true;
            }
        }

        return false;
    }

    private void HandleSellConfirmClicked()
    {
        if (placementSystem == null || placementSystem.CurrentMode != PlacementMode.SellConfirm)
        {
            return;
        }

        AudioManager.Instance.PlaySFX(ESFXType.UI_Comfirm);
        placementSystem.ConfirmSell();
    }

    private void HandleSellCancelClicked()
    {
        if (placementSystem == null || placementSystem.CurrentMode != PlacementMode.SellConfirm)
        {
            return;
        }

        AudioManager.Instance.PlaySFX(ESFXType.UI_Cancel);
        placementSystem.CancelCurrentAction();
    }

    private void HandleCancelAction()
    {
        if (placementSystem != null && placementSystem.IsPlacementMode)
        {
            placementSystem.CancelCurrentAction();
            return;
        }

        if (placementSystem != null && placementSystem.IsRelocateMode)
        {
            placementSystem.CancelCurrentAction();
            return;
        }

        if (placementSystem != null && placementSystem.IsSellMode)
        {
            placementSystem.CancelCurrentAction();
            return;
        }

        CloseBuildMode();
    }
}

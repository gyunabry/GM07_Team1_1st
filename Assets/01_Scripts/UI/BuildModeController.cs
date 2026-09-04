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

    [Header("배치 모드 닫기 버튼")]
    [SerializeField] private Button exitButton;

    [Header("편집 모드 버튼")]
    [SerializeField] private Button editModeButton;
    [SerializeField] private Button sellModeButton;

    [Header("판매 확인 UI")]
    [SerializeField] private Button sellConfirmButton;
    [SerializeField] private Button sellCancelButton;

    [Header("판매 불가 안내")]
    [SerializeField] private WarningPopupController saleWarningPopup;
    [SerializeField, TextArea] 
    private string notSellableMessage =
        "이 시설은 판매할 수 없습니다.";
    [SerializeField, TextArea] 
    private string minimumReuqiredMessage =
        "이 시설은 최소 1개가 필요하므로 판매할 수 없습니다.";


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

            placementSystem.SaleRejection += HandleSaleRejected;

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

            placementSystem.SaleRejection -= HandleSaleRejected;

            editModeButton?.onClick.RemoveListener(placementSystem.ToggleRelocateMode);
            sellModeButton?.onClick.RemoveListener(placementSystem.ToggleSellMode);

            sellConfirmButton?.onClick.RemoveListener(HandleSellConfirmClicked);
            sellCancelButton?.onClick.RemoveListener(HandleSellCancelClicked);
        }

        saleWarningPopup?.ClosePopup();
        anim?.Kill();
        anim = null;
    }

    public void OpenBuildMode()
    {
        if (IsBuildMode) return;

        if (TryFindPlayerArea(out activeArea))
        {
            cameraModeController?.EnterEdgeScroll(activeArea.CameraBounds);
        }

        AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);
        exitButton.gameObject.SetActive(true);

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

        AudioManager.Instance.PlaySFX(ESFXType.UI_Close);
        exitButton.gameObject.SetActive(false);

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

        if (!isSellMode)
        {
            saleWarningPopup?.ClosePopup();
        }

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
        if (saleWarningPopup != null && saleWarningPopup.isOpen)
        {
            saleWarningPopup.ClosePopup();
            return;
        }

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

    private void HandleSaleRejected(SaleBlockReason reason)
    {
        string message;

        switch (reason)
        {
            case SaleBlockReason.NotSellable:
                message = notSellableMessage;
                break;
            case SaleBlockReason.MinimumRequiredCount:
                message = minimumReuqiredMessage;
                break;
            default:
                return;
        }

        saleWarningPopup?.ShowMessage(message);
    }
}

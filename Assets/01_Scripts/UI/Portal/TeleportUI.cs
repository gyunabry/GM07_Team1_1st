using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TeleportUI : MonoBehaviour
{
    private const int DestinationCount = 3;

    [Header("팝업")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GraphicRaycaster graphicRaycaster;

    [Header("참조")]
    [SerializeField] private HuntingFieldManager fieldManager;

    [Header("버튼")]
    [SerializeField] private Button closeButton;
    [SerializeField] private PortalButtonView[] destinationButtons = new PortalButtonView[DestinationCount];

    private readonly Dictionary<string, Portal> activePortalById = new();

    private UnityAction[] destinationHandlers;

    private Portal activeSourcePortal;
    private GameObject activeInteractor;
    private Portal[] activeDestinations;

    private bool isOpen;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        BindButtons();
        SetVisible(false);
    }

    private void Start()
    {
        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.CurrencyChanged += HandleCurrencyChanged;
            CurrencySystem.Instance.LevelUp += HandleLevelUp;
        }

        if (fieldManager != null)
        {
            fieldManager.StateChanged += HandleFieldStateChanged;
        }
    }

    private void OnDestroy()
    {
        UnbindButtons();

        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.CurrencyChanged -= HandleCurrencyChanged;
            CurrencySystem.Instance.LevelUp -= HandleLevelUp;
        }

        if (fieldManager != null)
        {
            fieldManager.StateChanged -= HandleFieldStateChanged;
        }
    }

    public bool OpenUI(Portal sourcePortal, GameObject interactor, Portal[] destinations)
    {
        if (sourcePortal == null || interactor == null)
        {
            Debug.LogWarning($"{name} 포탈 UI를 열 수 없습니다.");
            return false;
        }

        if (canvas == null || graphicRaycaster == null || closeButton == null)
        {
            Debug.LogWarning($"{name} 팝업 UI 참조가 완성되지 않았습니다.");
            return false;
        }

        activeSourcePortal = sourcePortal;
        activeInteractor = interactor;

        CacheDestinations(destinations);

        SetVisible(true);
        RefreshDestinationButtons();

        return true;
    }

    public void CloseUI()
    {
        SetVisible(false);

        activeSourcePortal = null;
        activeInteractor = null;
        activeDestinations = null;
    }

    // 버튼 눌렀을 때 실행되는 메서드
    private void SelectDestination(int index)
    {
        PortalButtonView view = destinationButtons[index];
        HuntingFieldUnlockDataSO data = view.Data;

        if (!activePortalById.TryGetValue(data.DestinationId, out Portal destination))
        {
            return;
        }

        // 만약 해금되지 않은 사냥터라면 해금 시도 후 버튼 갱신
        if (!fieldManager.IsUnlocked(data))
        {
            if (fieldManager.TryUnlock(data))
            {
                RefreshDestinationButtons();
            }

            return;
        }

        // 순간이동 성공 시 UI 닫기
        if (activeSourcePortal.TryTeleportTo(activeInteractor, destination))
        {
            CloseUI();
        }
    }

    private void BindButtons()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseUI);
        }

        destinationHandlers = new UnityAction[destinationButtons.Length];

        for (int i = 0; i < destinationButtons.Length; i++)
        {
            PortalButtonView button = destinationButtons[i];

            if (button == null) continue;

            int destinationIndex = i;

            destinationHandlers[i] = () => SelectDestination(destinationIndex);

            button.Button.onClick.AddListener(destinationHandlers[i]);
        }
    }

    private void UnbindButtons()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseUI);
        }

        if (destinationHandlers == null) return;

        for (int i = 0; i < destinationButtons.Length; i++)
        {
            if (destinationButtons[i] != null && destinationHandlers[i] != null)
            {
                destinationButtons[i].Button.onClick.RemoveListener(destinationHandlers[i]);
            }
        }
    }

    private void RefreshDestinationButtons()
    {
        foreach (PortalButtonView view in destinationButtons)
        {
            if (view == null || view.Data == null) continue;

            bool portalExists = activePortalById.ContainsKey(view.Data.DestinationId);

            bool unlocked = fieldManager.IsUnlocked(view.Data);

            bool canUnlock = fieldManager.CanUnlock(view.Data);

            view.Refresh(portalExists, unlocked, canUnlock);
        }
    }

    private void CacheDestinations(Portal[] destinations)
    {
        activePortalById.Clear();

        if (destinations == null) return;

        foreach (Portal portal in destinations)
        {
            if (portal == null || string.IsNullOrWhiteSpace(portal.DestinationId))
            {
                continue;
            }

            activePortalById[portal.DestinationId] = portal;
        }
    }

    private void SetVisible(bool visible)
    {
        isOpen = visible;

        if (canvas != null) canvas.enabled = visible;
        if (graphicRaycaster != null) graphicRaycaster.enabled = visible;
    }

    private void HandleCurrencyChanged(int money, int exp)
    {
        if (isOpen)
        {
            RefreshDestinationButtons();
        }
    }

    private void HandleLevelUp()
    {
        if (isOpen)
        {
            RefreshDestinationButtons();
        }
    }

    private void HandleFieldStateChanged()
    {
        if (isOpen)
        {
            RefreshDestinationButtons();
        }
    }
}

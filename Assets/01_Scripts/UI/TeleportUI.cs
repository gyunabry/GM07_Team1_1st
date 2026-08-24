using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TeleportUI : MonoBehaviour
{
    private const int DestinationCount = 3;

    [Header("팝업")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GraphicRaycaster graphicRaycaster;
    [Tooltip("투명 이미지")]
    //[SerializeField] private Image worldInputBlocker;

    [Header("입력")]
    //[SerializeField] private InputManager inputManager;
    [SerializeField] private Button closeButton;

    [Header("사냥터 선택")]
    [SerializeField] private Button[] destinationButtons = new Button[DestinationCount];

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

    private void OnDestroy()
    {
        UnbindButtons();
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
        activeDestinations = destinations;

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

    private void SelectDestination(int index)
    {
        if (!isOpen || activeSourcePortal == null || activeInteractor == null)
        {
            return;
        }

        if (activeDestinations == null ||
            index < 0 ||
            index >= activeDestinations.Length)
        {
            Debug.LogWarning($"{name} {index}번 목적지가 없습니다.");
            return;
        }

        Portal destination = activeDestinations[index];

        if (destination == null)
        {
            Debug.LogWarning($"{name} {index}번 사냥터 포탈이 연결되지 않았습니다.");
            return;
        }

        if (!activeSourcePortal.TryTeleportTo(activeInteractor, destination))
        {
            Debug.LogWarning($"{name} {destination.name}으로 이동하지 못했습니다.");
        }

        CloseUI();
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
            Button button = destinationButtons[i];

            if (button == null) continue;

            int destinationIndex = i;

            destinationHandlers[i] = () => SelectDestination(destinationIndex);

            button.onClick.AddListener(destinationHandlers[i]);
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
                destinationButtons[i].onClick.RemoveListener(destinationHandlers[i]);
            }
        }
    }

    private void RefreshDestinationButtons()
    {
        for (int i = 0; i < destinationButtons.Length; i++)
        {
            if (destinationButtons[i] != null) continue;

            destinationButtons[i].interactable = 
                activeDestinations != null && 
                i < activeDestinations.Length 
                && activeDestinations[i] != null;
        }
    }

    private void SetVisible(bool visible)
    {
        isOpen = visible;

        if (canvas != null) canvas.enabled = visible;
        if (graphicRaycaster != null) graphicRaycaster.enabled = visible;
    }
}

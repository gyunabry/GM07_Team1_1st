using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BuildModeController : MonoBehaviour
{
    [SerializeField] private RectTransform buildMenu;
    [SerializeField] private Canvas buildMenuCanvas;
    [SerializeField] private GameObject gridView;
    [SerializeField] private InputManager inputManager;

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

    public bool IsBuildMode { get; private set; }

    private void Awake()
    {
        gridView.SetActive(false);

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
            inputManager.OnExit += CloseBuildMode;
        }
    }

    private void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.OnExit -= CloseBuildMode;
        }

        anim?.Kill();
        anim = null;
    }

    public void ToggleBuildMode()
    {
        if (IsBuildMode) CloseBuildMode();
        else OpenBuildMode();
    }

    public void OpenBuildMode()
    {
        if (IsBuildMode) return;

        IsBuildMode = true;

        anim?.Kill();

        buildMenu.gameObject.SetActive(true);
        buildMenuCanvas.enabled = true;

        anim = DOTween.Sequence().
            Join(buildMenu.DOAnchorPos(visiblePos, duration)
            .SetEase(openEase))
            .SetUpdate(true);

        gridView.SetActive(true);
    }

    public void CloseBuildMode()
    {
        if (!IsBuildMode && !buildMenu.gameObject.activeSelf)
        {
            return;
        }

        IsBuildMode = false;

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

        gridView.SetActive(false);
    }
}

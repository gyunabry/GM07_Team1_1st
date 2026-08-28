using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public enum CameraMode
{
    PlayerFollow,
    EdgeScroll
}

/// <summary>
/// Switches a Cinemachine camera between player following and mouse edge scrolling.
/// Attach this component to the same GameObject as the CinemachineCamera.
/// </summary>
[RequireComponent(typeof(CinemachineCamera))]
public class CameraModeController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Transform playerFollowTarget;

    [Header("엣지 스크롤")]
    [SerializeField, Min(1f)] private float edgeThickness = 20f;
    [SerializeField, Min(0f)] private float moveSpeed = 12f;
    [SerializeField] private bool blockWhenPointerOverUI = false;
    [SerializeField] private CameraMode initialMode = CameraMode.PlayerFollow;

    [Header("줌 설정")]
    [SerializeField] private CinemachinePositionComposer positionComposer;
    [SerializeField] private float minCameraDistance = 8f;
    [SerializeField] private float maxCameraDistance = 16f;
    [SerializeField] private float zoomSpeed = 0.01f;
    [SerializeField] private float zoomSmoothTime = 0.1f; // 부드러운 줌 효과를 위한 lerp 시간

    private float targetCameraDistance;
    private float zoomVelocity;

    private Transform freeCameraTarget;
    private CameraMode currentMode;
    private bool hasLoggedMissingReference;

    // 현재 선택된 영역
    private Collider activeEdgeScrollBounds;

    public CameraMode CurrentMode => currentMode;

    private void Awake()
    {
        if (cinemachineCamera == null)
        {
            cinemachineCamera = GetComponent<CinemachineCamera>();
        }

        if (playerFollowTarget == null && cinemachineCamera != null)
        {
            playerFollowTarget = cinemachineCamera.Follow;
        }

        if (positionComposer == null)
        {
            positionComposer = GetComponent<CinemachinePositionComposer>();
        }

        if (positionComposer != null)
        {
            targetCameraDistance = positionComposer.CameraDistance;
        }

        currentMode = CameraMode.PlayerFollow;
    }

    private void Start()
    {
        SetMode(initialMode);
    }

    private void Update()
    {
        HandleZoom();

        if (currentMode != CameraMode.EdgeScroll || !CanEdgeScroll())
        {
            return;
        }

        Vector2 direction = GetEdgeScrollDirection(Mouse.current.position.ReadValue());

        if (direction == Vector2.zero)
        {
            return;
        }

        MoveFreeCameraTarget(direction);
    }

    public void SetMode(CameraMode mode)
    {
        if (mode == currentMode)
        {
            return;
        }

        if (!HasCameraAndPlayerTarget())
        {
            return;
        }

        if (mode == CameraMode.EdgeScroll)
        {
            if (activeEdgeScrollBounds == null)
            {
                LogMissingReferenceOnce("Edge Scroll Bounds Collider가 연결되지 않았습니다.");
                return;
            }

            EnsureFreeCameraTarget();
            freeCameraTarget.position = ClampToBounds(playerFollowTarget.position);
            cinemachineCamera.Follow = freeCameraTarget;
        }
        else
        {
            cinemachineCamera.Follow = playerFollowTarget;
        }

        currentMode = mode;
    }

    public void SetEdgeScrollEnabled(bool enabled)
    {
        SetMode(enabled ? CameraMode.EdgeScroll : CameraMode.PlayerFollow);
    }

    public bool EnterEdgeScroll(Collider bounds)
    {
        if (!HasCameraAndPlayerTarget() || bounds == null)
        {
            return false;
        }

        activeEdgeScrollBounds = bounds;

        EnsureFreeCameraTarget();

        freeCameraTarget.position = ClampToBounds(playerFollowTarget.position);

        cinemachineCamera.Follow = freeCameraTarget;
        currentMode = CameraMode.EdgeScroll;

        return true;
    }

    public void FollowPlayer()
    {
        if (!HasCameraAndPlayerTarget()) return;

        cinemachineCamera.Follow = playerFollowTarget;
        activeEdgeScrollBounds = null;
        currentMode = CameraMode.PlayerFollow;
    }

    private bool CanEdgeScroll()
    {
        return Mouse.current != null
            && Application.isFocused
            && (!blockWhenPointerOverUI
                || EventSystem.current == null
                || !EventSystem.current.IsPointerOverGameObject())
            && freeCameraTarget != null
            && activeEdgeScrollBounds != null;
    }

    private Vector2 GetEdgeScrollDirection(Vector2 pointerPosition)
    {
        if (pointerPosition.x < 0f || pointerPosition.x > Screen.width
            || pointerPosition.y < 0f || pointerPosition.y > Screen.height)
        {
            return Vector2.zero;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (pointerPosition.x <= edgeThickness) horizontal = -1f;
        else if (pointerPosition.x >= Screen.width - edgeThickness) horizontal = 1f;

        if (pointerPosition.y <= edgeThickness) vertical = -1f;
        else if (pointerPosition.y >= Screen.height - edgeThickness) vertical = 1f;

        return new Vector2(horizontal, vertical).normalized;
    }

    private void MoveFreeCameraTarget(Vector2 direction)
    {
        Camera outputCamera = Camera.main;
        Transform cameraTransform = outputCamera != null ? outputCamera.transform : transform;

        Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 movement = (right * direction.x + forward * direction.y) * moveSpeed * Time.deltaTime;

        freeCameraTarget.position = ClampToBounds(freeCameraTarget.position + movement);
    }

    private Vector3 ClampToBounds(Vector3 position)
    {
        Bounds bounds = activeEdgeScrollBounds.bounds;

        position.x = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
        position.z = Mathf.Clamp(position.z, bounds.min.z, bounds.max.z);

        return position;
    }

    private void EnsureFreeCameraTarget()
    {
        if (freeCameraTarget != null)
        {
            return;
        }

        GameObject targetObject = new GameObject("Runtime Free Camera Target");
        targetObject.hideFlags = HideFlags.HideInHierarchy;
        freeCameraTarget = targetObject.transform;
    }

    private bool HasCameraAndPlayerTarget()
    {
        if (cinemachineCamera != null && playerFollowTarget != null)
        {
            return true;
        }

        LogMissingReferenceOnce("Cinemachine Camera 또는 Player Follow Target이 연결되지 않았습니다.");
        return false;
    }

    private void LogMissingReferenceOnce(string message)
    {
        if (hasLoggedMissingReference)
        {
            return;
        }

        Debug.LogError($"[{nameof(CameraModeController)}] {message}", this);
        hasLoggedMissingReference = true;
    }

    private void HandleZoom()
    {
        if (positionComposer == null || Mouse.current == null)
        {
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        float scrollY = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scrollY) > 0.01f)
        {
            targetCameraDistance -= scrollY * zoomSpeed;

            targetCameraDistance = Mathf.Clamp(
                targetCameraDistance,
                minCameraDistance,
                maxCameraDistance
            );
        }

        positionComposer.CameraDistance = Mathf.SmoothDamp(
            positionComposer.CameraDistance,
            targetCameraDistance,
            ref zoomVelocity,
            zoomSmoothTime,
            Mathf.Infinity,
            Time.unscaledDeltaTime
        );
    }

    private void OnDestroy()
    {
        if (freeCameraTarget != null)
        {
            Destroy(freeCameraTarget.gameObject);
        }
    }
}

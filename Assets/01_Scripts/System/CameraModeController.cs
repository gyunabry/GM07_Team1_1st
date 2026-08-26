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
    [Header("References")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Transform playerFollowTarget;
    [SerializeField] private Collider edgeScrollBounds;

    [Header("Edge Scroll")]
    [SerializeField, Min(1f)] private float edgeThickness = 20f;
    [SerializeField, Min(0f)] private float moveSpeed = 12f;
    [SerializeField] private bool blockWhenPointerOverUI = false;
    [SerializeField] private CameraMode initialMode = CameraMode.PlayerFollow;

    private Transform freeCameraTarget;
    private CameraMode currentMode;
    private bool hasLoggedMissingReference;

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

        currentMode = CameraMode.PlayerFollow;
    }

    private void Start()
    {
        SetMode(initialMode);
    }

    private void Update()
    {
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
            if (edgeScrollBounds == null)
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

    public void FollowPlayer()
    {
        SetMode(CameraMode.PlayerFollow);
    }

    private bool CanEdgeScroll()
    {
        return Mouse.current != null
            && Application.isFocused
            && (!blockWhenPointerOverUI
                || EventSystem.current == null
                || !EventSystem.current.IsPointerOverGameObject())
            && freeCameraTarget != null
            && edgeScrollBounds != null;
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
        Bounds bounds = edgeScrollBounds.bounds;
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

    private void OnDestroy()
    {
        if (freeCameraTarget != null)
        {
            Destroy(freeCameraTarget.gameObject);
        }
    }
}

using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [Header("카메라 추적 대상")]
    [SerializeField] private Transform cameraTarget;

    [Header("플레이어 기본 설정")]
    [SerializeField] private float moveSpeed = 5.0f;

    private Camera mainCamera;

    private void Start()
    {
        // 카메라 트래킹 등록
        CinemachineCamera cam = FindFirstObjectByType<CinemachineCamera>();

        if (cam == null)
        {
            return;
        }

        Transform target = cameraTarget != null ? cameraTarget : transform;

        cam.Target.TrackingTarget = target;

        mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null) return;

        Vector2 input = Vector2.zero;

        if (keyboard.wKey.isPressed) input.y += 1;
        if (keyboard.sKey.isPressed) input.y -= 1;
        if (keyboard.aKey.isPressed) input.x -= 1;
        if (keyboard.dKey.isPressed) input.x += 1;

        Vector3 forward = mainCamera.transform.forward;
        Vector3 right = mainCamera.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 dir = forward * input.y + right * input.x;

        transform.position += dir.normalized * moveSpeed * Time.deltaTime;
    }
}

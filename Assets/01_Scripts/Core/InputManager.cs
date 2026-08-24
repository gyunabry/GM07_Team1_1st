using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/* 
우클릭 이동으로 수정

- UI 위 -> 월드에서 아무 동작도 하지 않음
- 건물 배치 모드 -> 배치만 시도
- 상호작용 가능한 대상 -> 선택
- 이동 가능한 지형 -> 플레이어 이동
- 모두 실패 -> return
 
 */

public class InputManager : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask placementLayer;
    [SerializeField] private Camera mainCamera;

    private Vector3 lastPos;

    public event Action OnClicked, OnExit, OnRotation;

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && !IsPointerOverUI())
        {
            OnClicked?.Invoke();
        }
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            OnExit?.Invoke();
        }
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            OnRotation?.Invoke();
        }
    }

    // 마우스가 UI 위에 있다면 false 반환
    public bool IsPointerOverUI()
        => EventSystem.current.IsPointerOverGameObject();

    public bool TryGetWorldPosition(out Vector3 worldPosition)
    {
        worldPosition = default;

        if (mainCamera == null || Mouse.current == null)
        {
            return false;
        }

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        worldPosition = hit.point;
        return true;
    }

    public bool TryGetPlacementHit(out Vector3 worldPosition, out Collider hitCollider)
    {
        worldPosition = default;
        hitCollider = null;

        if (mainCamera == null || Mouse.current == null)
        {
            return false;
        }

        Vector2 mousePos = Mouse.current.position.ReadValue();

        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, placementLayer, QueryTriggerInteraction.Ignore))
        {
            worldPosition = hit.point;
            hitCollider = hit.collider;

            return true;
        }

        worldPosition = default;
        hitCollider = null;

        return false;
    }
}

using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/*
플레이어 오브젝트에 부착되어 상호작용 가능한 오브젝트를 감지하는 클래스

 */

public class PlayerInteractionDetector : MonoBehaviour
{
    private readonly HashSet<IInteractable> detectedInteractables = new();

    public int DetectedCount => detectedInteractables.Count;

    private void OnTriggerEnter(Collider other)
    {
        IInteractable interactable = other.GetComponentInParent<IInteractable>();

        if (interactable == null) return;

        // 이미 등록된 대상이면 리턴
        if (!detectedInteractables.Add(interactable))
        {
            return;
        }

        // 강조 표시
        if (interactable is IHighlightable highlightable)
        {
            highlightable.SetHighlighted(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IInteractable interactable = other.GetComponentInParent<IInteractable>();

        if (interactable == null) return;

        // 이미 등록된 대상이 아니라면 리턴
        if (!detectedInteractables.Remove(interactable))
        {
            return;
        }

        // 강조 표시 해제
        if (interactable is IHighlightable highlightable)
        {
            highlightable.SetHighlighted(false);
        }
    }

    public bool Contains(IInteractable interactable)
    {
        return interactable != null && detectedInteractables.Contains(interactable);
    }
}

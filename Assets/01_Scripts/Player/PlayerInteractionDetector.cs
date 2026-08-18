using System;
using System.Collections.Generic;
using UnityEngine;

/*
플레이어 오브젝트에 부착되어 상호작용 가능한 오브젝트를 감지하는 클래스

 */

public class PlayerInteractionDetector : MonoBehaviour
{
    private readonly Dictionary<IInteractable, HashSet<Collider>> detectedInteractables = new();

    public event Action<IInteractable> InteractableExited;

    public int DetectedCount => detectedInteractables.Count;

    private void OnTriggerEnter(Collider other)
    {
        IInteractable interactable = other.GetComponentInParent<IInteractable>();

        if (interactable == null) return;

        // 이미 등록된 대상이면 리턴
        if (!detectedInteractables.TryGetValue(interactable, out HashSet<Collider> colliders))
        {
            colliders = new HashSet<Collider>();
            detectedInteractables.Add(interactable, colliders);
        }

        if (!colliders.Add(other)) return;

        if (colliders.Count == 1 && interactable is IHighlightable highlightable)
        {
            highlightable.SetHighlighted(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IInteractable interactable = other.GetComponentInParent<IInteractable>();

        if (interactable == null) return;

        // 이미 등록된 대상이 아니라면 리턴
        if (!detectedInteractables.TryGetValue(interactable, out HashSet<Collider> colliders))
        {
            return;
        }

        if (!colliders.Remove(other)) return;

        if (colliders.Count > 0) return;

        detectedInteractables.Remove(interactable);

        // 강조 표시 해제
        if (interactable is IHighlightable highlightable)
        {
            highlightable.SetHighlighted(false);
        }

        InteractableExited?.Invoke(interactable);
    }

    public bool Contains(IInteractable interactable)
    {
        return interactable != null && detectedInteractables.ContainsKey(interactable);
    }
}

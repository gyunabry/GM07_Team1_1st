using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IInteractable, IHighlightable
{
    [Header("상호작용")]
    [SerializeField] private string interactionName = "상호작용";
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private bool isEnabled = true;

    [Header("강조 표시")]
    [SerializeField] private Renderer[] highlightRenderers;
    [SerializeField] private Color highlightColor = Color.white;

    public string InteractionName => interactionName;

    public Transform InteractionPoint => interactionPoint != null ? interactionPoint : transform;

    public bool CanInteract(GameObject interactor)
    {
        return isEnabled && interactor != null;
    }

    // 해당 베이스를 상속받는 자식 클래스에서 구현
    public abstract void Interact(GameObject interactor);

    public void SetHighlighted(bool value)
    {

    }
}

using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [Header("상호작용")]
    [SerializeField] private string interactionName = "상호작용";
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float interactionRange = 1.5f;
    [SerializeField] private bool isEnabled = true;

    public string InteractionName => interactionName;

    public Transform InteractionPoint => interactionPoint != null ? interactionPoint : transform;

    public float InteractableRange => interactionRange;

    public bool CanInteract(GameObject interactor)
    {
        return isEnabled && interactor != null;
    }

    // 해당 베이스를 상속받는 자식 클래스에서 구현
    public abstract void Interact(GameObject interactor);

    public void SetInteractionEnabled(bool value)
    {
        isEnabled = value;
    }

    protected void SetInteractionName(string name)
    {
        interactionName = name;
    }
}

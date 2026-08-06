using UnityEngine;

/* 상호작용 가능한 오브젝트들이 구현할 인터페이스 */


public interface IInteractable
{
    // UI에 표시할 문구 (예: LMB. 상호 작용)
    string InteractionName { get; }
    Transform InteractionPoint { get; }

    bool CanInteract(GameObject interactor);
    void Interact(GameObject interactor);
}

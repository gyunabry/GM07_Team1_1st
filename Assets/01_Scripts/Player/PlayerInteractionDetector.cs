using UnityEngine;

/*
플레이어 오브젝트에 부착되어 상호작용 가능한 오브젝트를 감지하는 클래스

 */

public class PlayerInteractionDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            
        }
    }
}

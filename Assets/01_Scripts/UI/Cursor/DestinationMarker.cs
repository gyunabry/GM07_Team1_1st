using DG.Tweening;
using UnityEngine;

public class Marker : MonoBehaviour
{
    [SerializeField] Transform destinationMarker;

    private void OnEnable()
    {
        destinationMarker.DOKill();
        destinationMarker.transform.position = new Vector3(transform.position.x, 0.6f, transform.position.z);
        destinationMarker.DOMoveY(0.9f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    private void OnDisable()
    {
        destinationMarker.DOKill();
    }
}

using UnityEngine;

public class GetItem : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Dropitem di = other.GetComponent<Dropitem>();
        if (di == null) return;
        di.GetItem();
    }
}

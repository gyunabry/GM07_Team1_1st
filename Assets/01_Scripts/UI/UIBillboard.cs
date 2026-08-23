using UnityEngine;

public class UIBillboard : MonoBehaviour
{
    private Camera targetCamera;
    private void Awake()
    {
        targetCamera = Camera.main;
    }

    void Update()
    {
        if (targetCamera == null) return;

        transform.rotation = Quaternion.LookRotation(
            targetCamera.transform.forward,
            targetCamera.transform.up);
    }
}

using UnityEngine;
using UnityEngine.AI;

public class Portal : MonoBehaviour
{
    public LayerMask layer;
    private void OnTriggerEnter(Collider other)
    {
        if((layer & (1<<other.gameObject.layer)) != 0)
        {
            Transform[] target = GetComponentsInChildren<Transform>();
            other.transform.position = target[1].position;
            NavMeshAgent player = other.gameObject.GetComponent<NavMeshAgent>();
            if (player != null)
            {
                player.ResetPath();
            }
        }
    }
}

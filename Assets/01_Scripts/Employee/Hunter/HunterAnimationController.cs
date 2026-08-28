using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public sealed class HunterAnimationController : MonoBehaviour
{
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    [SerializeField] private GameObject visualPrefab;
    [SerializeField] private RuntimeAnimatorController animatorController;
    [SerializeField] private Vector3 visualScale = Vector3.one;
    [SerializeField, Min(0.1f)] private float attackRange = 2f;
    [SerializeField, Min(0.05f)] private float attackInterval = 2f;
    [SerializeField] private Transform legacyVisual;

    private Animator animator;
    private NavMeshAgent agent;
    private float nextAttackTime;
    private float nextTargetCheckTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (legacyVisual != null)
        {
            legacyVisual.gameObject.SetActive(false);
        }

        if (visualPrefab != null)
        {
            GameObject visual = Instantiate(visualPrefab, transform);
            visual.name = "HunterVisual";
            visual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            visual.transform.localScale = visualScale;
            animator = visual.GetComponent<Animator>();
        }
        else
        {
            animator = GetComponentInChildren<Animator>(true);
        }

        if (animator != null && animatorController != null)
        {
            animator.runtimeAnimatorController = animatorController;
        }
    }

    private void Update()
    {
        if (animator == null || Time.time < nextAttackTime || Time.time < nextTargetCheckTime || agent == null || !agent.isOnNavMesh || !agent.isStopped)
        {
            return;
        }

        nextTargetCheckTime = Time.time + 0.1f;
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < enemies.Length; i++)
        {
            Enemy enemy = enemies[i];
            if (enemy != null && enemy.CurrentHp > 0f && (enemy.transform.position - transform.position).sqrMagnitude <= attackRange * attackRange)
            {
                animator.SetTrigger(AttackHash);
                nextAttackTime = Time.time + attackInterval;
                return;
            }
        }
    }
}

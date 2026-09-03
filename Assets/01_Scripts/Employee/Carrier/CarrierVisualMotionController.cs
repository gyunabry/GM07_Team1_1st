using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 리그가 없는 운반 직원 비주얼에 부유와 이동 기울기 연출을 적용합니다.
/// NavMeshAgent가 제어하는 루트 Transform은 변경하지 않습니다.
/// </summary>
[DisallowMultipleComponent]
public sealed class CarrierVisualMotionController : MonoBehaviour
{
    [SerializeField, Min(0f)] private float idleBobHeight = 0.16f;
    [SerializeField, Min(0f)] private float idleBobFrequency = 2f;
    [SerializeField, Range(0f, 45f)] private float idleSwayAngle = 10f;
    [SerializeField, Min(0f)] private float moveBobHeight = 0.26f;
    [SerializeField, Min(0f)] private float moveBobFrequency = 4f;
    [SerializeField, Range(0f, 45f)] private float moveTiltAngle = 25f;
    [SerializeField, Min(0.01f)] private float smoothingSpeed = 8f;

    private NavMeshAgent agent;
    private Transform visualRoot;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private float phaseOffset;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        visualRoot = FindVisualRoot();
        if (visualRoot == null)
        {
            enabled = false;
            return;
        }

        initialLocalPosition = visualRoot.localPosition;
        initialLocalRotation = visualRoot.localRotation;
        phaseOffset = Random.value * Mathf.PI * 2f;
    }

    private void OnDisable()
    {
        ResetVisual();
    }

    private void Update()
    {
        if (visualRoot == null)
        {
            return;
        }

        Vector3 velocity = agent != null && agent.enabled ? agent.velocity : Vector3.zero;
        bool isMoving = velocity.sqrMagnitude > 0.01f;
        float bobHeight = isMoving ? moveBobHeight : idleBobHeight;
        float bobFrequency = isMoving ? moveBobFrequency : idleBobFrequency;
        float bob = Mathf.Sin(Time.time * bobFrequency + phaseOffset) * bobHeight;

        visualRoot.localPosition = initialLocalPosition + Vector3.up * bob;

        Vector3 localVelocity = transform.InverseTransformDirection(velocity.normalized);
        Quaternion tilt = isMoving
            ? Quaternion.Euler(-localVelocity.z * moveTiltAngle, 0f, localVelocity.x * moveTiltAngle)
            : Quaternion.Euler(0f, Mathf.Sin(Time.time * idleBobFrequency + phaseOffset) * idleSwayAngle, 0f);
        visualRoot.localRotation = Quaternion.Slerp(
            visualRoot.localRotation,
            initialLocalRotation * tilt,
            smoothingSpeed * Time.deltaTime);
    }

    private Transform FindVisualRoot()
    {
        ParticleSystem particleSystem = GetComponentInChildren<ParticleSystem>(true);
        if (particleSystem == null)
        {
            return null;
        }

        Transform root = particleSystem.transform;
        while (root.parent != null && root.parent != transform)
        {
            root = root.parent;
        }

        return root.parent == transform ? root : null;
    }

    private void ResetVisual()
    {
        if (visualRoot == null)
        {
            return;
        }

        visualRoot.localPosition = initialLocalPosition;
        visualRoot.localRotation = initialLocalRotation;
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

//Agent의 NavMeshLink 진입 여부 확인, Start에서 End까지 포물선 이동 후, 종료 알림
public class NavMeshJump : MonoBehaviour
{
    [SerializeField] private float jumpDuration = 0.6f;
    [SerializeField] private float jumpHeight = 1.5f;

    private NavMeshAgent agent;
    private bool isTraversing;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoTraverseOffMeshLink = false;
    }

    private void Update()
    {
        if (isTraversing) return;
        if(agent.isOnOffMeshLink)
        {
            StartCoroutine(TraverseLinkCo());
        }
    }

    private IEnumerator TraverseLinkCo()
    {
        isTraversing = true;
        OffMeshLinkData linkData = agent.currentOffMeshLinkData;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = linkData.endPos + Vector3.up * agent.baseOffset;

        agent.isStopped = true;
        agent.updatePosition = false;
        float elapsedTime = 0.0f;

        while (elapsedTime < jumpDuration)
        {
            elapsedTime += Time.deltaTime;
            float ratio = Mathf.Clamp01(elapsedTime / jumpDuration);
            Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, ratio);
            float heightOffset = 4.0f * jumpHeight * ratio * (1.0f - ratio);
            currentPosition.y += heightOffset;
            transform.position = currentPosition;

            yield return null;
        }
        transform.position = endPosition;
        agent.nextPosition = endPosition;

        agent.CompleteOffMeshLink();
        isTraversing = false;
        agent.isStopped = false;
        agent.updatePosition = true;
    }
}

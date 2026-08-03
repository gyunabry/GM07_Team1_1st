using UnityEngine;
using UnityEngine.AI;

//이동경로 시각화.
//완성 후 해당 기능을 사용하지 않을 시 Agent - Line Renderer Material - Temp 삭제.
public class NavMeshPathVisualizer : MonoBehaviour
{
    [SerializeField] private float lineHeight = 0.15f;

    private NavMeshAgent agent;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 0;
    }

    private void Update()
    {
        if (agent.pathPending || !agent.hasPath)
        {
            lineRenderer.positionCount = 0;
            return;
        }
        Vector3[] corners = agent.path.corners;
        lineRenderer.positionCount = corners.Length;

        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 linePosition = corners[i] + Vector3.up * lineHeight;
            lineRenderer.SetPosition(i, linePosition);
        }
    }
}

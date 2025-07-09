using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathVisualizer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    private List<NavNode> currentPath = new List<NavNode>();
    public Transform userTracker;



    void Start()
    {
        ClearPath();
    }

    private void Awake()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.useWorldSpace = false; //  Enable local space
    }


    public void ShowPathWithUserStart(List<NavNode> path, Vector3 userStart)
    {
        if (path == null || path.Count == 0)
        {
            ClearPath();
            return;
        }

        currentPath = path;

        // Convert world to local position based on the parent (map)
        Transform mapTransform = transform;

        lineRenderer.positionCount = path.Count + 1;
        lineRenderer.SetPosition(0, mapTransform.InverseTransformPoint(userStart + Vector3.up * 0.005f));

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 worldPos = path[i].transform.position + Vector3.up * 0.005f;
            lineRenderer.SetPosition(i + 1, mapTransform.InverseTransformPoint(worldPos));
        }
    }



    private void Update()
    {
        if (currentPath != null && currentPath.Count > 0)
        {
            UpdateLine();
        }
    }

    private void UpdateLine()
    {
        if (lineRenderer.positionCount != currentPath.Count + 1)
            lineRenderer.positionCount = currentPath.Count + 1;

        Transform mapTransform = transform;

        // Keep index 0 = user's world position
        Vector3 userStart = userTracker.transform.position + Vector3.up * 0.005f;
        lineRenderer.SetPosition(0, mapTransform.InverseTransformPoint(userStart));

        for (int i = 0; i < currentPath.Count; i++)
        {
            Vector3 worldPos = currentPath[i].transform.position + Vector3.up * 0.005f;
            lineRenderer.SetPosition(i + 1, mapTransform.InverseTransformPoint(worldPos));
        }
    }



    public void ClearPath()
    {
        currentPath.Clear();
        lineRenderer.positionCount = 0;
    }
}

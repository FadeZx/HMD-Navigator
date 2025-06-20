using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathVisualizer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    private List<NavNode> currentPath = new List<NavNode>();

    private void Awake()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
    }

    public void ShowPath(List<NavNode> path)
    {
        if (path == null || path.Count == 0)
        {
            ClearPath();
            return;
        }

        currentPath = path;
        lineRenderer.positionCount = currentPath.Count;

        UpdateLine(); // Set initial line positions
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
        for (int i = 0; i < currentPath.Count; i++)
        {
            lineRenderer.SetPosition(i, currentPath[i].transform.position + Vector3.up * 0.005f);
        }
    }

    public void ClearPath()
    {
        currentPath.Clear();
        lineRenderer.positionCount = 0;
    }
}

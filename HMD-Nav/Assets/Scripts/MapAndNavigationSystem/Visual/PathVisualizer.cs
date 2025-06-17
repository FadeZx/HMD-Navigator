using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathVisualizer : MonoBehaviour
{
    public LineRenderer lineRenderer;

    public void ShowPath(List<NavNode> path)
    {
        if (path == null || path.Count == 0) return;

        lineRenderer.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            lineRenderer.SetPosition(i, path[i].transform.position + Vector3.up * 0.1f); // Slightly above ground
        }
    }

    public void ClearPath()
    {
        lineRenderer.positionCount = 0;
    }
}

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class UserNavigationVisualizer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform userOrigin; // This should point to the user’s XR rig or camera root
    public float verticalOffset = 0.05f;
    public float worldScaleMultiplier = 2.0f; // Scale from map space to real space

    private List<Vector3> worldPoints = new List<Vector3>();

    private void Awake()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.useWorldSpace = true;
        lineRenderer.enabled = false;
    }


    public void ShowWorldPath(List<NavNode> path, Vector3 userWorldPosInMap, Vector3 worldForward)
    {
        if (path == null || path.Count < 2)
        {
            ClearPath();
            return;
        }

        Vector3 mapForward = path.Count >= 2
            ? (path[1].transform.position - path[0].transform.position).normalized
            : Vector3.forward;

        mapForward.y = 0;
        worldForward.y = 0;

        // ✅ Declare alignRotation before using it
        Quaternion alignRotation;

        if (mapForward.sqrMagnitude < 0.001f || worldForward.sqrMagnitude < 0.001f)
        {
            Debug.LogWarning("[UserNavigationVisualizer] Invalid forward vectors for rotation alignment.");
            alignRotation = Quaternion.identity;
        }
        else
        {
            // ✅ Rotate the world to match the map
            alignRotation = Quaternion.FromToRotation(worldForward.normalized, mapForward.normalized);

        }

        Vector3 mapToWorldOffset = userOrigin.position - userWorldPosInMap;

        if (!lineRenderer.enabled)
            lineRenderer.enabled = true;

        worldPoints.Clear();

        Vector3 basePos = new Vector3(userOrigin.position.x, 0f, userOrigin.position.z);
        Vector3 userWorldPoint = basePos + Vector3.up * verticalOffset;
        worldPoints.Add(userWorldPoint);

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 mapPos = path[i].transform.position - userWorldPosInMap;
            Vector3 rotated = alignRotation * mapPos * worldScaleMultiplier;
            Vector3 worldPos = basePos + rotated + Vector3.up * verticalOffset;
            worldPoints.Add(worldPos);
        }

        lineRenderer.positionCount = worldPoints.Count;
        lineRenderer.SetPositions(worldPoints.ToArray());
    }

    public void ClearPath()
    {
        worldPoints.Clear();
        lineRenderer.positionCount = 0;
    }
}

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

        // Establish offset and direction to align path with real-world user space
        Vector3 mapForward = path[1].transform.position - path[0].transform.position;
        mapForward.y = 0;
        worldForward.y = 0;

        Quaternion alignRotation = Quaternion.FromToRotation(mapForward.normalized, worldForward.normalized);
        Vector3 mapToWorldOffset = userOrigin.position - userWorldPosInMap;

        if (!lineRenderer.enabled)
            lineRenderer.enabled = true;
        worldPoints.Clear();
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 mapPos = path[i].transform.position - userWorldPosInMap; // relative to user in map
            Vector3 rotated = alignRotation * mapPos * worldScaleMultiplier;
            Vector3 basePos = new Vector3(userOrigin.position.x, 0f, userOrigin.position.z); // Ground level
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

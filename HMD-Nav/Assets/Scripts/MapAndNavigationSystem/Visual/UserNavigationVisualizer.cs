using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class UserNavigationVisualizer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform userOrigin; // This should point to the user’s XR rig or camera root
    public float verticalOffset = 0.05f;
    public float worldScaleMultiplier = 2.0f; // Scale from map space to real space
    [Range(-180f, 180f)]
    public float manualRotationOffset = 0f; // Degrees to rotate the whole path around the user
    private Quaternion lockedMapToWorldRotation;
    private bool hasLockedRotation = false;


    private List<Vector3> worldPoints = new List<Vector3>();

    private void Awake()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.useWorldSpace = true;
        lineRenderer.enabled = false;
    }
    public void LockRotation(Vector3 mapForward, Vector3 xrForward)
    {
        mapForward.y = 0f;
        xrForward.y = 0f;

        if (mapForward.sqrMagnitude < 0.001f || xrForward.sqrMagnitude < 0.001f)
        {
            Debug.LogWarning("Invalid vectors passed to LockRotation.");
            return;
        }

        lockedMapToWorldRotation = Quaternion.FromToRotation(mapForward.normalized, xrForward.normalized);
        hasLockedRotation = true;
    }


    public void ShowWorldPath(List<NavNode> path, Vector3 userWorldPosInMap, Vector3 userNodeWorldPos)

    {
        if (!hasLockedRotation)
        {
            Debug.LogWarning("Cannot show path: rotation not locked.");
            return;
        }

        if (path == null || path.Count < 2)
        {
            ClearPath();
            return;
        }

        Vector3 mapOrigin = path[0].transform.position;
        Vector3 baseWorldPos = userNodeWorldPos - new Vector3(0f, verticalOffset, 0f);
        float floorY = baseWorldPos.y;

        worldPoints.Clear();

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 offsetInMap = path[i].transform.position - mapOrigin;
            Vector3 rotatedOffset = lockedMapToWorldRotation * offsetInMap;
            Vector3 worldOffset = rotatedOffset * worldScaleMultiplier;

            Vector3 finalWorldPos = baseWorldPos + worldOffset;
            finalWorldPos.y = floorY;

            worldPoints.Add(finalWorldPos);
        }

        lineRenderer.positionCount = worldPoints.Count;
        lineRenderer.SetPositions(worldPoints.ToArray());
        lineRenderer.enabled = true;
    }



    public void ClearPath()
    {
        worldPoints.Clear();
        lineRenderer.positionCount = 0;
    }
}

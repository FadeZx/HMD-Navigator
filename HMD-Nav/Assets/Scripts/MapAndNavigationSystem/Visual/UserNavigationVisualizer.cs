using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class UserNavigationVisualizer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform userOrigin; // This should point to the user’s XR rig or camera root
    public float verticalOffset = 0.05f;
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
    public void LockRotation(Quaternion mapToWorldRotation)
    {
        lockedMapToWorldRotation = mapToWorldRotation;
        hasLockedRotation = true;
    }

    public Transform mapTransform; // Assign this in inspector or externally

    public void ShowWorldPath(List<NavNode> path, Vector3 mapToWorldOffset, NavGraphManager navGraph)
    {
        if (path == null || path.Count < 2 || mapTransform == null)
        {
            ClearPath();
            return;
        }

        float mapUnitsPerMeter = NavConfig.Instance.mapUnitsPerMeter;

        // Step 1: Direction from path[0] to path[1] in map local
        Vector3 firstMapLocal = mapTransform.InverseTransformPoint(path[0].transform.position);
        Vector3 secondMapLocal = mapTransform.InverseTransformPoint(path[1].transform.position);
        Vector3 localPathDir = (secondMapLocal - firstMapLocal).normalized;

        // Step 2: XR rig facing (flat)
        Vector3 rigForwardFlat = new Vector3(userOrigin.forward.x, 0f, userOrigin.forward.z).normalized;
        Vector3 pathForwardFlat = new Vector3(localPathDir.x, 0f, localPathDir.z).normalized;

        float signedAngle = Vector3.SignedAngle(pathForwardFlat, rigForwardFlat, Vector3.up);
        Quaternion rotationToRig = Quaternion.AngleAxis(signedAngle, Vector3.up);
        Quaternion finalRotation = hasLockedRotation ? rotationToRig * lockedMapToWorldRotation : rotationToRig;

        Debug.Log($"🔄 [WorldPath] Applying rotation: {signedAngle:F1}°, scale: 1/{mapUnitsPerMeter:F2}");

        float floorY = userOrigin.position.y - verticalOffset;
        worldPoints.Clear();

        // ✅ Step 4: Convert and add path[0] to path[n]
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 nodeLocal = mapTransform.InverseTransformPoint(path[i].transform.position);
            Vector3 offset = nodeLocal - firstMapLocal;
            Vector3 rotatedOffset = finalRotation * offset;
            Vector3 worldPos = mapToWorldOffset + rotatedOffset / mapUnitsPerMeter;
            worldPos.y = floorY;
            worldPoints.Add(worldPos);
        }

        // ✅ Step 5: Apply to LineRenderer
        lineRenderer.positionCount = worldPoints.Count;
        lineRenderer.SetPositions(worldPoints.ToArray());
        lineRenderer.enabled = true;

        // ✅ Debug
        foreach (Vector3 p in GetWorldPathPoints())
            Debug.DrawRay(p, Vector3.up * 0.2f, Color.cyan, 2f);

        float totalWorldLength = 0f;
        for (int i = 1; i < worldPoints.Count; i++)
        {
            float segmentLength = Vector3.Distance(worldPoints[i - 1], worldPoints[i]);
            Debug.Log($"🔹 Segment {i}: {segmentLength:F2}m");
            totalWorldLength += segmentLength;
        }

        float expectedFromWeights = navGraph.GetPathWeight(path) / mapUnitsPerMeter;
        Debug.Log($"📏 [World Path] Total length: {totalWorldLength:F2}m | Expected from map weight: {expectedFromWeights:F2}m");
    }




    public List<Vector3> GetWorldPathPoints()
{
    return worldPoints != null ? new List<Vector3>(worldPoints) : new List<Vector3>();
}



    public void ShowSingleEdge(Vector3 userWorldPos, Vector3 nextNodeWorldPos)
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, userWorldPos + Vector3.up * verticalOffset);
        lineRenderer.SetPosition(1, nextNodeWorldPos + Vector3.up * verticalOffset);
        lineRenderer.enabled = true;
    }



    public void ClearPath()
    {
        worldPoints.Clear();
        lineRenderer.positionCount = 0;
    }
}

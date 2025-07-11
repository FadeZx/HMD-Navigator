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
    public void LockRotation(Quaternion mapToWorldRotation)
    {
        lockedMapToWorldRotation = mapToWorldRotation;
        hasLockedRotation = true;
    }

    public Transform mapTransform; // Assign this in inspector or externally

    public void ShowWorldPath(List<NavNode> path, Vector3 xrRigWorldPosition)
    {
        if (path == null || path.Count < 2 || mapTransform == null)
        {
            ClearPath();
            return;
        }

        Vector3 baseWorldPos = xrRigWorldPosition;
        baseWorldPos.y -= verticalOffset;
        float floorY = baseWorldPos.y;
        worldPoints.Clear();

        Vector3 firstLocalPos = mapTransform.InverseTransformPoint(path[0].transform.position);
        Vector3 secondLocalPos = mapTransform.InverseTransformPoint(path[1].transform.position);
        Vector3 localPathDir = (secondLocalPos - firstLocalPos).normalized;

        Vector3 rigForwardFlat = new Vector3(userOrigin.forward.x, 0f, userOrigin.forward.z).normalized;
        Vector3 baseForward = new Vector3(localPathDir.x, 0f, localPathDir.z).normalized;

        float signedAngle = Vector3.SignedAngle(baseForward, rigForwardFlat, Vector3.up);
        Quaternion rotationToRig = Quaternion.AngleAxis(signedAngle, Vector3.up);

        Quaternion finalRotation = hasLockedRotation
            ? rotationToRig * lockedMapToWorldRotation
            : rotationToRig;

        foreach (var node in path)
        {
            Vector3 nodeLocal = mapTransform.InverseTransformPoint(node.transform.position);
            Vector3 offset = nodeLocal - firstLocalPos;

            Vector3 rotatedOffset = finalRotation * offset;

            Vector3 worldPos = baseWorldPos + rotatedOffset * worldScaleMultiplier;
            worldPos.y = floorY;

            worldPoints.Add(worldPos);
        }

        lineRenderer.positionCount = worldPoints.Count;
        lineRenderer.SetPositions(worldPoints.ToArray());
        lineRenderer.enabled = true;

        Debug.Log($"✅ World path shown with locked rotation + XR alignment: {signedAngle:F1}°");
    }




    public void ClearPath()
    {
        worldPoints.Clear();
        lineRenderer.positionCount = 0;
    }
}

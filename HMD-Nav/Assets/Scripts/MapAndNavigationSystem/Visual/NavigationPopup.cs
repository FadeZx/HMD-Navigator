using Oculus.Platform.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NavigationPopup : MonoBehaviour
{
    public NavGraphManager navGraph;
    public UserLocationTracker userTracker;
    public PathVisualizer visualizer;
    public Transform mapRootTransform; // The rotating map (userNode's parent)

    private NavNode pendingDestination;
    public NavNode initDest;

    [Header("Path Calculation")]
    private float walkSpeedMetersPerSecond;  // You can tune this in Inspector
    private float mapUnitsPerMeter;

    public UserNavigationVisualizer userVisualizer; // ✅ Path visualizer in world space

    private IEnumerator InitNavigationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetDestination(initDest);
        ConfirmNavigation();
    }

    // ✅ Use XR rig forward in world space (not userNode forward)
    private float GetAngleOffsetBetweenXRRigAndMapPath(List<NavNode> path)
    {
        if (path == null || path.Count < 2) return 0f;

        Vector3 xrRigForward = userTracker.userTransform.forward;
        Vector3 pathDir = (path[1].transform.position - path[0].transform.position).normalized;

        xrRigForward.y = 0f;
        pathDir.y = 0f;

        return Vector3.SignedAngle(xrRigForward, pathDir, Vector3.up);
    }



    public void SetDestination(NavNode destination)
    {
        mapUnitsPerMeter = NavConfig.Instance.mapUnitsPerMeter;
        walkSpeedMetersPerSecond = NavConfig.Instance.walkSpeed;
        pendingDestination = destination;

        Vector3 userWorldPos = userTracker.transform.position;
        NavNode nearestNode = navGraph.FindNearestNode(userWorldPos);

        if (nearestNode == null || destination == null)
        {
            Debug.LogError("[NavigationPopup] Missing destination or nearest node.");
            return;
        }

        List<NavNode> fullPath = navGraph.FindPath(nearestNode, destination);
        float graphPathWeight = navGraph.GetPathWeight(fullPath);
        float userToGraphDist = Vector3.Distance(userWorldPos, nearestNode.transform.position);
        float scaledUserDist = userToGraphDist * mapUnitsPerMeter;
        float totalWeight = graphPathWeight + scaledUserDist;

        // Map path
        visualizer.ShowPathWithUserStart(fullPath, userWorldPos);

        // World path
        if (userVisualizer != null)
        {
            userVisualizer.worldScaleMultiplier = 1f / mapUnitsPerMeter;

            float signedAngle = GetAngleOffsetBetweenXRRigAndMapPath(fullPath);

            Quaternion rotationOffset = Quaternion.AngleAxis(signedAngle, Vector3.up);
            userVisualizer.LockRotation(rotationOffset);

            Debug.Log($"📐 [Angle] Applied signed angle offset from XR Rig to path: {signedAngle:F1}°");

            Vector3 userNodeWorldPos = userTracker.transform.position;

            userVisualizer.ShowWorldPath(
     fullPath,
     xrRigWorldPosition: userTracker.userTransform.position
 );


            Debug.Log($"🧭 [DEBUG] Angle between XR Rig forward and path: {GetAngleOffsetBetweenXRRigAndMapPath(fullPath):F1}°");
        }

        float meters = totalWeight / mapUnitsPerMeter;
        float seconds = meters / walkSpeedMetersPerSecond;
        int min = Mathf.FloorToInt(seconds / 60f);
        int sec = Mathf.FloorToInt(seconds % 60f);

        Debug.Log($"[NavigationPopup] Distance: {meters:F1} meters, Time: {min}m {sec}s");
    }

    public void ConfirmNavigation()
    {
        if (pendingDestination == null)
        {
            Debug.LogWarning("[NavigationPopup] No destination was set before confirmation.");
            return;
        }

        Debug.Log($"[NavigationPopup] Confirmed navigation to {pendingDestination.name}");

        Vector3 userWorldPos = userTracker.transform.position;
        NavNode startNode = navGraph.FindNearestNode(userWorldPos);

        if (startNode == null)
        {
            Debug.LogError("[NavigationPopup] Nearest node not found.");
            return;
        }

        List<NavNode> fullPath = navGraph.FindPath(startNode, pendingDestination);

        if (userVisualizer != null)
        {
            userVisualizer.worldScaleMultiplier = 1f / mapUnitsPerMeter;

            float signedAngle = GetAngleOffsetBetweenXRRigAndMapPath(fullPath);
            Quaternion rotationOffset = Quaternion.AngleAxis(signedAngle, Vector3.up);
            userVisualizer.LockRotation(rotationOffset);

            Debug.Log($"📐 [Angle] Applied signed angle offset from XR Rig to path: {signedAngle:F1}°");

            Vector3 userNodeWorldPos = userTracker.transform.position;

            userVisualizer.ShowWorldPath(
      fullPath,
      xrRigWorldPosition: userTracker.userTransform.position
  );


            Debug.Log($"🧭 [DEBUG] Angle between XR Rig forward and path: {GetAngleOffsetBetweenXRRigAndMapPath(fullPath):F1}°");
        }

        MapController mapController = FindAnyObjectByType<MapController>(FindObjectsInactive.Include);
        if (mapController != null)
            mapController.ToggleMap();
    }



    public void Cancel()
    {
        visualizer.ClearPath();
        userVisualizer?.ClearPath();
        pendingDestination = null;
    }
}

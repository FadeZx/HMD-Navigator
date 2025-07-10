using Oculus.Platform.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationPopup : MonoBehaviour
{
    public NavGraphManager navGraph;
    public UserLocationTracker userTracker;
    public PathVisualizer visualizer;
    public Transform mapRootTransform; // The rotating map (userNode's parent)

    private NavNode pendingDestination;
    public NavNode initDest;

    [Header("Path Calculation ")]
    private float walkSpeedMetersPerSecond;  // You can tune this in Inspector
    private float mapUnitsPerMeter;


    // public GameObject overlayArrow;  ❌ remove this
    public UserNavigationVisualizer userVisualizer; // ✅ Add this instead

    private void Start()
    {
       


    }

    private IEnumerator InitNavigationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetDestination(initDest);
        ConfirmNavigation(); // Optional follow-up action
    }

    /// <summary>
    /// Sets the destination and immediately shows the path from user to destination
    /// </summary>
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

        // Visualize on map
        visualizer.ShowPathWithUserStart(fullPath, userWorldPos);

        // Visualize in world
        if (userVisualizer != null)
        {
            userVisualizer.worldScaleMultiplier = 1f / mapUnitsPerMeter;

            // Get raw world forward
            Vector3 userWorldForward = userTracker.userTransform.forward;

            // Cancel out map rotation to get userNode.forward in map space
            Quaternion worldToMap = Quaternion.Inverse(mapRootTransform.rotation);
            Vector3 userForwardInMapSpace = worldToMap * userWorldForward.normalized;

            // Then: align this with world forward
            userVisualizer.LockRotation(userForwardInMapSpace, userWorldForward);

            // 🛠️ FIXED: define this here
            Vector3 userNodeWorldPos = userTracker.transform.position;

            userVisualizer.ShowWorldPath(
                fullPath,
                userWorldPosInMap: userWorldPos,
                userNodeWorldPos: userNodeWorldPos);
        }


        float meters = totalWeight / mapUnitsPerMeter;
        float seconds = meters / walkSpeedMetersPerSecond;
        int min = Mathf.FloorToInt(seconds / 60f);
        int sec = Mathf.FloorToInt(seconds % 60f);

        Debug.Log($"[NavigationPopup] Distance: {meters:F1} meters, Time: {min}m {sec}s");
    }




    /// <summary>
    /// Called after path is shown — to do more things (e.g., enable AR arrow, UI messages, etc.)
    /// </summary>
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

        // Visualize in world
        // Visualize in world
        if (userVisualizer != null)
        {
            userVisualizer.worldScaleMultiplier = 1f / mapUnitsPerMeter;

            // Get raw world forward
            Vector3 userWorldForward = userTracker.userTransform.forward;

            // Cancel out map rotation to get userNode.forward in map space
            Quaternion worldToMap = Quaternion.Inverse(mapRootTransform.rotation);
            Vector3 userForwardInMapSpace = worldToMap * userWorldForward.normalized;

            // Then: align this with world forward
            userVisualizer.LockRotation(userForwardInMapSpace, userWorldForward);

            // 🛠️ FIXED: define this here
            Vector3 userNodeWorldPos = userTracker.transform.position;

            userVisualizer.ShowWorldPath(
                fullPath,
                userWorldPosInMap: userWorldPos,
                userNodeWorldPos: userNodeWorldPos);
        }




        // ✅ Close map
        MapController mapController = FindAnyObjectByType<MapController>(FindObjectsInactive.Include);
        if (mapController != null)
            mapController.ToggleMap();
    }




    public void Cancel()
    {
        visualizer.ClearPath();       // ✅ Clears the map path
        userVisualizer?.ClearPath();  // ✅ Also clear the real-world path if it exists
        pendingDestination = null;
    }


}

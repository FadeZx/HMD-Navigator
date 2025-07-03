using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationPopup : MonoBehaviour
{
    public NavGraphManager navGraph;
    public UserLocationTracker userTracker;
    public PathVisualizer visualizer;

    private NavNode pendingDestination;
    float pathWeight = 0f;
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


        float scale = NavConfig.Instance.mapUnitsPerMeter;

        // Calculate scaled distance from user to nearest node
        float unityDistance = Vector3.Distance(userWorldPos, nearestNode.transform.position);
        float scaledWeight = unityDistance * scale;

        // Create virtual node (only for pathfinding)
        NavNode virtualStart = new GameObject("VirtualStartNode").AddComponent<NavNode>();
        virtualStart.transform.position = userWorldPos;
        virtualStart.nodeID = "UserPosition";
        virtualStart.manualConnections.Clear();
        virtualStart.connections.Clear();

        var edgeToNearest = new NavEdge(nearestNode, scaledWeight);
        virtualStart.manualConnections.Add(edgeToNearest);
        virtualStart.UseManualConnections();

        navGraph.allNodes.Add(virtualStart);

        // Find path (including user's real position)
        // Find path (including user's real position)
        List<NavNode> fullPath = navGraph.FindPath(virtualStart, destination);

        // ✅ Always manually add user-to-first-node distance (scaledWeight)
        float totalWeight = navGraph.GetPathWeight(fullPath) + scaledWeight;

        // ✅ Insert virtualStart at the front for visualization
        fullPath.Insert(0, virtualStart);

        // Convert to meters and time
        float meters = totalWeight / mapUnitsPerMeter;
        float seconds = meters / walkSpeedMetersPerSecond;
        int min = Mathf.FloorToInt(seconds / 60f);
        int sec = Mathf.FloorToInt(seconds % 60f);

        // Visualize path starting at user position
        visualizer.ShowPathWithUserStart(fullPath, userWorldPos);

        // Cleanup virtual node
        navGraph.allNodes.Remove(virtualStart);
        Destroy(virtualStart.gameObject);


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

        // ✅ Redo the path to get final data
        Vector3 userWorldPos = userTracker.transform.position;
        NavNode nearestNode = navGraph.FindNearestNode(userWorldPos);
        float unityDistance = Vector3.Distance(userWorldPos, nearestNode.transform.position);
        mapUnitsPerMeter = NavConfig.Instance.mapUnitsPerMeter;
        float scaledWeight = unityDistance * mapUnitsPerMeter;

        // Recreate virtual node
        NavNode virtualStart = new GameObject("VirtualStartNode").AddComponent<NavNode>();
        virtualStart.transform.position = userWorldPos;
        virtualStart.nodeID = "UserPosition";
        virtualStart.manualConnections.Clear();
        virtualStart.connections.Clear();
        var edgeToNearest = new NavEdge(nearestNode, scaledWeight);
        virtualStart.manualConnections.Add(edgeToNearest);
        virtualStart.UseManualConnections();
        navGraph.allNodes.Add(virtualStart);

        List<NavNode> fullPath = navGraph.FindPath(virtualStart, pendingDestination);
        fullPath.Insert(0, virtualStart); // Insert virtual start

        // ✅ Draw real-world path using userVisualizer
        if (userVisualizer != null)
        {
            userVisualizer.worldScaleMultiplier = 1f / mapUnitsPerMeter; // 🔁 Auto set scale
            Vector3 userNodeForward = userTracker.GetCurrentNode().transform.forward;
            userVisualizer.worldScaleMultiplier = 1f / mapUnitsPerMeter; // keep this
            userVisualizer.ShowWorldPath(fullPath, userWorldPos, userNodeForward);


        }



        navGraph.allNodes.Remove(virtualStart);
        Destroy(virtualStart.gameObject);

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

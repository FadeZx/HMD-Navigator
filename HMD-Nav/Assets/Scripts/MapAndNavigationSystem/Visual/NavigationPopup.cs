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
    public Transform nodeRoot; // ✅ Assign this in the inspector (e.g., a GameObject called "NodeRoot" under map)

    private NavNode pendingDestination;
    public NavNode initDest;

    [Header("Path Calculation")]
    private float walkSpeedMetersPerSecond;  // You can tune this in Inspector
    private float mapUnitsPerMeter;

    public UserNavigationVisualizer userVisualizer; // ✅ Path visualizer in world space
    public NavigationUpdater navigationUpdater;

    private List<NavNode> currentWorldPath = new List<NavNode>();


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

        pendingDestination = destination;
        // ✅ FIX: Ensure these are assigned
        mapUnitsPerMeter = NavConfig.Instance.mapUnitsPerMeter;
        walkSpeedMetersPerSecond = NavConfig.Instance.walkSpeed;

        Vector3 userWorldPos = userTracker.transform.position;
        NavNode startNode = navGraph.FindNearestNode(userWorldPos);

        if (startNode == null || destination == null)
        {
            Debug.LogError("[NavigationPopup] Missing destination or nearest node.");
            return;
        }

        List<NavNode> fullPath = navGraph.FindPath(startNode, destination);
        // ✅ Create a virtual NavNode for the user position
        GameObject virtualStartGO = new GameObject("UserVirtualNode");
        virtualStartGO.transform.position = userWorldPos;
        virtualStartGO.transform.SetParent(nodeRoot != null ? nodeRoot : mapRootTransform);


        NavNode virtualStartNode = virtualStartGO.AddComponent<NavNode>();
        virtualStartNode.nodeID = "-1"; // ✅ Fix: assign string, not int
                                     
        
        // ✅ Insert the virtual node at the beginning
        fullPath.Insert(0, virtualStartNode);
        // Map path
        visualizer.ShowPathWithoutUser(fullPath);

        // World path
        // World path
        if (userVisualizer != null)
        {
            float signedAngle = GetAngleOffsetBetweenXRRigAndMapPath(fullPath);
            Quaternion rotationOffset = Quaternion.AngleAxis(signedAngle, Vector3.up);
            userVisualizer.LockRotation(rotationOffset);

            Debug.Log($"📐 [Angle] Applied signed angle offset from XR Rig to path: {signedAngle:F1}°");

            // ✅ Compute the world offset from map space origin
            Vector3 userNodeLocal = mapRootTransform.InverseTransformPoint(userTracker.userTransform.position);
            Vector3 mapToWorldOffset = userTracker.userTransform.position - (userNodeLocal / mapUnitsPerMeter);


            userVisualizer.ShowWorldPath(
                fullPath,
                mapToWorldOffset: mapToWorldOffset,
                navGraph: navGraph
            );

            Debug.Log($"🧭 [DEBUG] Angle between XR Rig forward and path: {GetAngleOffsetBetweenXRRigAndMapPath(fullPath):F1}°");
        }


    }

    public void SnapMapToScannedMarker(string markerNodeID, Vector3 scannedWorldPosition, Quaternion scannedWorldRotation)
    {
        NavNode markerNode = navGraph.GetNodeByID(markerNodeID);

        if (markerNode == null || markerNode.nodeType != NodeType.Marker)
        {
            Debug.LogWarning($"[NavigationPopup] No valid marker node found for ID: {markerNodeID}");
            return;
        }

        // Step 1: Get original marker world pose (before scanning)
        Vector3 originalMarkerPos = markerNode.transform.position;
        Quaternion originalMarkerRot = markerNode.transform.rotation;

        // Step 2: Calculate rotation and position offset
        Quaternion rotationOffset = scannedWorldRotation * Quaternion.Inverse(originalMarkerRot);
        Vector3 positionOffset = scannedWorldPosition - (rotationOffset * originalMarkerPos);

        Debug.Log($"📍 Aligning map to marker. Offset Pos: {positionOffset}, Offset Rot: {rotationOffset.eulerAngles}");

        // Step 3: Apply to map root
        mapRootTransform.rotation = rotationOffset * mapRootTransform.rotation;
        mapRootTransform.position = rotationOffset * mapRootTransform.position + positionOffset;

        // Step 4: Optional - re-align visualizers
        visualizer.ClearPath();
        userVisualizer.ClearPath();

        Debug.Log("[NavigationPopup] 🧭 Map and nodes aligned to scanned marker.");
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



        MapController mapController = FindAnyObjectByType<MapController>(FindObjectsInactive.Include);
        if (mapController != null)
            //mapController.ToggleMap();
        currentWorldPath = navGraph.FindPath(startNode, pendingDestination);
        navigationUpdater.BeginPathProgression(
        currentWorldPath,
        userVisualizer.GetWorldPathPoints()
    );

    }







    public void Cancel()
    {
        visualizer.ClearPath();
        userVisualizer?.ClearPath();
        pendingDestination = null;
    }
}

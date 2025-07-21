using UnityEngine;

public class UserLocationTracker : MonoBehaviour
{
    public Transform userTransform;         
    public Transform mapRootTransform;     
    public NavGraphManager navGraph;

    private NavNode currentClosest;

    public Vector3 GetUserNodeForwardInMapSpace()
    {
        // XR forward in world
        Vector3 xrForward = userTransform.forward;

        // Convert to map space
        Quaternion worldToMap = Quaternion.Inverse(transform.parent.rotation); // mapRootTransform
        Vector3 forwardInMap = worldToMap * xrForward;

        return forwardInMap.normalized;
    }

    public Vector3 GetUserNodeForwardInWorldSpace()
    {
        // This returns the userNode.forward in world space
        return transform.forward;
    }


    void Update()
    {
        currentClosest = navGraph.FindNearestNode(userTransform.position);
        Debug.Log("Closest Node: " + currentClosest.nodeID);

        if (userTransform == null) return;

        Vector3 forwardFlat = userTransform.forward;
        forwardFlat.y = 0f;
        if (forwardFlat.sqrMagnitude < 0.001f) return;

        // ✅ Step 1: convert XR forward to map space
        Quaternion worldToMap = Quaternion.Inverse(mapRootTransform.rotation);
        Vector3 forwardInMap = worldToMap * forwardFlat.normalized;

        // ✅ Step 2: rotate userNode (this GameObject) to match that direction
        if (forwardInMap.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(mapRootTransform.rotation * forwardInMap, Vector3.up);

        Debug.DrawRay(transform.position, transform.forward * 0.5f, Color.green); // world-space forward


    }

    public void MoveUserNodeRelativeToMarker(NavNode markerNode, float angleDegrees, float distance)
    {
        if (markerNode == null || markerNode.nodeType != NodeType.Marker)
        {
            Debug.LogWarning("[UserLocationTracker] Marker node is null or not a Marker.");
            return;
        }

        // 1️⃣ Base direction = marker.forward (in world space)
        Vector3 markerForward = markerNode.transform.forward;
        markerForward.y = 0f; // keep it flat
        markerForward.Normalize();

        // 2️⃣ Rotate marker forward by given angle around Y axis
        Quaternion rotationOffset = Quaternion.Euler(0f, angleDegrees, 0f);
        Vector3 direction = rotationOffset * markerForward;

        // 3️⃣ Calculate target position
        Vector3 targetWorldPosition = markerNode.transform.position + direction * distance;

        // 4️⃣ Move user node (this GameObject)
        transform.position = targetWorldPosition;

        // 5️⃣ Face same direction as direction vector
        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        Debug.DrawLine(markerNode.transform.position, targetWorldPosition, Color.red, 2f);
        Debug.Log($"[UserLocationTracker] Moved user node to {distance}m at {angleDegrees}° from marker {markerNode.nodeID}");
    }


    public NavNode GetCurrentNode()
    {
        return currentClosest;
    }
}

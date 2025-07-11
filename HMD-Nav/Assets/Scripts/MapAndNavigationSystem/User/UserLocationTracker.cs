using UnityEngine;

public class UserLocationTracker : MonoBehaviour
{
    public Transform userTransform;          // XR rig (world)
    public Transform mapRootTransform;       // The rotating map (userNode's parent)
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


    public NavNode GetCurrentNode()
    {
        return currentClosest;
    }
}

using UnityEngine;

public class UserLocationTracker : MonoBehaviour
{
    public Transform userTransform;          // XR rig (world)
    public Transform mapRootTransform;       // The rotating map (userNode's parent)
    public NavGraphManager navGraph;
    public Transform directionIndicator;
    public float Indicatoroffset = 0.1f;

    private NavNode currentClosest;

    public Vector3 GetForwardInMapSpace()
    {
        Vector3 worldForward = userTransform.forward;
        worldForward.y = 0f;

        if (mapRootTransform == null)
        {
            Debug.LogError("[UserLocationTracker] mapRootTransform not assigned!");
            return Vector3.forward;
        }

        Quaternion currentWorldToMap = Quaternion.Inverse(mapRootTransform.rotation);
        return currentWorldToMap * worldForward.normalized;
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

        if (userTransform == null || directionIndicator == null) return;

        Vector3 forwardFlat = userTransform.forward;
        forwardFlat.y = 0f;
        if (forwardFlat.sqrMagnitude < 0.001f) return;

        Vector3 forwardInMap = transform.InverseTransformDirection(forwardFlat.normalized);

        Debug.DrawRay(transform.position, transform.TransformDirection(forwardInMap) * 0.5f, Color.red);
        directionIndicator.localPosition = forwardInMap.normalized * Indicatoroffset;
        directionIndicator.localRotation = Quaternion.LookRotation(forwardInMap.normalized, Vector3.up);
    }

    public NavNode GetCurrentNode()
    {
        return currentClosest;
    }
}

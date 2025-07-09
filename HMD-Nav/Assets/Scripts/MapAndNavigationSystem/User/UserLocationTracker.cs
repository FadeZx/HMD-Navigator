// UserLocationTracker.cs
using UnityEngine;

public class UserLocationTracker : MonoBehaviour
{
    public Transform userTransform;
    public NavGraphManager navGraph;
    public Transform directionIndicator;
    public float Indicatoroffset = 0.1f; // Offset for the direction indicator in map space

    private NavNode currentClosest;

    void Update()
    {
        currentClosest = navGraph.FindNearestNode(userTransform.position);
        Debug.Log("Closest Node: " + currentClosest.nodeID);

        if (userTransform == null || directionIndicator == null) return;

        // 1. Get forward direction from XR rig (in world), flatten it
        Vector3 forwardFlat = userTransform.forward;
        forwardFlat.y = 0f;

        if (forwardFlat.sqrMagnitude < 0.001f) return;

        // 2. Convert forward to map space direction (local to userNode parent)
        Vector3 forwardInMap = transform.InverseTransformDirection(
     Quaternion.Euler(0, -90f, 0) * forwardFlat.normalized
 );



        Debug.DrawRay(transform.position, transform.TransformDirection(forwardInMap) * 0.5f, Color.red);

        // 3. Position the indicator slightly in front of the userNode (in local space)
        directionIndicator.localPosition = forwardInMap.normalized * Indicatoroffset;



        // 4. Rotate the indicator to face the direction (locally)
        directionIndicator.localRotation = Quaternion.LookRotation(forwardInMap.normalized, Vector3.up);
    }




    public NavNode GetCurrentNode()
    {
        return currentClosest;
    }
}

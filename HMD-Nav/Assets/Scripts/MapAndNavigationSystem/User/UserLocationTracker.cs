// UserLocationTracker.cs
using UnityEngine;

public class UserLocationTracker : MonoBehaviour
{
    public Transform userTransform;
    public NavGraphManager navGraph;

    private NavNode currentClosest;

    void Update()
    {
        currentClosest = navGraph.FindNearestNode(userTransform.position);
        Debug.Log("Closest Node: " + currentClosest.nodeID);
    }

    public NavNode GetCurrentNode()
    {
        return currentClosest;
    }
}

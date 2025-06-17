using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    RouteJunction,
    Attraction,
    Elevator,
    Entrance
}

public class NavNode : MonoBehaviour
{
    public string nodeID;
    public NodeType nodeType;
    public bool hasCVMarker = false;

    public List<NavEdge> connections = new List<NavEdge>();

    private void OnDrawGizmos()
    {
        // Draw the node sphere
        Gizmos.color = nodeType == NodeType.Attraction ? Color.cyan : Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.2f);

        // Draw connections
        Gizmos.color = Color.white;
        foreach (var edge in connections)
        {
            if (edge != null && edge.target != null)
            {
                Gizmos.DrawLine(transform.position, edge.target.transform.position);
            }
        }
    }
}

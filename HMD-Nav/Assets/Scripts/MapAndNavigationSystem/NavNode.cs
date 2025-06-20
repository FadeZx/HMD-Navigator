using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    RouteJunction,
    Attraction,
    Elevator,
    Entrance
}

[ExecuteAlways]
public class NavNode : MonoBehaviour
{
    public string nodeID;
    public NodeType nodeType;
    public bool hasCVMarker = false;

    public float gizmoRadius = 0.2f;

    [Header("Manual Connections (Editable)")]
    public List<NavEdge> manualConnections = new List<NavEdge>();

    [HideInInspector] public List<NavEdge> connections = new List<NavEdge>();

    private void Awake()
    {
        if (string.IsNullOrEmpty(nodeID))
        {
            nodeID = $"{nodeType}_{gameObject.name}";
        }
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UseManualConnections(); // Always refresh in editor
        }
#endif
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UseManualConnections();
        }
#endif
    }

    public void UseManualConnections()
    {
        connections = new List<NavEdge>();

        foreach (var edge in manualConnections)
        {
            if (edge?.target == null || edge.target == this) continue;

            edge.weight = Vector3.Distance(transform.position, edge.target.transform.position);
            connections.Add(edge);
        }
    }
    public void AddBidirectionalConnection(NavNode other)
    {
        if (other == null || other == this) return;

        float distance = Vector3.Distance(transform.position, other.transform.position);

        // Add to this node
        var toOther = new NavEdge(other, distance);
        if (!manualConnections.Exists(e => e.target == other))
            manualConnections.Add(toOther);

        // Add to other node
        var backToThis = new NavEdge(this, distance);
        if (!other.manualConnections.Exists(e => e.target == this))
            other.manualConnections.Add(backToThis);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = nodeType == NodeType.Attraction ? Color.cyan : Color.yellow;
        Gizmos.DrawSphere(transform.position, gizmoRadius);

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

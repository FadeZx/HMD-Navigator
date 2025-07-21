using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum NodeType
{
    RouteJunction,
    Attraction,
    Elevator,
    Entrance,
    Marker // ✅ NEW type
}

[ExecuteAlways]
public class NavNode : MonoBehaviour
{
    public string nodeID;
    public NodeType nodeType;

    public float gizmoRadius = 0.2f;

    [Header("Manual Connections (Editable)")]
    public List<NavEdge> manualConnections = new List<NavEdge>();

    [HideInInspector] public List<NavEdge> connections = new List<NavEdge>();

    [Header("Edit Mode Visual")]
    public bool isEditMode = false;
    public GameObject nodeVisual;
    public GameObject textLabel;
    public GameObject pokeInteractionObject;

    private NavNodeSpawner spawner;

    private void Awake()
    {
        spawner = FindFirstObjectByType<NavNodeSpawner>();
        if (spawner == null)
        {
            Debug.LogWarning("[NavNode] No NavNodeSpawner found in scene!");
        }

        if (string.IsNullOrEmpty(nodeID))
        {
            nodeID = $"{nodeType}_{gameObject.name}";
        }

        UpdateEditVisual();
    }

    private void Start()
    {
        if (nodeType != NodeType.Marker)
        {
            EnsureBidirectionalConnections();
        }
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UseManualConnections();
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

    private void UpdateEditVisual()
    {
        if (nodeVisual != null)
            nodeVisual.SetActive(isEditMode);

        if (textLabel != null)
        {
            var tmp = textLabel.GetComponent<TextMeshPro>();
            if (tmp != null)
            {
                tmp.text = nodeID;
                tmp.enabled = isEditMode;
            }
            textLabel.SetActive(isEditMode);
        }

        if (pokeInteractionObject != null)
            pokeInteractionObject.SetActive(isEditMode);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateEditVisual();
    }
#endif

    public void UseManualConnections()
    {
        connections = new List<NavEdge>();

        if (nodeType == NodeType.Marker) return; // 🚫 Marker nodes have no connections

        foreach (var edge in manualConnections)
        {
            if (edge?.target == null || edge.target == this) continue;

            edge.weight = Vector3.Distance(transform.position, edge.target.transform.position);
            connections.Add(edge);
        }
    }

    public void AddBidirectionalConnection(NavNode other)
    {
        if (nodeType == NodeType.Marker || other.nodeType == NodeType.Marker) return;

        if (other == null || other == this) return;

        float distance = Vector3.Distance(transform.position, other.transform.position);

        var toOther = new NavEdge(other, distance);
        if (!manualConnections.Exists(e => e.target == other))
            manualConnections.Add(toOther);

        var backToThis = new NavEdge(this, distance);
        if (!other.manualConnections.Exists(e => e.target == this))
            other.manualConnections.Add(backToThis);
    }

    public void EnsureBidirectionalConnections()
    {
        if (nodeType == NodeType.Marker) return;

        foreach (var edge in manualConnections)
        {
            if (edge == null || edge.target == null || edge.target == this) continue;

            var targetNode = edge.target;
            if (targetNode.nodeType == NodeType.Marker) continue;

            bool alreadyLinkedBack = targetNode.manualConnections.Exists(e => e.target == this);

            if (!alreadyLinkedBack)
            {
                float distance = Vector3.Distance(transform.position, targetNode.transform.position);
                targetNode.manualConnections.Add(new NavEdge(this, distance));
#if UNITY_EDITOR
                Debug.Log($"[NavNode] Auto-linked back from {targetNode.name} to {name}");
#endif
            }
        }
    }

    private void OnDrawGizmos()
    {
        // 🎨 Gizmo color based on node type
        if (nodeType == NodeType.Attraction)
            Gizmos.color = Color.cyan;
        else if (nodeType == NodeType.Marker)
            Gizmos.color = Color.blue;
        else
            Gizmos.color = Color.yellow;

        Gizmos.DrawSphere(transform.position, gizmoRadius);

        // 📏 Draw connections (skip for Marker)
        if (nodeType != NodeType.Marker)
        {
            Gizmos.color = Color.white;
            foreach (var edge in connections)
            {
                if (edge != null && edge.target != null)
                {
                    Gizmos.DrawLine(transform.position, edge.target.transform.position);
                }
            }
        }

        // ➤ Forward arrow for Marker node
        if (nodeType == NodeType.Marker)
        {
            Gizmos.color = Color.blue;
            Vector3 forward = transform.forward * 0.5f;
            Gizmos.DrawLine(transform.position, transform.position + forward);
            Gizmos.DrawSphere(transform.position + forward, 0.03f);
        }

#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(transform.position + Vector3.up * (gizmoRadius + 0.01f), nodeID);
#endif
    }

    public void SpawnWorldNode()
    {
        if (spawner != null)
        {
            spawner.SpawnFromNavNode(this);
            Debug.Log($"[NavNode] Spawned world node from {name}");
        }
    }
}

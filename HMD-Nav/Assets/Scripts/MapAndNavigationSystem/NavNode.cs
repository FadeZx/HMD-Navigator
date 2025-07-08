using System.Collections.Generic;
using UnityEngine;
using TMPro;


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


    [Header("Edit Mode Visual")]
    public bool isEditMode = false;
    public  GameObject  nodeVisual;
    public GameObject textLabel; // 👈 Add this
    public GameObject pokeInteractionObject;

    private NavNodeSpawner spawner;


    private void Start()
    {
        EnsureBidirectionalConnections(); // auto-fix one-way links
    }

    private void Awake()
    {
        // Find the NavNodeSpawner in the scene
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

    // Inside your NavNode script
    public void SpawnWorldNode()
    {
        if (spawner != null)
        {
            spawner.SpawnFromNavNode(this); // ✅ Pass this instance
            Debug.Log($"[NavNode] Spawned world node from {name}");
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
            pokeInteractionObject.SetActive(isEditMode); // ✅ Hide when not editing
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

    public void EnsureBidirectionalConnections()
    {
        foreach (var edge in manualConnections)
        {
            if (edge == null || edge.target == null || edge.target == this) continue;

            var targetNode = edge.target;

            // If target node doesn't already link back
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
        Gizmos.color = nodeType == NodeType.Attraction ? Color.cyan : Color.yellow;
        Gizmos.DrawSphere(transform.position, gizmoRadius);

        // Draw connections
        Gizmos.color = Color.white;
        foreach (var edge in connections)
        {
            if (edge != null && edge.target != null)
            {
                Gizmos.DrawLine(transform.position, edge.target.transform.position);
            }
        }

#if UNITY_EDITOR
        // Draw node ID label
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * (gizmoRadius + 0.01f),
            nodeID
        );
#endif
    }

  


}

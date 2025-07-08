using UnityEngine;

public class NavNodeSpawner : MonoBehaviour
{
    [Header("Node Spawn Settings")]
    public GameObject wrldNavNodePrefab;          // Prefab with WrldNavNode component
    public OVRSkeleton handSkeleton;
    private OVRSkeleton.BoneId wristBone = OVRSkeleton.BoneId.Hand_WristRoot;

    private Transform wristTransform;

    void Start()
    {
        TryAssignWristBone();
    }

    void TryAssignWristBone()
    {
        if (handSkeleton == null || handSkeleton.Bones == null) return;

        foreach (var bone in handSkeleton.Bones)
        {
            if (bone.Id == wristBone)
            {
                wristTransform = bone.Transform;
                Debug.Log("[NavNodeSpawner] Wrist bone assigned.");
                break;
            }
        }
    }

    public void SpawnNode()
    {
        if (wrldNavNodePrefab == null || wristTransform == null)
        {
            Debug.LogWarning("Missing nodePrefab or wristTransform!");
            return;
        }

        GameObject newNode = Instantiate(
            wrldNavNodePrefab,
            wristTransform.position,
            wristTransform.rotation
        );

        Debug.Log($"[NavNodeSpawner] Spawned node at wrist: {wristTransform.position}");
    }

    /// <summary>
    /// Spawns a WrldNavNode at wrist with ID and type copied from source NavNode.
    /// </summary>
    public void SpawnFromNavNode(NavNode sourceNode)
    {
        if (wrldNavNodePrefab == null || sourceNode == null)
        {
            Debug.LogWarning("[NavNodeSpawner] Missing references for spawn.");
            return;
        }

        // Spawn at the source node's position and rotation
        GameObject newNode = Instantiate(
            wrldNavNodePrefab,
            sourceNode.transform.position + new Vector3(0, 0.02f, 0),
            sourceNode.transform.rotation
        );

        newNode.name = $"Wrld_{sourceNode.nodeID}";

        WrldNavNode wrldComponent = newNode.GetComponent<WrldNavNode>();
        if (wrldComponent != null)
        {
            wrldComponent.linkedNodeID = sourceNode.nodeID;
            wrldComponent.nodeType = sourceNode.nodeType;
        }

        Debug.Log($"[NavNodeSpawner] Spawned WrldNavNode for '{sourceNode.nodeID}' at source position.");
    }

}

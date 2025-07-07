using UnityEngine;


public class NavNodeSpawner : MonoBehaviour
{
    [Header("Node Spawn Settings")]
    public GameObject nodePrefab;
    public OVRSkeleton handSkeleton;               // Assign this in Inspector
    public OVRSkeleton.BoneId wristBone = OVRSkeleton.BoneId.Hand_WristRoot;

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
        if (nodePrefab == null || wristTransform == null)
        {
            Debug.LogWarning("Missing nodePrefab or wristTransform!");
            return;
        }

        GameObject newNode = Instantiate(
            nodePrefab,
            wristTransform.position,
            wristTransform.rotation
        );

        Debug.Log($"[NavNodeSpawner] Spawned node at wrist: {wristTransform.position}");
    }
}

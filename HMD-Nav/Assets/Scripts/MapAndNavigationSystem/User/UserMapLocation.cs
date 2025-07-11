using UnityEngine;

public class UserMapLocation : MonoBehaviour
{
    public Transform xrRigTransform;     // XR Rig or CenterEyeAnchor
    public Transform mapRootTransform;   // The rotating map (userNode's parent)
    public float worldToMapScale = 0.5f; // 1 world meter = 0.5 map units

    private Vector3 initialXRRigWorldPos;
    private Vector3 initialUserNodeLocalPos;
    private Quaternion initialMapRotation;
    private Vector3 forwardInMap;

    void Start()
    {
        if (xrRigTransform == null || mapRootTransform == null)
        {
            Debug.LogError("UserMapLocation: Assign xrRigTransform and mapRootTransform.");
            enabled = false;
            return;
        }

        initialXRRigWorldPos = xrRigTransform.position;
        initialUserNodeLocalPos = transform.localPosition;
        initialMapRotation = mapRootTransform.rotation * Quaternion.Inverse(transform.rotation);

    }


    public Vector3 GetUserNodeForwardInMapSpace()
    {
        return forwardInMap;
    }

    void Update()
    {
        // Movement logic (unchanged)
        Vector3 worldDelta = xrRigTransform.position - initialXRRigWorldPos;
        worldDelta.y = 0f;

        Quaternion worldToMapRotation = Quaternion.Inverse(initialMapRotation);
        Vector3 deltaInMapSpace = worldToMapRotation * worldDelta;

        transform.localPosition = initialUserNodeLocalPos + deltaInMapSpace * worldToMapScale;

        // Rotation logic — ?use current map rotation
        Vector3 worldForward = xrRigTransform.forward;
        worldForward.y = 0f;

        if (worldForward.sqrMagnitude > 0.001f)
        {
            Quaternion currentWorldToMap = Quaternion.Inverse(mapRootTransform.rotation);
            forwardInMap = currentWorldToMap * worldForward.normalized;
            transform.localRotation = Quaternion.LookRotation(forwardInMap);
        }
    }

}

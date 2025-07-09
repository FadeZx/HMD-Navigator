using UnityEngine;

public class UserMapLocation : MonoBehaviour
{
    public Transform xrRigTransform;     // XR Rig or CenterEyeAnchor
    public Transform mapRootTransform;   // The rotating map (userNode's parent)
    public float worldToMapScale = 0.5f; // 1 world meter = 0.5 map units (adjust this)

    private Vector3 initialXRRigWorldPos;
    private Vector3 initialUserNodeLocalPos;

    void Start()
    {
        if (xrRigTransform == null || mapRootTransform == null)
        {
            Debug.LogError("UserMapLocation: Assign xrRigTransform and mapRootTransform.");
            enabled = false;
            return;
        }

        // Store initial positions
        initialXRRigWorldPos = xrRigTransform.position;
        initialUserNodeLocalPos = transform.localPosition;
    }

    void Update()
    {
        // Calculate how much the XR rig has moved since start
        Vector3 worldDelta = xrRigTransform.position - initialXRRigWorldPos;
        worldDelta.y = 0f; // Ignore vertical movement

        // Convert delta movement into map space direction
        Vector3 deltaInMapSpace = mapRootTransform.InverseTransformDirection(worldDelta);

        // Scale and apply movement to userNode
        transform.localPosition = initialUserNodeLocalPos + deltaInMapSpace * worldToMapScale;

        // ROTATION: Keep using XR rig forward
        Vector3 worldForward = xrRigTransform.forward;
        worldForward.y = 0f;

        if (worldForward.sqrMagnitude > 0.001f)
        {
            Vector3 forwardInMapSpace = mapRootTransform.InverseTransformDirection(worldForward.normalized);
            transform.localRotation = Quaternion.LookRotation(forwardInMapSpace);
        }
    }
}

using UnityEngine;

public class UserMapLocation : MonoBehaviour
{
    public Transform xrRigTransform;    // XR Rig or CenterEyeAnchor
    public Transform mapRootTransform;  // The rotating map (userNode's parent)

    void Update()
    {
        if (xrRigTransform == null || mapRootTransform == null) return;

        // Get XR Rig forward direction (flattened on Y)
        Vector3 worldForward = xrRigTransform.forward;
        worldForward.y = 0;

        if (worldForward.sqrMagnitude < 0.001f)
            return;

        // Convert the XR forward into local direction relative to map rotation
        Vector3 forwardInMapSpace = mapRootTransform.InverseTransformDirection(worldForward.normalized);

        // Apply that direction as local rotation (userNode is child of map)
        transform.localRotation = Quaternion.LookRotation(forwardInMapSpace);
    }
}

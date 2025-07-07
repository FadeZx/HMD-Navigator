using UnityEngine;

public class BillboardFaceCamera : MonoBehaviour
{
    [Tooltip("If true, X-axis (tilt) will be fixed and the object won't tilt up/down to face the camera.")]
    public bool lockXRotation = true;

    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void LateUpdate()
    {
        if (Camera.main == null) return;

        Vector3 directionToCamera = Camera.main.transform.position - transform.position;

        if (lockXRotation)
        {
            directionToCamera.y = 0f; // Keep vertical rotation fixed
        }

        if (directionToCamera != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(-directionToCamera.normalized);
        }

        transform.localScale = initialScale;
    }
}

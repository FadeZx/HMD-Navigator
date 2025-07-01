using UnityEngine;

public class BillboardFaceCamera : MonoBehaviour
{
    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void LateUpdate()
    {
        if (Camera.main == null) return;

        // Point the object toward the camera
        Vector3 directionToCamera = Camera.main.transform.position - transform.position;
        directionToCamera.y = 0f; // Optional: Lock vertical axis to avoid tilting

        if (directionToCamera != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(-directionToCamera);
        }

        // Restore original scale (in case rotation messed it up)
        transform.localScale = initialScale;
    }
}

// UserMapLocation.cs
using UnityEngine;

public class UserMapLocation : MonoBehaviour
{
    public float moveSpeed = 2f;

    void Update()
    {
        // This can be replaced later with headset tracking or CV-based position
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        transform.Translate(new Vector3(h, 0, v) * moveSpeed * Time.deltaTime);
    }
}

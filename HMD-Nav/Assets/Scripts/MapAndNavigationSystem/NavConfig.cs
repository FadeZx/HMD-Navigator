using UnityEngine;

public class NavConfig : MonoBehaviour
{
    public static NavConfig Instance;
    public float mapUnitsPerMeter = 0.0012f;
    public float walkSpeed = 1.2f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}

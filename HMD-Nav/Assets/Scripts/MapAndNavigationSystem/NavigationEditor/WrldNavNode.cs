using UnityEngine;

public class WrldNavNode : MonoBehaviour
{
    public string linkedNodeID;
    public NodeType nodeType;

    // Optional visual name update
    void Start()
    {
        gameObject.name = $"Wrld_{linkedNodeID}";
    }
}

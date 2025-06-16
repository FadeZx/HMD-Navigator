using UnityEngine;

[System.Serializable]
public class NavEdge
{
    public NavNode target;
    public float weight;

    public NavEdge(NavNode targetNode, float weight)
    {
        target = targetNode;
        this.weight = weight;
    }
}

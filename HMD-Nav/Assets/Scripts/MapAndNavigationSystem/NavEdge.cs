[System.Serializable]
public class NavEdge
{
    public NavNode target;
    public float weight;

   
    public NavEdge() { }

    public NavEdge(NavNode targetNode, float weight)
    {
        target = targetNode;
        this.weight = weight;
    }
}

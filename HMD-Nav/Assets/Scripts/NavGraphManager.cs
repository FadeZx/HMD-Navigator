using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NavGraphManager : MonoBehaviour
{
    public List<NavNode> allNodes;

    void Start()
    {
        // Auto-fill node list if not assigned
        if (allNodes == null || allNodes.Count == 0)
        {
            allNodes = FindObjectsOfType<NavNode>().ToList();
        }
    }

    public NavNode FindNearestNode(Vector3 position)
    {
        return allNodes.OrderBy(n => Vector3.Distance(n.transform.position, position)).FirstOrDefault();
    }

    // Placeholder for Dijkstra or A* to be added next
    public List<NavNode> FindPath(NavNode start, NavNode goal)
    {
        // To be implemented
        return new List<NavNode>();
    }
}

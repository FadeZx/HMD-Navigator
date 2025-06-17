using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NavGraphManager : MonoBehaviour
{
    public List<NavNode> allNodes;

    void Start()
    {
        if (allNodes == null || allNodes.Count == 0)
        {
            allNodes = FindObjectsOfType<NavNode>().ToList();
        }
    }

    public NavNode FindNearestNode(Vector3 position)
    {
        return allNodes.OrderBy(n => Vector3.Distance(n.transform.position, position)).FirstOrDefault();
    }

    // ✅ Dijkstra's Algorithm
    public List<NavNode> FindPath(NavNode start, NavNode goal)
    {
        var previous = new Dictionary<NavNode, NavNode>();
        var distances = new Dictionary<NavNode, float>();
        var unvisited = new List<NavNode>(allNodes);

        foreach (var node in allNodes)
        {
            distances[node] = float.MaxValue;
        }
        distances[start] = 0;

        while (unvisited.Count > 0)
        {
            var current = unvisited.OrderBy(n => distances[n]).First();
            unvisited.Remove(current);

            if (current == goal) break;

            foreach (var edge in current.connections)
            {
                var neighbor = edge.target;
                float alt = distances[current] + edge.weight;

                if (alt < distances[neighbor])
                {
                    distances[neighbor] = alt;
                    previous[neighbor] = current;
                }
            }
        }

        // Reconstruct path
        var path = new List<NavNode>();
        var currentNode = goal;

        while (previous.ContainsKey(currentNode))
        {
            path.Insert(0, currentNode);
            currentNode = previous[currentNode];
        }

        if (currentNode == start)
        {
            path.Insert(0, start); // Include start node
        }

        return path;
    }
}

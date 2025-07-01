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
            allNodes = FindObjectsByType<NavNode>(FindObjectsSortMode.None).ToList();
            Debug.Log($"[NavGraphManager] Found {allNodes.Count} NavNodes");
        }
    }



    public NavNode FindNearestNode(Vector3 position)
    {
        if (allNodes == null || allNodes.Count == 0)
        {
            Debug.LogWarning("[NavGraphManager] No nodes found in allNodes!");
            return null;
        }

        var closest = allNodes.OrderBy(n => Vector3.Distance(n.transform.position, position)).FirstOrDefault();
        //Debug.Log($"[NavGraphManager] Nearest node to {position} is {closest?.name}");
        return closest;
    }

    public float GetPathWeight(List<NavNode> path)
    {
        float total = 0f;
        for (int i = 0; i < path.Count - 1; i++)
        {
            var from = path[i];
            var to = path[i + 1];
            var edge = from.connections.Find(e => e.target == to);
            if (edge != null)
            {
                total += edge.weight;
            }
        }
        return total;
    }

    //  Dijkstra's Algorithm
    public List<NavNode> FindPath(NavNode start, NavNode goal)
    {
        if (!allNodes.Contains(start)) Debug.LogError($"[FindPath] Start node '{start?.name}' not in allNodes!");
        if (!allNodes.Contains(goal)) Debug.LogError($"[FindPath] Goal node '{goal?.name}' not in allNodes!");

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

        // ✅ Debug log of path
        Debug.Log($"[FindPath] Path from {start.name} to {goal.name}:");
        for (int i = 0; i < path.Count; i++)
        {
            Debug.Log($"  Step {i}: {path[i].name} at {path[i].transform.position}");
        }
        if (path.Count == 0)
        {
            Debug.LogWarning($"[FindPath] No valid path reconstructed from {start?.name} to {goal?.name}");
        }


        return path;
    }

}

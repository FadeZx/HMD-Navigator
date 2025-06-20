using UnityEngine;
using System.Collections.Generic;

public class NavRouteBaker : MonoBehaviour
{
    public float defaultWeight = 1f;

    public void BuildRoute()
    {
        var children = new List<Transform>();
        foreach (Transform child in transform)
        {
            children.Add(child);
        }

        for (int i = 0; i < children.Count; i++)
        {
            var node = children[i].gameObject.GetComponent<NavNode>();
            if (node == null)
                node = children[i].gameObject.AddComponent<NavNode>();

            node.manualConnections.Clear();
            if (i > 0)
            {
                var prevNode = children[i - 1].gameObject.GetComponent<NavNode>();
                float dist = Vector3.Distance(node.transform.position, prevNode.transform.position);
                node.manualConnections.Add(new NavEdge(prevNode, dist));
                prevNode.manualConnections.Add(new NavEdge(node, dist)); // ✅ bidirectional
            }
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        Debug.Log("[NavRouteBuilder] Route built!");
    }
}

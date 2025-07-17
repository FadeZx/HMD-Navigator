using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationUpdater : MonoBehaviour
{
    public UserLocationTracker userTracker;
    public UserNavigationVisualizer userVisualizer;
    public PathVisualizer visualizer;

    private List<NavNode> currentWorldPath;
    private float reachThreshold;
    private Vector3 originalUserStartPos;
    private bool hasReachedFirstNode = false;
    private List<Vector3> userPathWorldPoints; // ⬅️ This is the replicated path in world space



    public void BeginPathProgression(List<NavNode> path, List<Vector3> worldPathPoints)
    {
        currentWorldPath = new List<NavNode>(path);
        userPathWorldPoints = new List<Vector3>(worldPathPoints); // Store for comparison

        reachThreshold = NavConfig.Instance.reachThreshold;
        hasReachedFirstNode = false;

        originalUserStartPos = userTracker.mapRootTransform.InverseTransformPoint(userTracker.userTransform.position);

        // ✅ Render full user world path, not a short segment
        //userVisualizer.ShowWorldPath(path, userTracker.userTransform.position);

        StartCoroutine(UpdateWorldPathProgression());
    }



    private IEnumerator UpdateWorldPathProgression()
    {
        GameObject debugSphere = null;

        while (currentWorldPath.Count > 0)
        {
            Vector3 userWorldPos = userTracker.userTransform.position;
            Vector3 targetWorldPathPoint = userPathWorldPoints[0]; // World-space

            float dist = Vector3.Distance(userWorldPos, targetWorldPathPoint);

            Debug.DrawLine(userWorldPos, targetWorldPathPoint, Color.green, 0.5f);
            Debug.Log($"User: {userWorldPos}, Target: {targetWorldPathPoint}, Dist: {dist}");
            Debug.Log($"🧭 [Updater] Checking distance to next node {currentWorldPath[0].nodeID}: dist = {dist:F3}");

            // 🛠️ Show reach sphere
            if (debugSphere == null)
            {
                debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                debugSphere.name = "ReachThresholdDebugSphere";
                debugSphere.GetComponent<Collider>().enabled = false;
                debugSphere.GetComponent<Renderer>().material.color = Color.red;
            }

            debugSphere.transform.position = targetWorldPathPoint;
            debugSphere.transform.localScale = Vector3.one * reachThreshold * 2;

            if (dist <= reachThreshold)
            {
                Debug.Log($"✅ [Updater] Reached node: {currentWorldPath[0].nodeID}");

                currentWorldPath.RemoveAt(0);
                userPathWorldPoints.RemoveAt(0);

                if (!hasReachedFirstNode)
                {
                    hasReachedFirstNode = true;
                }

                if (currentWorldPath.Count >= 2)
                {
                    visualizer.ShowPathWithoutUser(currentWorldPath);
                    Debug.Log($"🧩 [Updater] Trimmed map path updated. Next target: {currentWorldPath[0].nodeID}");
                }
                else
                {
                    Debug.Log("🏁 [Updater] Final node reached. Clearing path.");
                    //userVisualizer.ClearPath();
                    visualizer.ClearPath();
                    Destroy(debugSphere);
                    yield break;
                }
            }

            yield return new WaitForSeconds(0.2f);
        }

        userVisualizer.ClearPath();
        Destroy(debugSphere);
    }




}

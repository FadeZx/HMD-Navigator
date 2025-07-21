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
    public NavGraphManager navGraph;

    [Header("Debug Toggles")]
    public bool showFullPath = false;
    public bool showReachDebugSphere = true;


    public void BeginPathProgression(List<NavNode> path, List<Vector3> worldPathPoints)
    {
        Debug.Log("🟢 BeginPathProgression CALLED");
        currentWorldPath = new List<NavNode>(path);
        userPathWorldPoints = new List<Vector3>(worldPathPoints); // Store for comparison

        reachThreshold = NavConfig.Instance.reachThreshold;
        hasReachedFirstNode = false;

        originalUserStartPos = userTracker.mapRootTransform.InverseTransformPoint(userTracker.userTransform.position);

        userVisualizer.ClearPath(); // 🧼 <-- reset here

        if (showFullPath)
        {
            userVisualizer.ShowWorldPath(path, userTracker.userTransform.position, navGraph);
        }

        StartCoroutine(UpdateWorldPathProgression());
    }




    private IEnumerator UpdateWorldPathProgression()
    {
        GameObject debugSphere = null;

        while (currentWorldPath.Count > 1) // Only proceed if we have at least 2 nodes to make an edge
        {
            Vector3 userWorldPos = userTracker.userTransform.position;
            Vector3 targetWorldPathPoint = userPathWorldPoints[1] + userVisualizer.debugPathOffset; // (used for collision)
            Vector3 visualTarget = targetWorldPathPoint + Vector3.down * userVisualizer.verticalOffset; // lowered line render only

            float dist = Vector3.Distance(userWorldPos, targetWorldPathPoint);

            Debug.DrawLine(userWorldPos, targetWorldPathPoint, Color.green, 0.5f);
            Debug.Log($"User: {userWorldPos}, Target: {targetWorldPathPoint}, Dist: {dist}");
            Debug.Log($"🧭 [Updater] Checking distance to next node {currentWorldPath[1].nodeID}: dist = {dist:F3}");

            // 🔴 Draw or update debug reach sphere
            if (showReachDebugSphere)
            {
                if (debugSphere == null)
                {
                    debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    debugSphere.name = "ReachThresholdDebugSphere";
                    debugSphere.GetComponent<Collider>().enabled = false;

                    Material redMat = new Material(Shader.Find("Unlit/Color"));
                    redMat.color = Color.red;
                    debugSphere.GetComponent<Renderer>().material = redMat;
                }

                debugSphere.transform.position = targetWorldPathPoint;
                debugSphere.transform.localScale = Vector3.one * reachThreshold * 2f;
            }
            // ✅ Draw just one edge from user to next node
            // ✅ Draw static edge from previous path point to current target
            Vector3 previousPathPoint = userPathWorldPoints[0] + userVisualizer.debugPathOffset;
            Vector3 visualPrevious = previousPathPoint + Vector3.down * userVisualizer.verticalOffset;
            userVisualizer.ShowSingleEdge(visualPrevious, visualTarget);

            if (dist <= reachThreshold)
            {
                Debug.Log($"✅ [Updater] Reached node: {currentWorldPath[1].nodeID}");

                currentWorldPath.RemoveAt(0);
                userPathWorldPoints.RemoveAt(0);

                if (!hasReachedFirstNode)
                    hasReachedFirstNode = true;

                if (currentWorldPath.Count >= 2)
                {
                    visualizer.ShowPathWithoutUser(currentWorldPath);
                    Debug.Log($"🧩 [Updater] Trimmed map path updated. Next target: {currentWorldPath[1].nodeID}");

                    Vector3 newFrom = userPathWorldPoints[0] + userVisualizer.debugPathOffset;
                    Vector3 newTo = userPathWorldPoints[1] + userVisualizer.debugPathOffset;
                    Vector3 visualFrom = newFrom + Vector3.down * userVisualizer.verticalOffset;
                    Vector3 visualTo = newTo + Vector3.down * userVisualizer.verticalOffset;
                    userVisualizer.ShowSingleEdge(visualFrom, visualTo);


                }
                else
                {
                    Debug.Log("🏁 [Updater] Final node reached. Clearing path.");
                    visualizer.ClearPath();
                    userVisualizer.ClearPath();
                    Destroy(debugSphere);
                    yield break;
                }
            }


            yield return new WaitForSeconds(0.2f);
        }

        userVisualizer.ClearPath();
        if (debugSphere != null)
            Destroy(debugSphere);

    }





}

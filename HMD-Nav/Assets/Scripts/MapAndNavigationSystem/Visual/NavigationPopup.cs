using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationPopup : MonoBehaviour
{
    public NavGraphManager navGraph;
    public UserLocationTracker userTracker;
    public PathVisualizer visualizer;

    private NavNode pendingDestination;

    public NavNode initDest;

    private void Start()
    {
        // Optional: Start navigation after delay for testing
        //StartCoroutine(InitNavigationAfterDelay(5f));
    }

    private IEnumerator InitNavigationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetDestination(initDest);
        ConfirmNavigation(); // Optional follow-up action
    }

    /// <summary>
    /// Sets the destination and immediately shows the path from user to destination
    /// </summary>
    public void SetDestination(NavNode node)
    {
        pendingDestination = node;

        var start = userTracker.GetCurrentNode();
        if (start == null)
        {
            Debug.LogError("[NavigationPopup] Start node is null! User may not be near any NavNode.");
            return;
        }

        if (pendingDestination == null)
        {
            Debug.LogError("[NavigationPopup] Destination is null!");
            return;
        }

        var path = navGraph.FindPath(start, pendingDestination);
        visualizer.ShowPath(path);
    }

    /// <summary>
    /// Called after path is shown — to do more things (e.g., enable AR arrow, UI messages, etc.)
    /// </summary>
    public void ConfirmNavigation()
    {
        if (pendingDestination == null)
        {
            Debug.LogWarning("[NavigationPopup] No destination was set before confirmation.");
            return;
        }

        Debug.Log($"[NavigationPopup] Confirmed navigation to {pendingDestination.name}");

        // Placeholder for future logic (e.g., activate arrow guidance)
        // You can extend this method with AR effects, voice, mission start, etc.
    }

    public void Cancel()
    {
        gameObject.SetActive(false);
        visualizer.ClearPath();
        pendingDestination = null;
    }
}

using System.Collections;
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
        StartCoroutine(InitNavigationAfterDelay(5f));
    }

    private IEnumerator InitNavigationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetDestination(initDest);
        ConfirmNavigation();
    }

    public void SetDestination(NavNode node)
    {
        pendingDestination = node;
        gameObject.SetActive(true);
    }

    public void ConfirmNavigation()
    {
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
        //gameObject.SetActive(false);
    }

    public void Cancel() => gameObject.SetActive(false);
}

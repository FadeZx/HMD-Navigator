using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class AttractionController : MonoBehaviour
{
    [Header("Attraction Visuals")]
    public GameObject mapPin;
    public Animator pinAnimator;
    public RawImage attractionImageDisplay;
    public Texture attractionTexture;
    public AudioSource soundEffect;

    [Header("Navigation")]
    public NavNode destinationNode;
    public GameObject confirmPanel;
    public TextMeshProUGUI destinationTitleTMP;
    public TextMeshProUGUI distanceText; // shows meters
    public TextMeshProUGUI timeText;     // shows time in minutes and seconds                                      
    public TextMeshProUGUI popupTitleTMP;
    public GameObject destinationButton;

    private bool isActivated = false;

    private void Start()
    {
        if (destinationNode == null)
        {
            Debug.LogWarning($"{name} has no destination node assigned.");
            return;
        }

        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        if (mapPin != null)
            mapPin.SetActive(false);

        if (destinationTitleTMP != null)
            destinationTitleTMP.text = destinationNode.nodeID;

        if (popupTitleTMP != null)
            popupTitleTMP.text = destinationNode.nodeID;

        if (attractionImageDisplay != null && attractionTexture != null)
            attractionImageDisplay.texture = attractionTexture;

        UpdateWeightText(); // ✅ Update route info
    }

    public void ConfirmNavigation()
    {
        // Optional future logic
    }

    public void CancelNavigation()
    {
        isActivated = false;

        if (pinAnimator != null)
            pinAnimator.SetBool("isActive", false);

        if (confirmPanel != null)
            confirmPanel.SetActive(false);
        if (mapPin != null)
            mapPin.SetActive(false);

        NavigationPopup navPopup = FindFirstObjectByType<NavigationPopup>();
        if (navPopup == null)
        {
            Debug.LogError("No NavigationPopup found in scene.");
            return;
        }

        navPopup.Cancel();
        confirmPanel.SetActive(false);
        destinationButton.SetActive(true);

        if (soundEffect != null)
            soundEffect.Play();
    }

    public void SetThisDestination()
    {
        isActivated = true;

        NavigationPopup navPopup = FindFirstObjectByType<NavigationPopup>();
        if (navPopup == null)
        {
            Debug.LogError("No NavigationPopup found in scene.");
            return;
        }

        navPopup.SetDestination(destinationNode);

        if (mapPin != null)
            mapPin.SetActive(true);

        if (pinAnimator != null)
            pinAnimator.SetBool("isActive", true);

        if (destinationButton != null)
            destinationButton.SetActive(false);

        if (confirmPanel != null)
            confirmPanel.SetActive(true);

        if (popupTitleTMP != null)
            popupTitleTMP.text = destinationNode.nodeID;

        if (attractionImageDisplay != null && attractionTexture != null)
            attractionImageDisplay.texture = attractionTexture;

        if (soundEffect != null)
            soundEffect.Play();

        UpdateWeightText(); // ✅ Show distance + time

        Debug.Log($"{name} set navigation to {destinationNode.nodeID}");
    }

    private void UpdateWeightText()
    {
        if (distanceText == null || timeText == null || destinationNode == null)
            return;

        NavigationPopup navPopup = FindFirstObjectByType<NavigationPopup>();
        NavGraphManager navGraph = FindFirstObjectByType<NavGraphManager>();
        if (navPopup == null || navGraph == null)
        {
            distanceText.text = "[Error]";
            timeText.text = "[Error]";
            return;
        }

        NavNode userNode = navPopup.userTracker?.GetCurrentNode();
        if (userNode == null)
        {
            distanceText.text = "[No User]";
            timeText.text = "[No User]";
            return;
        }

        var path = navGraph.FindPath(userNode, destinationNode);
        if (path == null || path.Count == 0)
        {
            distanceText.text = "—";
            timeText.text = "—";
            return;
        }

        float totalWeight = navGraph.GetPathWeight(path);
        float meters = totalWeight / navPopup.mapUnitsPerMeter;
        float timeSeconds = meters / navPopup.walkSpeedMetersPerSecond;

        int mins = Mathf.FloorToInt(timeSeconds / 60);

        //  Updated formatting
        distanceText.text = $"{Mathf.RoundToInt(meters)} m";
        timeText.text = $"{mins} min";

    }

}

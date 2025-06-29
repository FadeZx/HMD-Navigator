using UnityEngine;
using DG.Tweening;

public class MapController : MonoBehaviour
{
    [Header("Map Setup")]
    public GameObject map;
    public Transform userHand;
    public float animationDuration = 0.4f;

    [Header("Scale & Position")]
    public Vector3 hiddenScale = Vector3.one * 0.001f; // use very small scale to avoid breaking interaction
    private Vector3 lastVisibleScale;

    private bool isOpen = true;

    private void Start()
    {
        if (map == null || userHand == null)
        {
            Debug.LogWarning("[MapController] Map or UserHand not assigned.");
            return;
        }

        lastVisibleScale = map.transform.localScale; // Store initial scale
        isOpen = true;
    }

    public void ToggleMap()
    {
        if (map == null || userHand == null) return;

        if (isOpen)
            CloseMap();
        else
            OpenMap();
    }

    public void OpenMap()
    {
        if (map == null || userHand == null) return;

        map.SetActive(true);

        // Start from hand position and hidden scale
        map.transform.position = userHand.position;
        map.transform.localScale = hiddenScale;

        // Animate to current hand position and last visible scale
        map.transform.DOMove(userHand.position, animationDuration); // Stay at hand
        map.transform.DOScale(lastVisibleScale, animationDuration);

        isOpen = true;
    }

    public void CloseMap()
    {
        if (map == null || userHand == null) return;

        // Save current visible scale
        lastVisibleScale = map.transform.localScale;

        // Animate to hand position and hidden scale
        map.transform.DOMove(userHand.position, animationDuration);
        map.transform.DOScale(hiddenScale, animationDuration)
            .OnComplete(() => map.SetActive(false));

        isOpen = false;
    }
}

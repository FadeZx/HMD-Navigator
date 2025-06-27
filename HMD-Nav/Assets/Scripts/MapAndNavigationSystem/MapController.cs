using UnityEngine;
using DG.Tweening;

public class MapController : MonoBehaviour
{
    [Header("Map Setup")]
    public GameObject map;
    public Transform userHand; // Assign the user's hand transform (e.g., left or right)
    public float animationDuration = 0.4f;

    [Header("Scale & Position")]
    private Vector3 originalScale;
    public Vector3 hiddenScale = Vector3.zero;
    private Vector3 originalPosition;

    private bool isOpen = true;

    private void Start()
    {
        if (map == null || userHand == null)
        {
            Debug.LogWarning("[MapController] Map or UserHand not assigned.");
            return;
        }

        // Save original scale and position
        originalScale = map.transform.localScale;
        originalPosition = map.transform.position;

        // Optionally start closed
        map.transform.localScale = originalScale;
        isOpen = true;
    }

    public void ToggleMap()
    {
        if (map == null || userHand == null) return;

        if (isOpen)
        {
            CloseMap();
        }
        else
        {
            OpenMap();
        }
    }

    public void OpenMap()
    {
        map.SetActive(true);
        map.transform.DOMove(originalPosition, animationDuration);
        map.transform.DOScale(originalScale, animationDuration);
        isOpen = true;
    }

    public void CloseMap()
    {
        map.transform.DOMove(userHand.position, animationDuration);
        map.transform.DOScale(hiddenScale, animationDuration)
            .OnComplete(() => map.SetActive(false));
        isOpen = false;
    }
}

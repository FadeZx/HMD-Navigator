using UnityEngine;
using UnityEngine.Events;

public class PinchAction : MonoBehaviour
{
    [Header("Settings")]
    public OVRHand hand;                     // Assign Left or Right hand
    public float pinchThreshold = 0.8f;
    public bool requirePalmOpen = true;

    [Header("Action")]
    public UnityEvent onPinchAction;         // Assign any Unity event here

    private bool wasPinching = false;

    void Update()
    {
        if (hand == null) return;

        bool isPinching = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index) > pinchThreshold;
        bool palmCondition = !requirePalmOpen || hand.IsSystemGestureInProgress;

        if (isPinching && !wasPinching && palmCondition)
        {
            onPinchAction?.Invoke();
            Debug.Log($"[PinchAction] Pinch triggered on {hand.name}");
        }

        wasPinching = isPinching;
    }
}

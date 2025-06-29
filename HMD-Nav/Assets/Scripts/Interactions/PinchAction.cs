using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class PinchAction : MonoBehaviour
{
    [Header("Settings")]
    public OVRHand hand;
    public float pinchThreshold = 0.8f;
    public float palmThreshold = 0.85f; // Threshold for palm facing up detection
    public bool requirePalmOpen = true;
    public float holdDuration = 0.3f;
    public float doublePinchInterval = 0.5f;

    [Header("Events")]
    public UnityEvent onSinglePinch;
    public UnityEvent onHoldPinch;
    public UnityEvent onDoublePinch;

    private bool wasPinching = false;
    private float pinchStartTime = 0f;
    private float lastPinchTime = -999f;
    private bool holdTriggered = false;

    void Update()
    {
        if (hand == null)
        {
            Debug.LogWarning("PinchAction: hand is not assigned.");
            return;
        }

        bool isPinching = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index) > pinchThreshold;
        bool palmUp = !requirePalmOpen || IsPalmFacingUp();

        if (isPinching && palmUp)
        {
            if (!wasPinching)
            {
                // New pinch started
                float timeSinceLast = Time.time - lastPinchTime;

                if (timeSinceLast <= doublePinchInterval)
                {
                    onDoublePinch?.Invoke();
                    Debug.Log("[PinchAction] Double pinch triggered");
                    lastPinchTime = -999f; // reset to avoid triple triggering
                }
                else
                {
                    pinchStartTime = Time.time;
                    holdTriggered = false;
                    lastPinchTime = Time.time;
                }
            }
            else
            {
                // Holding pinch
                if (!holdTriggered && Time.time - pinchStartTime >= holdDuration)
                {
                    onHoldPinch?.Invoke();
                    Debug.Log("[PinchAction] Hold pinch triggered");
                    holdTriggered = true;
                }
            }
        }
        else if (!isPinching && wasPinching)
        {
            if (!holdTriggered && Time.time - pinchStartTime < holdDuration)
            {
                onSinglePinch?.Invoke();
                Debug.Log("[PinchAction] Single pinch triggered");
            }
        }

        wasPinching = isPinching;
    }

    private bool IsPalmFacingUp()
    {
        var skeleton = hand.GetComponentInChildren<OVRSkeleton>();
        if (skeleton == null || skeleton.Bones == null)
            return false;

        var bones = skeleton.Bones.ToList();
        var wrist = bones.Find(b => b.Id == OVRSkeleton.BoneId.Hand_WristRoot);
        var middle = bones.Find(b => b.Id == OVRSkeleton.BoneId.Hand_Middle1);

        if (wrist == null || middle == null)
            return false;

        Vector3 palmNormal = (middle.Transform.position - wrist.Transform.position).normalized;
        float dot = Vector3.Dot(palmNormal, Vector3.up);

        return dot > palmThreshold;
    }
}

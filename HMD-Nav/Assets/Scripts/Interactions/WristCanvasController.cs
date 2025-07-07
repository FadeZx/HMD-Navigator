using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class WristCanvasController : MonoBehaviour
{
    [Header("Canvas & Hand Setup")]
    public GameObject wristCanvas;
    public OVRSkeleton handSkeleton;
    public OVRHand ovrHand;

    [Header("Wrist Canvas Offset")]
    public Vector3 positionOffset = new Vector3(0, 0.05f, -0.05f); // Up + Forward
    public Vector3 rotationOffsetEuler = Vector3.zero;

    [Header("Gesture Settings")]
    public float palmUpThreshold = 0.7f; // dot > 0.7 means facing up

    private Transform wristTransform;
    private bool isVisible = false;

    void Start()
    {
        if (wristCanvas != null)
            wristCanvas.SetActive(false);
    }

    void Update()
    {
        TryAssignWrist();

        if (wristTransform == null || ovrHand == null)
            return;

        if (IsPalmFacingUp())
        {
            if (!isVisible)
            {
                ShowCanvas();
            }
        }
        else
        {
            if (isVisible)
            {
                HideCanvas();
            }
        }

        if (isVisible && wristCanvas != null)
        {
            wristCanvas.transform.position = wristTransform.TransformPoint(positionOffset);

            // Use palm’s forward rotation for canvas alignment
            Quaternion palmRotation = ovrHand.transform.rotation;
            wristCanvas.transform.rotation = palmRotation * Quaternion.Euler(rotationOffsetEuler);
        }
    }


    void TryAssignWrist()
    {
        if (wristTransform == null && handSkeleton != null && handSkeleton.Bones != null && handSkeleton.Bones.Count > 0)
        {
            foreach (var bone in handSkeleton.Bones)
            {
                if (bone.Id == OVRSkeleton.BoneId.Hand_WristRoot)
                {
                    wristTransform = bone.Transform;
                    Debug.Log("Wrist bone found and assigned.");
                    break;
                }
            }
        }
    }

    private bool IsPalmFacingUp()
    {
        var skeleton = ovrHand.GetComponentInChildren<OVRSkeleton>();
        if (skeleton == null || skeleton.Bones == null)
            return false;

        var bones = skeleton.Bones.ToList();
        var wrist = bones.Find(b => b.Id == OVRSkeleton.BoneId.Hand_WristRoot);
        var middle = bones.Find(b => b.Id == OVRSkeleton.BoneId.Hand_Middle1);

        if (wrist == null || middle == null)
            return false;

        Vector3 palmNormal = (middle.Transform.position - wrist.Transform.position).normalized;
        float dot = Vector3.Dot(palmNormal, Vector3.up);

        return dot > palmUpThreshold;
    }

    void ShowCanvas()
    {
        isVisible = true;
        wristCanvas.SetActive(true);
    }

    public void HideCanvas()
    {
        isVisible = false;
        if (wristCanvas != null)
            wristCanvas.SetActive(false);
    }

}

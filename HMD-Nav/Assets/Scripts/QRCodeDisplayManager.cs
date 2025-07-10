using System;
using System.Collections.Generic;
using System.Linq;
using Meta.XR;
using PassthroughCameraSamples;
using UnityEngine;

public class QrCodeDisplayManager : MonoBehaviour
{

    [SerializeField] private QRCodeScanner scanner;
    [SerializeField] private EnvironmentRaycastManager envRaycastManager;

    private readonly Dictionary<string, MarkerController> _activeMarkers = new();
    private WebCamTextureManager _webCamTextureManager;
    private PassthroughCameraEye _passthroughCameraEye;

    private readonly HashSet<string> _lockedMarkers = new();
    private readonly Dictionary<string, PoseAccumulator> _poseAccumulators = new();

    private enum QrRaycastMode
    {
        CenterOnly,
        PerCorner
    }

    [SerializeField] private QrRaycastMode raycastMode = QrRaycastMode.PerCorner;

    private void Awake()
    {
        _webCamTextureManager = FindAnyObjectByType<WebCamTextureManager>();
        _passthroughCameraEye = _webCamTextureManager.Eye;
    }

    private void Update()
    {
        UpdateMarkers();
    }

    private async void UpdateMarkers()
    {
        var qrResults = await scanner.ScanFrameAsync() ?? Array.Empty<QrCodeResult>();

        foreach (var qrResult in qrResults)
        {
            if (qrResult?.corners == null || qrResult.corners.Length < 4)
            {
                continue;
            }

            if (_lockedMarkers.Contains(qrResult.text))
                continue;

            var count = qrResult.corners.Length;
            var uvs = new Vector2[count];
            for (var i = 0; i < count; i++)
            {
                uvs[i] = new Vector2(qrResult.corners[i].x, qrResult.corners[i].y);
            }

            var centerUV = Vector2.zero;
            foreach (var uv in uvs) centerUV += uv;
            centerUV /= count;

            var intrinsics = PassthroughCameraUtils.GetCameraIntrinsics(_passthroughCameraEye);
            var centerPixel = new Vector2Int(
                Mathf.RoundToInt(centerUV.x * intrinsics.Resolution.x),
                Mathf.RoundToInt(centerUV.y * intrinsics.Resolution.y)
            );

            var centerRay = PassthroughCameraUtils.ScreenPointToRayInWorld(_passthroughCameraEye, centerPixel);
            if (!envRaycastManager || !envRaycastManager.Raycast(centerRay, out var centerHitInfo))
            {
                continue;
            }

            var center = centerHitInfo.point;
            var distance = Vector3.Distance(centerRay.origin, center);
            var worldCorners = new Vector3[count];

            for (var i = 0; i < count; i++)
            {
                var pixelCoord = new Vector2Int(
                    Mathf.RoundToInt(uvs[i].x * intrinsics.Resolution.x),
                    Mathf.RoundToInt(uvs[i].y * intrinsics.Resolution.y)
                );
                var r = PassthroughCameraUtils.ScreenPointToRayInWorld(_passthroughCameraEye, pixelCoord);

                if (raycastMode == QrRaycastMode.PerCorner)
                {
                    if (envRaycastManager.Raycast(r, out var cornerHit))
                    {
                        worldCorners[i] = cornerHit.point;
                    }
                    else
                    {
                        worldCorners[i] = r.origin + r.direction * distance;
                    }
                }
                else // CenterOnly
                {
                    worldCorners[i] = r.origin + r.direction * distance;
                }
            }

            // Pose estimation
            center = Vector3.zero;
            foreach (var c in worldCorners)
            {
                center += c;
            }
            center /= count;

            var up = (worldCorners[1] - worldCorners[0]).normalized;
            var right = (worldCorners[2] - worldCorners[1]).normalized;
            var normal = -Vector3.Cross(up, right).normalized;
            var poseRot = Quaternion.LookRotation(normal, up);

            var width = Vector3.Distance(worldCorners[0], worldCorners[1]);
            var height = Vector3.Distance(worldCorners[0], worldCorners[3]);
            var scaleFactor = 1.5f;
            var scale = new Vector3(width * scaleFactor, height * scaleFactor, 1f);

            //Consistency accumulation

            if (!_poseAccumulators.TryGetValue(qrResult.text, out var accumulator))
            {
                accumulator = new PoseAccumulator();
                _poseAccumulators[qrResult.text] = accumulator;
            }
            accumulator.Add(center, poseRot);

            const float posVarThreshold = 0.0025f;  // meters^2
            const float angleVarThreshold = 5f;      // degrees

            if (!accumulator.IsStable(posVarThreshold, angleVarThreshold))
            {
                // Optionally, show a preview marker here if you want!
                continue;
            }

            // Use averaged pose for extra smoothness:
            center = accumulator.MeanPosition;
            poseRot = accumulator.MeanRotation;



            if (_activeMarkers.TryGetValue(qrResult.text, out var marker))
            {
                marker.UpdateMarker(center, poseRot, scale, qrResult.text);
            }
            else
            {
                var markerGo = MarkerPool.Instance.GetMarker();
                if (!markerGo)
                {
                    continue;
                }

                marker = markerGo.GetComponent<MarkerController>();
                if (!marker)
                {
                    continue;
                }

                marker.UpdateMarker(center, poseRot, scale, qrResult.text);
                _activeMarkers[qrResult.text] = marker;
            }

            _lockedMarkers.Add(qrResult.text);
            _poseAccumulators.Remove(qrResult.text);
            QRCodeTracker.Instance.RegisterOrUpdateMarker(qrResult.text, center, poseRot);

        }

        // Cleanup
        var keysToRemove = new List<string>();
        foreach (var kvp in _activeMarkers)
        {
            if (!kvp.Value.gameObject.activeSelf)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            _activeMarkers.Remove(key);
        }
    }
}


// Helper class to store and evaluate recent pose detections
class PoseAccumulator
{

    const int MaxSamples = 10;    //recent detections to consider
    readonly Queue<Vector3> positions = new();
    readonly Queue<Quaternion> rotations = new();

    public void Add(Vector3 pos, Quaternion rot)
    {
        if (positions.Count == MaxSamples) positions.Dequeue();
        if (rotations.Count == MaxSamples) rotations.Dequeue();
        positions.Enqueue(pos);
        rotations.Enqueue(rot);
    }

    public bool IsStable(float positionThreshold, float angleThreshold)
    {
        if (positions.Count < MaxSamples) return false;

        // Position variance
        Vector3 mean = positions.Aggregate(Vector3.zero, (a, b) => a + b) / positions.Count;
        float posVariance = positions.Average(p => (p - mean).sqrMagnitude);

        // Angle variance (relative to the mean)
        Quaternion meanRot = rotations.First(); // Simple approx
        float angleVariance = rotations.Average(r => Quaternion.Angle(r, meanRot));

        return posVariance < positionThreshold && angleVariance < angleThreshold;
    }

    public Vector3 MeanPosition => positions.Aggregate(Vector3.zero, (a, b) => a + b) / positions.Count;
    public Quaternion MeanRotation
    {
        get
        {
            // For simplicity, just return the last rotation
            // For more accuracy, use SLERP to average quaternions (advanced)
            return rotations.Last();
        }

    }


}
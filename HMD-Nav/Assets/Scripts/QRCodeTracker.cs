using UnityEngine;
using System.Collections.Generic;

public class QRCodeTracker : MonoBehaviour
{
    public static QRCodeTracker Instance { get; private set; }

    public class MarkerInfo
    {
        public string qrText;
        public Vector3 position;
        public Quaternion rotation;

        // Convenience properties
        public float GetDistanceTo(Transform target) =>
            Vector3.Distance(position, target.position);

        public Vector3 GetLocalPosition(Transform target) =>
            Quaternion.Inverse(rotation) * (target.position - position);

        public Quaternion GetLocalRotation(Transform target) =>
            Quaternion.Inverse(rotation) * target.rotation;

    }

    private readonly Dictionary<string, MarkerInfo> _markers = new();

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void RegisterOrUpdateMarker(string qrText, Vector3 position, Quaternion rotation)
    {
        if (!_markers.TryGetValue(qrText, out var info))
        {
            info = new MarkerInfo { qrText = qrText };
            _markers[qrText] = info;
        }
        info.position = position;
        info.rotation = rotation;

    }

    public void UnregisterMarker(string qrText)
    {
        _markers.Remove(qrText);
    }

    // Returns all markers currently in user's field of view (within maxDistance and viewAngle)
    public IEnumerable<MarkerInfo> GetAllMarkers() => _markers.Values;

    public IEnumerable<MarkerInfo> GetMarkersInView(Transform camera, float maxDistance = 10f, float fovAngle = 90f)
    {
        foreach (var marker in _markers.Values)
        {
            var toMarker = marker.position - camera.position;
            if (toMarker.magnitude <= maxDistance)
            {
                float angle = Vector3.Angle(camera.forward, toMarker);
                if (angle < fovAngle * 0.5f)
                    yield return marker;
            }
        }
    }


    // Returns the closest marker currently in view
    public MarkerInfo GetClosestMarkerInView(Transform camera, float maxDistance = 10f, float fovAngle = 90f)
    {
        MarkerInfo closest = null;
        float minDist = float.MaxValue;
        foreach (var marker in GetMarkersInView(camera, maxDistance, fovAngle))
        {
            float dist = (marker.position - camera.position).magnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = marker;
            }
        }
        return closest;
    }

    public MarkerInfo GetMarker(string qrText)
    {
        _markers.TryGetValue(qrText, out var info);
        return info;
    }

}

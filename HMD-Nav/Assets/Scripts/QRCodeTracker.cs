using System.Collections.Generic;
using UnityEngine;

public class QRCodeTracker : MonoBehaviour
{
    public static QRCodeTracker Instance { get; private set; }
    private Camera _camera;
    private readonly Dictionary<string, MarkerInfo> _markers = new();


    public struct CalibrationInfo
    {
        public string qrText;
        public Vector3 toMarker;        // Vector from camera to marker
        public Vector3 markerForward;   // Marker’s forward vector
        public float distance;
        public float angleToUser;        // same as markerInfo.GetAngleToTarget()
        public float viewAngleFromUser;  // same as markerInfo.GetViewAngleFromTarget()

        public CalibrationInfo(
       string qrText,
       Vector3 toMarker,
       Vector3 markerForward,
       float distance,
       float angleToUser,
       float viewAngleFromUser)
        {
            this.qrText = qrText;
            this.toMarker = toMarker;
            this.markerForward = markerForward;
            this.distance = distance;
            this.angleToUser = angleToUser;
            this.viewAngleFromUser = viewAngleFromUser;
        }
    }

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

        //Angle between marker's forward and the direction to the user
        //0° = target is directly in front; 180° = directly behind.
        public float GetAngleToTarget(Transform target)
        {
            Vector3 toTarget = (target.position - position).normalized;
            Vector3 markerForward = rotation * Vector3.forward;
            return Vector3.Angle(markerForward, toTarget);
        }

        //Angle between the users's view direction and the marker
        //0° = looking straight at marker; 180° = looking away.

        public float GetViewAngleFromTarget(Transform target)
        {
            Vector3 toMarker = (position - target.position).normalized;
            Vector3 targetForward = target.forward;
            return Vector3.Angle(targetForward, toMarker);
        }



    }

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);
        _camera = Camera.main;
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

    public bool TryGetCalibrationInfo(out CalibrationInfo info, float maxDistance = 10f, float fovAngle = 90f)
    {
        info = default;

        if (_camera == null)
        {
            Debug.LogWarning("Calibration camera not assigned.");
            return false;
        }

        var closestMarker = GetClosestMarkerInView(_camera.transform, maxDistance, fovAngle);
        if (closestMarker == null) return false;

        Vector3 toMarker = (closestMarker.position - _camera.transform.position).normalized;
        Vector3 markerForward = closestMarker.rotation * Vector3.forward;
        float distance = Vector3.Distance(_camera.transform.position, closestMarker.position);

        float angleToUser = closestMarker.GetAngleToTarget(_camera.transform);
        float viewAngleFromUser = closestMarker.GetViewAngleFromTarget(_camera.transform);

        info = new CalibrationInfo(
            closestMarker.qrText,
            toMarker,
            markerForward,
            distance,
            angleToUser,
            viewAngleFromUser
        );

        return true;

    }


    public MarkerInfo GetMarker(string qrText)
    {
        _markers.TryGetValue(qrText, out var info);
        return info;
    }

}

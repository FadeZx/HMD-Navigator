using UnityEngine;
using TMPro;
public class MarkerController : MonoBehaviour
{

    private TextMeshProUGUI _textMesh;
    public float lastUpdateTime;
    private Camera _camera;
    private string _qrText; // Store the QR string for this marker


    private void Awake()
    {
        _camera = Camera.main;
        _textMesh = GetComponentInChildren<TextMeshProUGUI>();
        if (_textMesh == null)
        {
            Debug.LogError("No TextMeshProUGUI found on marker prefab!");
        }
    }

    public void UpdateMarker(Vector3 position, Quaternion rotation, Vector3 scale, string text)
    {
        transform.SetPositionAndRotation(position, rotation);
        transform.localScale = scale;
        if (_textMesh)
        {
            //_textMesh.text = text;
            _qrText = text; // Remember which marker we're showing
        }

        lastUpdateTime = Time.time;
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

    }

    // Update is called once per frame
    void Update()
    {

        if (_textMesh == null || _qrText == null) return;

        //label face the camera
        _textMesh.transform.rotation = Quaternion.LookRotation(_textMesh.transform.position - _camera.transform.position);

        // Get the latest marker data using the stored QR string
        var markerInfo = QRCodeTracker.Instance.GetMarker(_qrText);

        if (markerInfo != null)
        {
            float distance = markerInfo.GetDistanceTo(_camera.transform);
            Vector3 localPos = markerInfo.GetLocalPosition(_camera.transform);
            // Compose real-time label text (change to whatever format you want!)
            _textMesh.text = $"Dist: {distance:F2}m\nLocal: {localPos.ToString("F2")}";
        }
        else
        {
            _textMesh.text = "Marker not tracked";
        }

        //if (_textMesh)
        //{
        //    _textMesh.transform.rotation = Quaternion.LookRotation(_textMesh.transform.position - _camera.transform.position);
        //}

        //if (gameObject.activeSelf && Time.time - lastUpdateTime > 2f)
        //{
        //    gameObject.SetActive(false);
        //}
    }
}

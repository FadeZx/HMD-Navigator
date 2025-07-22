using UnityEngine;
using TMPro;

public class CalibrationDebugDisplay : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI debugText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        debugText.text = "Calibration HUD active!";
    }


    // Update is called once per frame
    void Update()
    {

        if (QRCodeTracker.Instance.TryGetCalibrationInfo(out var info))
        {
            debugText.text =
                $"<b>CALIBRATION MARKER</b>\n" +
                $"QR: {info.qrText}\n" +
                $"Dist: {info.distance:F2}m\n" +
                $"Angle (marker -> you): {info.angleToUser:F1}°\n" +
                $"Angle (you -> marker): {info.viewAngleFromUser:F1}°";
        }
        else
        {
            debugText.text = "No calibration marker in view";
        }

    }
}

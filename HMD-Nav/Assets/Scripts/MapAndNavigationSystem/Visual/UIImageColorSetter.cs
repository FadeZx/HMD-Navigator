using UnityEngine;
using UnityEngine.UI;

public class UIImageColorSetter : MonoBehaviour
{
    public Image targetImage;


    public void SetColor(Color newColor)
    {
        if (targetImage != null)
            targetImage.color = newColor;
    }

    public void SetColorHex(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color col))
            SetColor(col);
        Debug.Log($"Set color to {hex} on {targetImage?.name ?? "null target"}");
    }
}

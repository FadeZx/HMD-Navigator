using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class FixCanvasShaderChannels : MonoBehaviour
{
    void Reset()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.additionalShaderChannels =
                AdditionalCanvasShaderChannels.TexCoord1 |
                AdditionalCanvasShaderChannels.Normal |
                AdditionalCanvasShaderChannels.Tangent;
            Debug.Log("Canvas shader channels set: TexCoord1, Normal, Tangent");
        }
        else
        {
            Debug.LogWarning("No Canvas component found.");
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [Header("Scene Toggle Settings")]
    public string sceneA = "EditMode_Scene";
    public string sceneB = "NavigationMode_Scene";

    public void LoadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            Debug.Log($"Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene name is empty or null.");
        }
    }

    public void QuitApp()
    {
        Debug.Log("Quitting application...");
        Application.Quit();
    }

    /// <summary>
    /// Toggles between sceneA and sceneB.
    /// Attach this to a toggle button.
    /// </summary>
    public void ToggleScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == sceneA)
        {
            Debug.Log($"Toggling to {sceneB}");
            SceneManager.LoadScene(sceneB);
        }
        else if (currentScene == sceneB)
        {
            Debug.Log($"Toggling to {sceneA}");
            SceneManager.LoadScene(sceneA);
        }
        else
        {
            Debug.LogWarning("Current scene is not in toggle list. No action taken.");
        }
    }
}

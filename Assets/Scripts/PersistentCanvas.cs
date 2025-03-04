using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentCanvas : MonoBehaviour
{
    private Canvas canvas;
    // Set this to the exact name of your level scene.
    public string levelSceneName = "Level 1";

    void Awake()
    {
        // Make the canvas persistent across scene loads.
        DontDestroyOnLoad(gameObject);
        canvas = GetComponent<Canvas>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Called when any scene is loaded.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == levelSceneName)
        {
            StartCoroutine(SetCameraFromLevelScene(scene));
        }
    }

    // Coroutine to update the canvas's camera after scene 2 is fully initialized.
    IEnumerator SetCameraFromLevelScene(Scene scene)
    {
        // Wait a short period to ensure the camera is fully initialized.
        yield return new WaitForSeconds(0.1f);

        Camera levelCamera = null;
        foreach (GameObject rootObj in scene.GetRootGameObjects())
        {
            levelCamera = rootObj.GetComponentInChildren<Camera>();
            if (levelCamera != null)
            {
                break;
            }
        }

        if (levelCamera != null)
        {
            canvas.worldCamera = levelCamera;
            Debug.Log("Canvas worldCamera set to level scene camera: " + levelCamera.name);
        }
        else
        {
            Debug.LogError("No camera found in the level scene.");
        }
    }

    void LateUpdate()
    {
        // Reassign worldCamera to ensure it follows Cinemachine's final transform.
        if (canvas != null && canvas.worldCamera != null)
        {
            // This assumes the camera controlled by Cinemachine is your current main camera.
            canvas.worldCamera = Camera.main;
        }
    }

}

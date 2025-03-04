using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadTrigger : MonoBehaviour
{
    [SerializeField] private SceneField[] scenesToLoad;
    [SerializeField] private SceneField[] scenesToUnload;

    private GameObject player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        player = GameObject.Find("HeroKnight");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == player)
        {
            // Load and unload the scenes
            LoadScenes();
            UnloadScenes();

        }
    }

    private void LoadScenes() {
        for (int i = 0; i < scenesToLoad.Length; i++)
        {
            bool isLoaded = false;
            for (int j = 0; j < SceneManager.sceneCount; j++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(j);
                if (loadedScene.name == scenesToLoad[i].SceneName)
                {
                    isLoaded = true;
                    break;
                }
            }

            if (!isLoaded)
            {
                SceneManager.LoadSceneAsync(scenesToLoad[i], LoadSceneMode.Additive);
                // Disable and enable the Cinemachine
            }
        }
    }

    private void UnloadScenes()
    {
        for(int i = 0; i < scenesToUnload.Length; i++)
        {
            for (int j = 0; j < SceneManager.sceneCount; j++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(j);
                if (loadedScene.name == scenesToUnload[i].SceneName)
                {
                    SceneManager.UnloadSceneAsync(scenesToUnload[i]);
                }
            }
        }
    }
}

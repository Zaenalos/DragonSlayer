using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro; // Only if using TextMeshPro

public class BlinkingText : MonoBehaviour, IPointerClickHandler
{
    private TextMeshProUGUI textComponent;

    void Start()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        StartCoroutine(BlinkText());
    }

    IEnumerator BlinkText()
    {
        while (true)
        {
            textComponent.enabled = !textComponent.enabled; // Toggle visibility
            yield return new WaitForSeconds(0.5f);
        }
    }

    // Detects when the text is clicked/tapped
    public void OnPointerClick(PointerEventData eventData)
    {
        UnloadMainMenu();
    }

    void UnloadMainMenu()
    {
        string menuSceneName = "Main Menu Scene"; // Change this to match your main menu scene name
        SceneManager.LoadScene("Tutorial Scene");
        if (SceneManager.GetSceneByName(menuSceneName).isLoaded)
        {
            SceneManager.UnloadSceneAsync(menuSceneName);
        }
    }
}

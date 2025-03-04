using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class BlinkingText : MonoBehaviour, IPointerClickHandler
{
    [Header("Main menu objects")]
    [SerializeField] private GameObject[] mainMenuObjects;

    [Header("Loading UI")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider loadingSlider;

    [Header("Scenes to load")]
    [SerializeField] private SceneField levelScene;

    private List<AsyncOperation> scenesToLoad = new List<AsyncOperation>();

    private TextMeshProUGUI textComponent;
    private readonly WaitForSeconds blinkDelay = new WaitForSeconds(0.5f);

    void Start()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        SetTextAlpha(1f);
        StartCoroutine(BlinkText());

        // Make sure loading panel is initially hidden
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    IEnumerator BlinkText()
    {
        // Toggle alpha between 1 (visible) and 0 (invisible) every 0.5 seconds using a cached WaitForSeconds.
        while (true)
        {
            float currentAlpha = textComponent.color.a;
            float targetAlpha = (currentAlpha == 1f) ? 0f : 1f;
            SetTextAlpha(targetAlpha);
            yield return blinkDelay;
        }
    }

    void SetTextAlpha(float alpha)
    {
        Color color = textComponent.color;
        color.a = alpha;
        textComponent.color = color;
    }

    private void HideMenu()
    {
        for (int i = 0; i < mainMenuObjects.Length; i++)
        {
            mainMenuObjects[i].SetActive(false);
        }
    }

    private IEnumerator ProgressLoad()
    {
        float loadProgress = 0f;
        for (int i = 0; i < scenesToLoad.Count; i++)
        {
            while (!scenesToLoad[i].isDone)
            {
                loadProgress += scenesToLoad[i].progress;
                loadingSlider.value = loadProgress / scenesToLoad.Count;
                yield return null;
            }
        }
    }

    private void LoadScenesAndUnloadMenu()
    {
        // Unload main menu objects immediately.
        HideMenu();
        // Show loading panel
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        // Reset loading slider
        if (loadingSlider != null)
            loadingSlider.value = 0f;

        // Start loading both scenes asynchronously.
        scenesToLoad.Add(SceneManager.LoadSceneAsync(levelScene));

        // Start coroutine to update loading slider.
        StartCoroutine(ProgressLoad());
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        LoadScenesAndUnloadMenu();
    }
}

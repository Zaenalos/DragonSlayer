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
        // Ensure the text is fully visible initially.
        SetTextAlpha(1f);
        StartCoroutine(BlinkText());
    }

    IEnumerator BlinkText()
    {
        // Toggle alpha between 1 (visible) and 0 (invisible) every 0.5 seconds.
        while (true)
        {
            float currentAlpha = textComponent.color.a;
            float targetAlpha = (currentAlpha == 1f) ? 0f : 1f;
            SetTextAlpha(targetAlpha);
            yield return new WaitForSeconds(0.5f);
        }
    }

    void SetTextAlpha(float alpha)
    {
        Color color = textComponent.color;
        color.a = alpha;
        textComponent.color = color;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        // Load the next scene when the text is clicked.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
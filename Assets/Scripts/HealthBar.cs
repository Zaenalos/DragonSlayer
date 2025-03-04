using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{

    public Slider slider;
    private void Awake()
    {
        slider = GetComponent<Slider>();


        if (slider == null)
        {
            Debug.LogError("Slider component not found");
        }
    }
    public void SetMaxHealth(int health)
    {
        slider.minValue = 0;
        slider.maxValue = health;
        slider.value = health;
    }

    public void SetHealth(int health)
    {
        slider.value = health;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public RectTransform moveLeftButton;
    public RectTransform moveRightButton;
    public RectTransform jumpButton;
    public RectTransform attackButton;
    public RectTransform blockButton;
    public RectTransform rollButton;

    void Start()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        ResizeButton(moveLeftButton, screenWidth * 0.15f, screenHeight * 0.10f);
        ResizeButton(moveRightButton, screenWidth * 0.15f, screenHeight * 0.10f);
        ResizeButton(jumpButton, screenWidth * 0.12f, screenHeight * 0.08f);
        ResizeButton(attackButton, screenWidth * 0.12f, screenHeight * 0.08f);
        ResizeButton(blockButton, screenWidth * 0.12f, screenHeight * 0.08f);
        ResizeButton(rollButton, screenWidth * 0.12f, screenHeight * 0.08f);
    }

    void ResizeButton(RectTransform button, float width, float height)
    {
        button.sizeDelta = new Vector2(width, height);
    }
}

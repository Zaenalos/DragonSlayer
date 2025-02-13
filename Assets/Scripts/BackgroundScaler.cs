using UnityEngine;

[ExecuteInEditMode]
public class BackgroundScaler : MonoBehaviour
{
    public Camera mainCamera;
    public SpriteRenderer background;

    void Start()
    {
        FitBackground();
    }

    void FitBackground()
    {
        if (!mainCamera) mainCamera = Camera.main;
        if (!background) background = GetComponent<SpriteRenderer>();

        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        Vector2 spriteSize = background.sprite.bounds.size;

        Vector3 scale = transform.localScale;
        scale.x = cameraWidth / spriteSize.x;
        scale.y = cameraHeight / spriteSize.y;

        transform.localScale = scale;
    }
}

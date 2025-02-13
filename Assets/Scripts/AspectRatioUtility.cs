using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AspectRatioUtility : MonoBehaviour
{
    [SerializeField] private Vector2 baseAspect = new Vector2(16, 9);
    [SerializeField] private float baseOrthographicSize = 5f;

    private Camera cam;
    private float targetRatio;
    private int lastWidth, lastHeight;

    void Awake()
    {
        cam = GetComponent<Camera>();
        targetRatio = baseAspect.x / baseAspect.y;
        AdjustCameraSize();
    }

    void Update()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            AdjustCameraSize();
            lastWidth = Screen.width;
            lastHeight = Screen.height;
        }
    }

    void AdjustCameraSize()
    {
        float screenRatio = (float)Screen.width / Screen.height;

        if (screenRatio >= targetRatio)
        {
            // Wider than target: maintain height
            cam.orthographicSize = baseOrthographicSize;
        }
        else
        {
            // Taller than target: adjust to maintain width
            cam.orthographicSize = baseOrthographicSize * (targetRatio / screenRatio);
        }
    }
}
using UnityEngine;

[RequireComponent(typeof(Camera)), ExecuteAlways]
public class AspectRatioCameraFitter : MonoBehaviour
{
    [SerializeField, Range(0, 2)] int currentAspectRatioIndex;
    [SerializeField] Vector2[] targetAspectRatios = { new(16, 9), new(21, 9), new(6, 13) };
    [SerializeField] Vector2 rectCenter = new(0.5f, 0.5f); // Center of the viewport

    Camera _cam;
    Vector2Int _lastScreenSize;
    int _lastAspectIndex;

    void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    void OnValidate()
    {
        _cam ??= GetComponent<Camera>();
    }

    void Update()
    {
        Vector2Int screenSize;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            screenSize = GetGameViewSize(); // Use reflection to get the game view size in Editor
        }
        else
        {
            screenSize = new Vector2Int(Screen.width, Screen.height); // Use Screen in Play mode
        }
#else
        screenSize = new Vector2Int(Screen.width, Screen.height); // Always use Screen in builds
#endif

        bool needsUpdate = screenSize != _lastScreenSize || currentAspectRatioIndex != _lastAspectIndex;
        if (!needsUpdate) return;

        currentAspectRatioIndex = Mathf.Clamp(currentAspectRatioIndex, 0, targetAspectRatios.Length - 1);
        UpdateCameraViewport(screenSize);
        _lastScreenSize = screenSize;
        _lastAspectIndex = currentAspectRatioIndex;
    }

    void UpdateCameraViewport(Vector2Int screenSize)
    {
        float targetAspect = targetAspectRatios[currentAspectRatioIndex].x / targetAspectRatios[currentAspectRatioIndex].y;
        float screenAspect = (float)screenSize.x / screenSize.y;

        // Default to full viewport (normalized)
        Rect rect = new Rect(0, 0, 1, 1);

        if (screenAspect > targetAspect) // Wider screen
        {
            float scale = screenAspect / targetAspect;
            rect.width = 1 / scale;
            // Use rectCenter.x to center horizontally
            rect.x = rectCenter.x - (rect.width / 2f);
        }
        else // Taller screen
        {
            float scale = targetAspect / screenAspect;
            rect.height = 1 / scale;
            // Use rectCenter.y to center vertically
            rect.y = rectCenter.y - (rect.height / 2f);
        }

        _cam.rect = rect;
    }

#if UNITY_EDITOR
    Vector2Int GetGameViewSize()
    {
        // Use reflection to get the game view size in the editor
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod(
            "GetSizeOfMainGameView",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
        );

        if (GetSizeOfMainGameView != null)
        {
            var size = (Vector2)GetSizeOfMainGameView.Invoke(null, null);
            return new Vector2Int((int)size.x, (int)size.y);
        }

        // Fallback to Screen.width/height if reflection fails
        return new Vector2Int(Screen.width, Screen.height);
    }
#endif
}

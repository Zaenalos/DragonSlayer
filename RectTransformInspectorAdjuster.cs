/*

Got this cool script from the Unity forum, which adjusts the RectTransform size based on the aspect ratio of the screen.

*/


using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class RectTransformInspectorAdjuster : MonoBehaviour
{
    private RectTransform rectTransform;
    public float targetAspect = 16f / 9f;

    private Vector2 lastScreenSize;

#if UNITY_EDITOR
    private void OnEnable()
    {
        EditorApplication.update += EditorUpdate;
        lastScreenSize = GetScreenSize();
    }

    private void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
    }

    private void EditorUpdate()
    {
        Vector2 currentSize = GetScreenSize();
        if (currentSize != lastScreenSize)
        {
            AdjustRectTransform();
            lastScreenSize = currentSize;
        }
    }
#endif

    private void Update()
    {
        if (Application.isPlaying)
        {
            Vector2 currentSize = new Vector2(Screen.width, Screen.height);
            if (currentSize != lastScreenSize)
            {
                AdjustRectTransform();
                lastScreenSize = currentSize;
            }
        }
    }

    private void AdjustRectTransform()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
            return;

        // Get current screen/game view size
        Vector2 screenSize = GetScreenSize();

        float currentAspect = screenSize.x / screenSize.y;

        // Use Undo.RecordObject to make changes visible in Inspector
#if UNITY_EDITOR
        Undo.RecordObject(rectTransform, "Adjust RectTransform Size");
#endif

        if (currentAspect < targetAspect)
        {
            float newWidth = rectTransform.rect.height * targetAspect;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
        }
        else
        {
            float newHeight = rectTransform.rect.width / targetAspect;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
        }

#if UNITY_EDITOR
        // Force Inspector refresh
        EditorUtility.SetDirty(rectTransform);
        PrefabUtility.RecordPrefabInstancePropertyModifications(rectTransform);
        // Update RectTransform values in Inspector
        rectTransform.ForceUpdateRectTransforms();
#endif
    }

    private Vector2 GetScreenSize()
    {
#if UNITY_EDITOR
        return Handles.GetMainGameViewSize();
#else
        return new Vector2(Screen.width, Screen.height);
#endif
    }
}

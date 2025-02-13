using UnityEngine;

public class MaxFPS : MonoBehaviour
{
    [Header("Frame Settings")]
    [Tooltip("Set the target frame rate. Use 0 for uncapped frame rate.")]
    public float TargetFrameRate = 60.0f;

    private float _targetFrameTime;

    void Awake()
    {
        QualitySettings.vSyncCount = 0; // Disable V-Sync
        UpdateFrameRate();
    }

    void Update()
    {
        // Only update if the target frame rate setting has been changed (if you plan to allow changes at runtime)
        if (Application.targetFrameRate != (TargetFrameRate <= 0 ? -1 : (int)TargetFrameRate))
        {
            UpdateFrameRate();
        }

        // Log the actual frame time for debugging (optional)
        float actualFrameTime = Time.deltaTime;
        Debug.Log($"Target Frame Time: {_targetFrameTime:F3} sec, Actual Frame Time: {actualFrameTime:F3} sec");
    }

    void UpdateFrameRate()
    {
        if (TargetFrameRate <= 0)
        {
            Application.targetFrameRate = -1; // Uncapped frame rate
            _targetFrameTime = 0f;
        }
        else
        {
            Application.targetFrameRate = (int)TargetFrameRate;
            _targetFrameTime = 1.0f / TargetFrameRate;
        }
    }
}

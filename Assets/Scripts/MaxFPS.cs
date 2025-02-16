using UnityEngine;

public class MaxFPS : MonoBehaviour
{
    private int targetFrameRate;
    //private float _targetFrameTime;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        RefreshRate screenRefreshRate = Screen.currentResolution.refreshRateRatio;

        // Explicitly cast the double to float here:
        targetFrameRate = Mathf.Max(60, Mathf.RoundToInt((float)screenRefreshRate.value));

        UpdateFrameRate();
    }

    void Update()
    {
        if (Application.targetFrameRate != targetFrameRate)
        {
            UpdateFrameRate();
        }

        Debug.Log($"Target Frame Rate: {targetFrameRate} FPS");
    }

    void UpdateFrameRate()
    {
        Application.targetFrameRate = targetFrameRate;
        //_targetFrameTime = 1f / targetFrameRate;
    }
}
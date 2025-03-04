using UnityEngine;
using Unity.Cinemachine;

public class CameraBoundsSetter : MonoBehaviour
{
    public GameObject polygonBounds;
    private void Start()
    {
        // Find the confiner in the PersistentGameplay scene
        CinemachineConfiner2D confiner = FindFirstObjectByType<CinemachineConfiner2D>();
        if (confiner != null)
        {
            Debug.Log("CinemachineConfiner2D found!");
            PolygonCollider2D bounds = polygonBounds.GetComponent<PolygonCollider2D>();
            if (bounds != null)
            {
                Debug.Log("PolygonCollider2D found!");
                confiner.BoundingShape2D = bounds;
                confiner.InvalidateBoundingShapeCache(); // Refresh the bounding shape cache
            }
        }
        else
        {
            Debug.LogWarning("CinemachineConfiner2D not found!");
        }
    }
}

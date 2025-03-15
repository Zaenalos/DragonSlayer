using UnityEngine;

public class Barrier : MonoBehaviour
{
    [Header("Object Reference")]
    [Tooltip("Assign the GameObject. This barrier will destroy itself if the object is destroyed.")]
    public GameObject playerGameObject;

    private void Update()
    {
        // If the referenced player GameObject has been destroyed, destroy this barrier.
        if (playerGameObject == null)
        {
            Destroy(gameObject);
        }
    }
}

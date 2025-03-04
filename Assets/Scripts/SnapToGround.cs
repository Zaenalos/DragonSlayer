using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SnapToGround : MonoBehaviour
{
    [Header("Raycast Settings")]
    [Tooltip("Tag to identify ground objects (e.g., 'Ground')")]
    [SerializeField] private string groundTag = "Ground";

    [Tooltip("Maximum distance for the downward raycast.")]
    [SerializeField] private float raycastDistance = 5f;

    [Tooltip("Offset for the raycast origin from the object's position.")]
    [SerializeField] private Vector2 rayOffset = Vector2.zero;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        SnapToGroundPosition();
    }

    /// <summary>
    /// Casts a ray downward from the object's position (plus offset) to find the ground with the specified tag,
    /// then snaps the object to that hit point.
    /// </summary>
    private void SnapToGroundPosition()
    {
        // Calculate the raycast origin.
        Vector2 rayOrigin = (Vector2)transform.position + rayOffset;

        // Use RaycastAll to get every collider hit along the ray.
        RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.down, raycastDistance);

        // Iterate through all hits and look for the first with the ground tag.
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag(groundTag))
            {
                Debug.Log($"Raycast hit: {hit.collider.gameObject.name} at {hit.point}");

                // Snap the object's Y position to the hit point.
                Vector3 newPosition = transform.position;
                newPosition.y = hit.point.y;
                transform.position = newPosition;

                // Reset the Rigidbody's velocity to prevent any unwanted movement.
                rb.linearVelocity = Vector2.zero;
                return; // Exit after snapping to the first valid ground hit.
            }
        }

        Debug.LogWarning($"{name}: No ground detected with tag '{groundTag}' within {raycastDistance} units from {rayOrigin}");
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the raycast in the Scene view for debugging.
        Vector2 rayOrigin = (Vector2)transform.position + rayOffset;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.down * raycastDistance);
    }
}

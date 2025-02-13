using UnityEngine;

public class SnapToGround : MonoBehaviour
{
    public string groundTag = "Ground";  // Tag to identify ground objects
    public float raycastDistance = 5f; // Max distance to check for the ground
    public Vector2 rayOffset = Vector2.zero; // Offset for raycast (e.g., center of character)

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        SnapToGroundPosition();
    }

    void SnapToGroundPosition()
    {
        // Perform a raycast downward from the character's position + offset
        Vector2 rayOrigin = (Vector2)transform.position + rayOffset;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, raycastDistance);

        if (hit.collider != null && hit.collider.CompareTag(groundTag))
        {
            // Snap the character to the ground
            Vector3 newPosition = transform.position;
            newPosition.y = hit.point.y;
            transform.position = newPosition;

            // Freeze the Rigidbody's velocity to avoid unintended movement
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            Debug.LogWarning("No ground detected below the character.");
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw the raycast in the Scene view for debugging
        Vector2 rayOrigin = (Vector2)transform.position + rayOffset;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.down * raycastDistance);
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

public class LadderLogic : MonoBehaviour
{
    [SerializeField]
    private float climbSpeed = 3f; // Adjust climb speed as needed

    // Reference to the "move" Move that returns a Vector2 (with up and down bindings)
    [SerializeField]
    private InputSystem controls;

    private bool isOnLadder = false;
    private Rigidbody2D rb;
    private float originalGravity;

    private void Awake()
    {

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("LadderLogic requires a Rigidbody2D component on the GameObject.");
        }
        originalGravity = rb.gravityScale;
    }

    private void OnEnable()
    {
        if (controls != null)
        {
            controls.Player.Move.Enable();
        }
    }

    private void OnDisable()
    {
        if (controls != null)
        {
            controls.Player.Move.Disable();
        }
    }

    private void Update()
    {
        if (isOnLadder)
        {
            // Disable gravity while on the ladder
            rb.gravityScale = 0f;

            // Read the vertical input from the move Move (W/S keys)
            float verticalInput = controls.Player.Move.ReadValue<Vector2>().y;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalInput * climbSpeed);
        }
        else
        {
            // Restore original gravity when not climbing
            rb.gravityScale = originalGravity;
        }
    }

    // Trigger when entering a ladder area (ensure ladder objects have a Collider2D with "Is Trigger" checked)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isOnLadder = true;
            // Optionally, reset vertical velocity upon entering the ladder
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
    }

    // Trigger when leaving a ladder area
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isOnLadder = false;
        }
    }
}

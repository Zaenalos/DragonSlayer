using UnityEngine;

public class FloatingAnimate : MonoBehaviour
{
    // Adjust these values to control the float amplitude and speed.
    public float amplitude = 0.1f; // Vertical movement in world units.
    public float frequency = 1f;   // Speed of oscillation.

    private Vector3 startPos;

    void Start()
    {
        // Record the starting position of the button.
        startPos = transform.position;
    }

    void Update()
    {
        // Calculate the new vertical position using a sine wave.
        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}

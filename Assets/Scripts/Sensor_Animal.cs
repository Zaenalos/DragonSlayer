using UnityEngine;
using System.Collections;

public class Sensor_Animal : MonoBehaviour
{
    [SerializeField] private string groundTag = "Ground"; // Set the tag for ground objects. private bool isGrounded;
    [SerializeField] private bool isGrounded = false;
    public bool State() => isGrounded;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(groundTag))
            isGrounded = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(groundTag))
            isGrounded = false;
    }
}
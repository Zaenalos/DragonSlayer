using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    public float parallaxEffect;
    public GameObject cam;

    private float startPos, length;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float distance = (cam.transform.position.x * parallaxEffect); // 0 = background moves at the same speed as the camera, 1 = background moves at the same speed as the camera
        float movement = (cam.transform.position.x * (1 - parallaxEffect)); // 0 = background doesn't move, 1 = background moves at the same speed as the camera
        transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);

        // If the background is out of the camera view, move it back to the start position
        if (movement > startPos + length)
        {
            startPos += length;
        }
        else if (movement < startPos - length)
        {
            startPos -= length;
        }

    }
}

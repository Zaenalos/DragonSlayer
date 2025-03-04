using UnityEngine;

public class RespawnLogic : MonoBehaviour
{
    private Vector2 spawnPoint = new Vector2(4.1f, -4.5f);
    private GameObject playerObject;

    private void Awake()
    {
        playerObject = gameObject;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Respawn"))
        {
            Vector3 position = collision.transform.position;
            spawnPoint = new Vector2(position.x, position.y);
        }
    }

    public void Respawn()
    {
        // Get HeroKnight's position and set it to the spawnPoint
        // Reset the player's health also to 100
        playerObject.GetComponent<HeroKnight>().ResetHealth();
        playerObject.transform.position = new Vector3(spawnPoint.x, spawnPoint.y, playerObject.transform.position.z);
    }
}

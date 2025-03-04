using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<RespawnLogic>()?.Respawn();
        }
        else if (collision.CompareTag("Enemy"))
        {
            Debug.Log("Enemy destroyed");
            Destroy(collision.gameObject);
        }
    }
}

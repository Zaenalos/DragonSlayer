using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField] private GameObject playerObject;
    private HeroKnight player;

    private void Awake()
    {
        if (playerObject == null)
            playerObject = GameObject.Find("HeroKnight");

        player = playerObject.GetComponent<HeroKnight>();
    }
    // Make a death trap logic.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Kill the player.
            player.DieTrap();
        }
    }
}

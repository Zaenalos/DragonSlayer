using UnityEngine;

public class SongTrigger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag the player GameObject here.")]
    public GameObject plauer; // Consider renaming to 'player'
    [Tooltip("Drag the MusicBG GameObject here, which has an AudioSource.")]
    public GameObject musicBG;
    [Tooltip("Assign the new AudioClip to play when the player enters the trigger.")]
    public AudioClip newSong;

    private bool isTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the trigger has already been triggered
        if (isTriggered)
        {
            return;
        }
        // Check if the colliding object is the designated player
        if (collision.gameObject == plauer)
        {
            AudioSource audioSource = musicBG.GetComponent<AudioSource>();
            if (audioSource != null && newSong != null)
            {
                // Change the audio clip and play it
                audioSource.clip = newSong;
                audioSource.Play();
                isTriggered = true;
            }
            else
            {
                Debug.LogWarning("AudioSource or newSong is missing on MusicBG!");
            }
        }
    }
}

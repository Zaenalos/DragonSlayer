using UnityEngine;

public class DialogPopup : MonoBehaviour
{
    // Reference to the DialogueBubble GameObject that should be shown.
    [SerializeField] private GameObject dialogueBubble;

    // Name of the child GameObject (DialogueText) inside the dialogue bubble.
    [SerializeField] private string dialogueTextChildName = "DialogueText";

    // Called when another collider enters the trigger attached to this GameObject.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Activate the dialogue bubble (i.e., "itself")
            if (dialogueBubble != null)
            {
                dialogueBubble.SetActive(true);

                // Find and activate its child DialogueText
                Transform dialogueText = dialogueBubble.transform.Find(dialogueTextChildName);
                if (dialogueText != null)
                {
                    dialogueText.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("Child '" + dialogueTextChildName + "' not found under " + dialogueBubble.name);
                }
            }
            else
            {
                Debug.LogWarning("DialogueBubble reference is not set on " + gameObject.name);
            }
        }
    }

    // Optional: Hide the dialogue when the player leaves the trigger area.
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (dialogueBubble != null)
            {
                dialogueBubble.SetActive(false);
            }
        }
    }
}

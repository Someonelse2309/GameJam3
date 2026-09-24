using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialog Settings")]
    public bool autoTriggerOnApproach = true;

    [Header("Dialog Content")]
    public DialogueSentence[] dialogueSentences;

    private bool isPlayerNearby = false;
    private bool hasTriggered = false;

    void Update()
    {
        if (!autoTriggerOnApproach && isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            StartDialogueSequence();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (autoTriggerOnApproach && !hasTriggered)
            {
                StartDialogueSequence();
                hasTriggered = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            hasTriggered = false;
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.EndDialogue();
            }
        }
    }

    public void StartDialogueSequence()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogueSentences);
        }
    }
}
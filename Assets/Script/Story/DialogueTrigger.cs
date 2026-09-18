using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueData dialogueData;

    [Header("Settings")]
    public bool triggerOnce = true;
    public bool deactivateAfterTrigger = false;

    [Header("AutoWalk - Character yang auto jalan")]
    public AutoWalk autoWalkTarget; // AutoWalk yang akan dipanggil

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && CanTrigger())
        {
            Debug.Log("DialogueTrigger: Player masuk trigger - " + gameObject.name);
            TriggerDialogue();
        }
    }

    public void TriggerDialogue()
    {
        if (CanTrigger())
        {
            Debug.Log("DialogueTrigger: Triggering dialogue - " + (dialogueData != null ? dialogueData.name : "NULL"));

            hasTriggered = true;

            if (deactivateAfterTrigger)
                gameObject.SetActive(false);

            // Start AutoWalk kalau ada
            if (autoWalkTarget != null)
            {
                Debug.Log("DialogueTrigger: Starting AutoWalk - " + autoWalkTarget.gameObject.name);
                autoWalkTarget.StartWalking();
            }

            // Start Dialogue
            if (DialogueManager.instance == null)
            {
                Debug.LogError("DialogueTrigger ERROR: DialogueManager.instance NULL!");
                return;
            }

            if (dialogueData == null)
            {
                Debug.LogError("DialogueTrigger ERROR: dialogueData NULL! Assign Dialogue Data asset ke DialogueTrigger di Inspector.");
                return;
            }

            DialogueManager.instance.StartDialogue(dialogueData);
        }
    }

    private bool CanTrigger()
    {
        if (triggerOnce && hasTriggered)
        {
            Debug.Log("DialogueTrigger: Sudah pernah trigger, skip.");
            return false;
        }
        if (DialogueManager.instance != null && DialogueManager.instance.IsDialogueActive())
        {
            Debug.Log("DialogueTrigger: Dialogue sedang aktif, skip.");
            return false;
        }

        return true;
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
        gameObject.SetActive(true);
    }
}

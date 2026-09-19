using UnityEngine;

public class DialogueTriggerEP1 : MonoBehaviour
{
    public DialogueDataEP1 dialogueData;

    [Header("Settings")]
    public bool triggerOnce = true;
    public bool deactivateAfterTrigger = false;

    [Header("AutoWalk - Character yang auto jalan")]
    public AutoWalk autoWalkTarget;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && CanTrigger())
        {
            Debug.Log("DialogueTriggerEP1: Player masuk trigger - " + gameObject.name);
            TriggerDialogue();
        }
    }

    public void TriggerDialogue()
    {
        if (CanTrigger())
        {
            Debug.Log("DialogueTriggerEP1: Triggering dialogue - " + (dialogueData != null ? dialogueData.name : "NULL"));

            hasTriggered = true;

            if (deactivateAfterTrigger)
                gameObject.SetActive(false);

            if (autoWalkTarget != null)
            {
                Debug.Log("DialogueTriggerEP1: Starting AutoWalk - " + autoWalkTarget.gameObject.name);
                autoWalkTarget.StartWalking();
            }

            if (DialogueManagerEP1.instanceEP1 == null)
            {
                Debug.LogError("DialogueTriggerEP1 ERROR: DialogueManagerEP1.instanceEP1 NULL!");
                return;
            }

            if (dialogueData == null)
            {
                Debug.LogError("DialogueTriggerEP1 ERROR: dialogueData NULL!");
                return;
            }

            DialogueManagerEP1.instanceEP1.StartDialogue(dialogueData);
        }
    }

    private bool CanTrigger()
    {
        if (triggerOnce && hasTriggered)
        {
            Debug.Log("DialogueTriggerEP1: Sudah pernah trigger, skip.");
            return false;
        }
        if (DialogueManagerEP1.instanceEP1 != null && DialogueManagerEP1.instanceEP1.IsDialogueActive())
        {
            Debug.Log("DialogueTriggerEP1: Dialogue sedang aktif, skip.");
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

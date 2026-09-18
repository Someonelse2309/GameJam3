using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialog Settings")]
    public bool autoTriggerOnApproach = true; // Centang di Inspector agar otomatis mulai saat mendekat

    [Header("Dialog T.1 Tanaka Koji")]
    public DialogueSentence[] dialogueSentences = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "Well well well, look what we have here" },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "鬼殺し (Onikoroshi)…. The ghost slayer…." },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "I thought you left this life behind…." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "I need your help…." },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "Why should I? You should just go home an…." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "You owe me one, Remember Nagoya?" },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "I remember…" },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "But I’m hungry…" },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "Get me something to eat, and we’ll talk…" }
    };

    private bool isPlayerNearby = false;
    private bool hasTriggered = false;

    void Update()
    {
        // Jika tidak auto trigger, player bisa tekan E atau tap untuk panggil dialog
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

            // Otomatis jalankan dialog saat player mendekat masuk area collider
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
            hasTriggered = false; // Reset trigger ketika player menjauh

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
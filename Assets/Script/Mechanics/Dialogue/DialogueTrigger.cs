using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
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

    void Update()
    {
        // Tekan 'E' saat di dekat NPC untuk lanjut/mulai dialog
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(dialogueSentences);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (DialogueManager.Instance != null) DialogueManager.Instance.EndDialogue();
        }
    }
}
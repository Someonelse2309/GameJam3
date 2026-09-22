using UnityEngine;

public class PedagangTrigger : MonoBehaviour
{
    [Header("References")]
    public DialogueManager dialogueManager;
    public BeggarTrigger beggarTrigger;
    public ItemData yakitoriItem;

    private bool isPlayerNearby = false;

    [Header("Dialogues")]
    // Dialog jika belum bicara dengan Beggar
    public DialogueSentence[] dialogueBelumMinta = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Pedagang", sentence = "Welcome! Fresh yakitori made with the finest recipe!" },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "I don't have any appetite right now..." }
    };

    // Dialog T.1 PT1: Saat disuruh Beggar cari makanan
    public DialogueSentence[] dialogueBeliYakitori = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Give me two portions of yakitori." },
        new DialogueSentence { speakerName = "Pedagang", sentence = "Here is the yakitori." }
    };

    // Dialog jika sudah bawa Yakitori di tas
    public DialogueSentence[] dialogueSudahBeli = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Pedagang", sentence = "Eat it while it's still warm! Don't let it get cold." }
    };

    private void Start()
    {
        if (dialogueManager == null)
            dialogueManager = DialogueManager.Instance != null ? DialogueManager.Instance : FindFirstObjectByType<DialogueManager>();

        if (beggarTrigger == null)
            beggarTrigger = FindFirstObjectByType<BeggarTrigger>();
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            TriggerInteraction();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            TriggerInteraction();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            // Tutup dialog seketika saat MC menjauh
            if (dialogueManager != null && dialogueManager.isDialogueActive)
            {
                dialogueManager.CancelDialogue();
            }
        }
    }

    public void TriggerInteraction()
    {
        if (dialogueManager == null || dialogueManager.isDialogueActive) return;

        // 1. Cek apakah sudah disuruh oleh Beggar
        bool isBeggarQuestActive = beggarTrigger != null && beggarTrigger.currentState == BeggarTrigger.QuestState.LookingForFood;
        bool hasYakitori = InventoryManager.Instance != null && yakitoriItem != null && InventoryManager.Instance.HasItem(yakitoriItem);

        if (!isBeggarQuestActive && beggarTrigger != null && beggarTrigger.currentState == BeggarTrigger.QuestState.NotStarted)
        {
            // Belum ketemu Beggar -> Pedagang hanya menyapa biasa, tidak kasih item
            dialogueManager.StartDialogue(dialogueBelumMinta);
        }
        else if (isBeggarQuestActive && !hasYakitori)
        {
            // Sedang disuruh Beggar dan belum punya Yakitori -> Beli Yakitori
            dialogueManager.StartDialogue(dialogueBeliYakitori, () =>
            {
                if (InventoryManager.Instance != null && yakitoriItem != null)
                {
                    InventoryManager.Instance.AddItem(yakitoriItem);
                }

                // Picu Yakuza muncul di samping Beggar tepat saat Yakitori selesai dibeli
                if (beggarTrigger != null)
                {
                    beggarTrigger.SpawnYakuzaBesideBeggar();
                }
            });
        }
        else
        {
            // Sudah punya Yakitori di tas atau quest sudah lewat
            dialogueManager.StartDialogue(dialogueSudahBeli);
        }
    }
}
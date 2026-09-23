using UnityEngine;

public class PedagangTrigger : MonoBehaviour
{
    [Header("Dependencies")]
    public BeggarTrigger beggarTrigger;
    public ItemData yakitoriItem;
    public DialogueManager dialogueManager;

    [Header("Quest Indicator")]
    [Tooltip("Tarik child QuestBubble milik Pedagang ke sini")]
    public QuestIndicator questIndicator;

    [Header("Dialogue (Beli/Minta Yakitori)")]
    public DialogueSentence[] dialoguePT1 = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Give me two portion of yakitori." },
        new DialogueSentence { speakerName = "Seller", sentence = "Here is the yakitori." }
    };

    [Header("Dialogue Yapping (Sebelum / Sesudah Quest)")]
    public DialogueSentence[] idleDialogue = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Seller", sentence = "Fresh skewers every day! Watch out for the Yakuza around here." }
    };

    private void Start()
    {
        if (dialogueManager == null)
            dialogueManager = DialogueManager.Instance != null ? DialogueManager.Instance : FindFirstObjectByType<DialogueManager>();

        if (beggarTrigger == null)
            beggarTrigger = FindFirstObjectByType<BeggarTrigger>();

        RefreshIndicator();
    }

    private void Update()
    {
        RefreshIndicator();
    }

    private void RefreshIndicator()
    {
        if (questIndicator == null) return;

        if (dialogueManager != null && dialogueManager.isDialogueActive)
        {
            questIndicator.SetVisible(false);
            return;
        }

        // Syarat bubble Pedagang aktif: Beggar butuh makanan & player belum punya makanannya
        bool beggarNeedsFood = beggarTrigger != null && beggarTrigger.currentState == BeggarTrigger.QuestState.LookingForFood;
        bool hasYakitori = InventoryManager.Instance != null && yakitoriItem != null && InventoryManager.Instance.HasItem(yakitoriItem);

        if (beggarNeedsFood && !hasYakitori)
        {
            questIndicator.SetVisible(true);
        }
        else
        {
            questIndicator.SetVisible(false); // Mode yapping biasa
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            TriggerPedagangInteraction();
        }
    }

    public void TriggerPedagangInteraction()
    {
        if (dialogueManager == null || dialogueManager.isDialogueActive) return;

        bool beggarNeedsFood = beggarTrigger != null && beggarTrigger.currentState == BeggarTrigger.QuestState.LookingForFood;
        bool hasYakitori = InventoryManager.Instance != null && yakitoriItem != null && InventoryManager.Instance.HasItem(yakitoriItem);

        if (beggarNeedsFood && !hasYakitori)
        {
            // Ambil yakitori untuk Beggar
            dialogueManager.StartDialogue(dialoguePT1, () =>
            {
                if (InventoryManager.Instance != null && yakitoriItem != null)
                {
                    InventoryManager.Instance.AddItem(yakitoriItem);
                }
                RefreshIndicator();
            });
        }
        else
        {
            // Yapping biasa
            dialogueManager.StartDialogue(idleDialogue);
        }
    }
}
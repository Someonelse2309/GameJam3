using UnityEngine;

public class BeggarTrigger : MonoBehaviour
{
    [Header("Manager References")]
    public DialogueManager dialogueManager;
    public ItemData yakitoriItem;
    public YakuzaEnemy yakuzaTarget; // Drag GameObject Yakuza botak ke sini

    public enum QuestState { NotStarted, LookingForFood, YakuzaFight, QuestCompleted }
    [Header("Quest State")]
    public QuestState currentState = QuestState.NotStarted;

    private bool isPlayerNearby = false;
    private Transform playerTransform;

    // Dialogue PT1: Pertemuan awal
    public DialogueSentence[] dialoguePT1 = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "Well well well, look what we have here" },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "Onikoroshi.... The ghost slayer...." },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "I thought you left this life behind...." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "I need your help...." },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "Why should I? You should just go home and...." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "You owe me one, Remember Nagoya?" },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "I remember...." },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "But I'm hungry...." },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "Get me something to eat, and we'll talk...." }
    };

    // Dialogue PT2: Datang Yakuza memukuli Beggar
    public DialogueSentence[] dialoguePT2 = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Bald Yakuza", sentence = "You know the rule, no talking about the boss." },
        new DialogueSentence { speakerName = "Bald Yakuza", sentence = "Last warning is your last warning, now you have to go." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "TANAKA!" },
        new DialogueSentence { speakerName = "Bald Yakuza", sentence = "Stay away from this, your debt has been settled." }
    };

    // Dialogue PT3: Beggar berterima kasih & beri clue Blacksmith
    public DialogueSentence[] dialoguePT3 = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "That was close... And I'm still hungry..." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Here's your yakitori." },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "What a feast!" },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Now start talking." },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "Aoyama, the one who took away your child. He's here." },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "But you need a weapon." },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "Go to Ito Shun, He's the last blacksmith in town." },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "He's on the further side of the town, Look for him in his house." },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "and beware, The Yakuza knows you're here for revenge." }
    };

    private void Start()
    {
        if (dialogueManager == null)
            dialogueManager = DialogueManager.Instance != null ? DialogueManager.Instance : FindFirstObjectByType<DialogueManager>();

        if (yakuzaTarget != null)
        {
            yakuzaTarget.gameObject.SetActive(false); // Sembunyikan Yakuza sebelum quest Yakitori selesai
            CharacterHealth yakuzaHealth = yakuzaTarget.GetComponent<CharacterHealth>();
            if (yakuzaHealth != null)
            {
                yakuzaHealth.OnDeath += OnYakuzaDefeated;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            playerTransform = collision.transform;
            TriggerInteraction();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerNearby = false;
    }

    public void TriggerInteraction()
    {
        if (dialogueManager == null || dialogueManager.isDialogueActive) return;

        bool hasYakitori = InventoryManager.Instance != null && yakitoriItem != null && InventoryManager.Instance.HasItem(yakitoriItem);

        // State 1: Awal game
        if (currentState == QuestState.NotStarted)
        {
            dialogueManager.StartDialogue(dialoguePT1, () =>
            {
                currentState = QuestState.LookingForFood;
            });
        }
        // State 2: Kembali bawa Yakitori -> Mulai Dialogue PT2 (Yakuza menghadang)
        else if (currentState == QuestState.LookingForFood && hasYakitori)
        {
            dialogueManager.StartDialogue(dialoguePT2, () =>
            {
                // Mulai mode bertarung
                currentState = QuestState.YakuzaFight;
                if (yakuzaTarget != null)
                {
                    yakuzaTarget.StartCombat(playerTransform);
                }
            });
        }
        else if (currentState == QuestState.LookingForFood && !hasYakitori)
        {
            DialogueSentence[] remind = new DialogueSentence[]
            {
                new DialogueSentence { speakerName = "Tanaka Koji", sentence = "I'm still hungry... Get me that yakitori from the food cart!" }
            };
            dialogueManager.StartDialogue(remind);
        }
        else if (currentState == QuestState.QuestCompleted)
        {
            DialogueSentence[] completed = new DialogueSentence[]
            {
                new DialogueSentence { speakerName = "Tanaka Koji", sentence = "Go find Ito Shun on the other side of town." }
            };
            dialogueManager.StartDialogue(completed);
        }
    }

    private void OnYakuzaDefeated()
    {
        // Serahkan yakitori & hapus dari tas
        if (InventoryManager.Instance != null && yakitoriItem != null)
        {
            InventoryManager.Instance.RemoveItem(yakitoriItem);
        }

        currentState = QuestState.QuestCompleted;

        // Lanjut ke Dialogue PT3
        if (dialogueManager != null)
        {
            dialogueManager.StartDialogue(dialoguePT3);
        }
    }
}
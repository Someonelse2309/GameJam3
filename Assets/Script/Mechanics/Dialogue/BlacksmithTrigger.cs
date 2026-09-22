using System.Collections;
using UnityEngine;

public class BlacksmithTrigger : MonoBehaviour
{
    [Header("Quest Dependencies")]
    public BeggarTrigger beggarTrigger;
    public ItemData katanaItem;
    public DialogueManager dialogueManager;

    [Header("Yakuza Wave (T.3)")]
    public YakuzaEnemy[] yakuzaEnemies; // Masukkan 3 Yakuza
    private int defeatedYakuzaCount = 0;
    private bool combatStarted = false;

    public enum BlacksmithState { Locked, ReadyForTalk, InCombat, CombatFinished, QuestCompleted }
    [Header("State")]
    public BlacksmithState currentState = BlacksmithState.Locked;

    private bool isPlayerNearby = false;
    private Transform playerTransform;

    [Header("Dialogue PT1 (Datang ke Blacksmith & Yakuza Datang)")]
    public DialogueSentence[] dialoguePT1 = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Ito Shun?" },
        new DialogueSentence { speakerName = "Ito Shun", sentence = "I know you, The Yakuza are asking around about you." },
        new DialogueSentence { speakerName = "Ito Shun", sentence = "I know what you're looking for and I have it somewhere here." },
        new DialogueSentence { speakerName = "Ito Shun", sentence = "But I need time to look for it, Wait here." },
        new DialogueSentence { speakerName = "Suit Yakuza", sentence = "Michelle Sato! You better stop whatever you're doing here." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "I'm here for revenge and I won't go without it." },
        new DialogueSentence { speakerName = "Suit Yakuza", sentence = "Then we have no choice but to remove you." }
    };

    [Header("Dialogue PT2 (Setelah Kalahkan Yakuza & Dapat Katana)")]
    public DialogueSentence[] dialoguePT2 = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Suit Yakuza", sentence = "Aoyama will get you" },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Send him my regards" },
        new DialogueSentence { speakerName = "Ito Shun", sentence = "The legend was true, you're the ghostslayer." },
        new DialogueSentence { speakerName = "Ito Shun", sentence = "Here's your katana, Go get your revenge." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Thank you Ito Shun." },
        new DialogueSentence { speakerName = "Ito Shun", sentence = "Be careful." }
    };

    private void Start()
    {
        if (dialogueManager == null)
            dialogueManager = DialogueManager.Instance != null ? DialogueManager.Instance : FindFirstObjectByType<DialogueManager>();

        if (beggarTrigger == null)
            beggarTrigger = FindFirstObjectByType<BeggarTrigger>();

        // Pastikan 3 Yakuza nonaktif di awal
        if (yakuzaEnemies != null)
        {
            foreach (var yakuza in yakuzaEnemies)
            {
                if (yakuza != null)
                {
                    yakuza.gameObject.SetActive(false);
                    CharacterHealth hp = yakuza.GetComponent<CharacterHealth>();
                    if (hp != null) hp.OnDeath += OnYakuzaKilled;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            playerTransform = collision.transform;
            TriggerBlacksmithInteraction();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }

    public void TriggerBlacksmithInteraction()
    {
        if (dialogueManager == null || dialogueManager.isDialogueActive) return;

        // Cek apakah quest Beggar sudah selesai
        bool beggarFinished = beggarTrigger != null && beggarTrigger.currentState == BeggarTrigger.QuestState.QuestCompleted;

        if (!beggarFinished)
        {
            DialogueSentence[] busyDialog = new DialogueSentence[]
            {
                new DialogueSentence { speakerName = "Ito Shun", sentence = "I'm busy right now. Come back later." }
            };
            dialogueManager.StartDialogue(busyDialog);
            return;
        }

        if (currentState == BlacksmithState.Locked || currentState == BlacksmithState.ReadyForTalk)
        {
            // Mulai Dialogue PT1
            dialogueManager.StartDialogue(dialoguePT1, () =>
            {
                StartCombatWave();
            });
        }
        else if (currentState == BlacksmithState.CombatFinished)
        {
            // Mulai Dialogue PT2 & Berikan Katana
            dialogueManager.StartDialogue(dialoguePT2, () =>
            {
                currentState = BlacksmithState.QuestCompleted;
                if (InventoryManager.Instance != null && katanaItem != null)
                {
                    InventoryManager.Instance.AddItem(katanaItem); // Masuk ke Tas
                }
            });
        }
    }

    private void StartCombatWave()
    {
        currentState = BlacksmithState.InCombat;
        combatStarted = true;

        if (yakuzaEnemies != null)
        {
            foreach (var yakuza in yakuzaEnemies)
            {
                if (yakuza != null)
                {
                    yakuza.gameObject.SetActive(true);
                    yakuza.StartCombat(playerTransform);
                }
            }
        }
    }

    private void OnYakuzaKilled()
    {
        defeatedYakuzaCount++;

        // Jika 2 Yakuza tumbang, 1 Yakuza terakhir kabur sesuai storyboard
        if (defeatedYakuzaCount >= 2 && combatStarted)
        {
            combatStarted = false;
            StartCoroutine(HandleLastYakuzaFlee());
        }
    }

    private IEnumerator HandleLastYakuzaFlee()
    {
        // Cari Yakuza yang masih hidup untuk kabur
        foreach (var yakuza in yakuzaEnemies)
        {
            if (yakuza != null && yakuza.gameObject.activeSelf)
            {
                CharacterHealth hp = yakuza.GetComponent<CharacterHealth>();
                if (hp != null && !hp.isDead)
                {
                    // Efek kabur (matikan AI lalu menghilang)
                    yakuza.enabled = false;
                    yakuza.gameObject.SetActive(false);
                    break;
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        currentState = BlacksmithState.CombatFinished;

        // Otomatis jalankan dialog penyerahan senjata
        TriggerBlacksmithInteraction();
    }
}
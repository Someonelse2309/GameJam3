using System.Collections;
using UnityEngine;

public class BeggarTrigger : MonoBehaviour
{
    public enum QuestState { NotStarted, LookingForFood, YakuzaEncounter, CombatFinished, QuestCompleted }

    [Header("Quest State")]
    public QuestState currentState = QuestState.NotStarted;

    [Header("Manager References")]
    public DialogueManager dialogueManager;
    public ItemData yakitoriItem;
    public YakuzaEnemy yakuzaTarget;

    [Header("Quest Indicator")]
    [Tooltip("Tarik child QuestBubble milik Beggar ke sini")]
    public QuestIndicator questIndicator;

    [Header("Dialogue PT1 (Pertemuan Awal)")]
    public DialogueSentence[] dialoguePT1;

    [Header("Dialogue PT2 (Yakuza Memalak)")]
    public DialogueSentence[] dialoguePT2;

    [Header("Dialogue PT3 (Selesai Kalahkan Yakuza)")]
    public DialogueSentence[] dialoguePT3;

    private bool isPlayerNearby = false;
    private bool inCombat = false;

    private void Start()
    {
        if (dialogueManager == null)
            dialogueManager = DialogueManager.Instance != null ? DialogueManager.Instance : FindFirstObjectByType<DialogueManager>();

        // Sembunyikan Yakuza di awal permainan
        if (yakuzaTarget != null)
        {
            yakuzaTarget.gameObject.SetActive(false);
            CharacterHealth hp = yakuzaTarget.GetComponent<CharacterHealth>();
            if (hp != null) hp.OnDeath += OnYakuzaDefeated;
        }

        RefreshIndicator();
    }

    private void Update()
    {
        CheckYakuzaArrival();
        RefreshIndicator();
    }

    // Memeriksa apakah makanan sudah didapat agar Yakuza langsung spawn memalak Beggar dari kejauhan
    private void CheckYakuzaArrival()
    {
        if (currentState == QuestState.LookingForFood)
        {
            bool hasYakitori = InventoryManager.Instance != null && yakitoriItem != null && InventoryManager.Instance.HasItem(yakitoriItem);

            if (hasYakitori && yakuzaTarget != null && !yakuzaTarget.gameObject.activeSelf)
            {
                yakuzaTarget.gameObject.SetActive(true);
                yakuzaTarget.enabled = true; // Memutar animasi idle

                // Balik sprite agar Yakuza selalu menghadap ke Beggar
                SpriteRenderer yakuzaSr = yakuzaTarget.GetComponent<SpriteRenderer>();
                if (yakuzaSr != null)
                {
                    yakuzaSr.flipX = yakuzaTarget.transform.position.x > transform.position.x;
                }
            }
        }
    }

    private void RefreshIndicator()
    {
        if (questIndicator == null) return;

        if ((dialogueManager != null && dialogueManager.isDialogueActive) || inCombat)
        {
            questIndicator.SetVisible(false);
            return;
        }

        bool hasYakitori = InventoryManager.Instance != null && yakitoriItem != null && InventoryManager.Instance.HasItem(yakitoriItem);

        if (currentState == QuestState.NotStarted)
        {
            questIndicator.SetVisible(true);
        }
        else if (currentState == QuestState.LookingForFood)
        {
            // Bubble menyala di Beggar hanya jika makanan sudah di tangan
            questIndicator.SetVisible(hasYakitori);
        }
        else if (currentState == QuestState.CombatFinished)
        {
            questIndicator.SetVisible(true);
        }
        else
        {
            questIndicator.SetVisible(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            TriggerBeggarInteraction();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;

            // Jika player cuma lewat lalu menjauh, tutup dialog dan batalkan trigger quest
            if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive && !DialogueManager.Instance.isEngaged)
            {
                DialogueManager.Instance.CancelDialogue();
            }
        }
    }

    public void TriggerBeggarInteraction()
    {
        if (dialogueManager == null || dialogueManager.isDialogueActive || inCombat) return;

        bool hasYakitori = InventoryManager.Instance != null && yakitoriItem != null && InventoryManager.Instance.HasItem(yakitoriItem);

        if (currentState == QuestState.NotStarted)
        {
            dialogueManager.StartDialogue(dialoguePT1, () =>
            {
                currentState = QuestState.LookingForFood;
                RefreshIndicator();
            });
        }
        else if (currentState == QuestState.LookingForFood)
        {
            if (!hasYakitori)
            {
                DialogueSentence[] hungryDialog = new DialogueSentence[]
                {
                    new DialogueSentence { speakerName = "Tanaka Koji", sentence = "I'm still hungry... Get me something to eat first." }
                };
                dialogueManager.StartDialogue(hungryDialog);
            }
            else
            {
                // Player tiba membawa makanan saat Yakuza sedang memalak
                currentState = QuestState.YakuzaEncounter;

                dialogueManager.StartDialogue(dialoguePT2, () =>
                {
                    StartBeggarCombat();
                });
            }
        }
        else if (currentState == QuestState.CombatFinished)
        {
            dialogueManager.StartDialogue(dialoguePT3, () =>
            {
                currentState = QuestState.QuestCompleted;

                if (InventoryManager.Instance != null && yakitoriItem != null)
                {
                    InventoryManager.Instance.RemoveItem(yakitoriItem);
                }

                RefreshIndicator();
            });
        }
        else if (currentState == QuestState.QuestCompleted)
        {
            DialogueSentence[] completedDialog = new DialogueSentence[]
            {
                new DialogueSentence { speakerName = "Tanaka Koji", sentence = "Go find Ito Shun. He will forge your path." }
            };
            dialogueManager.StartDialogue(completedDialog);
        }
    }

    private void StartBeggarCombat()
    {
        inCombat = true;

        if (yakuzaTarget != null)
        {
            yakuzaTarget.enabled = true;

            Transform playerTransform = null;
            if (PlayerMovement.Instance != null)
            {
                playerTransform = PlayerMovement.Instance.transform;
            }
            else
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerTransform = player.transform;
            }

            if (playerTransform != null)
            {
                yakuzaTarget.StartCombat(playerTransform);
            }
        }
    }

    private void OnYakuzaDefeated()
    {
        inCombat = false;
        currentState = QuestState.CombatFinished;
        StartCoroutine(PostCombatDialogueDelay());
    }

    private IEnumerator PostCombatDialogueDelay()
    {
        yield return new WaitForSeconds(0.6f);
        TriggerBeggarInteraction();
    }
}
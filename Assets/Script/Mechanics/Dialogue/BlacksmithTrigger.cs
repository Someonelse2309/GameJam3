using System.Collections;
using UnityEngine;

public class BlacksmithTrigger : MonoBehaviour
{
    [Header("Quest Dependencies")]
    public BeggarTrigger beggarTrigger;
    public ItemData katanaItem;
    public DialogueManager dialogueManager;

    [Header("Quest Indicator (Bubble Atas Kepala)")]
    public QuestIndicator questIndicator;

    [Header("Guiding Bubble (Pinggir Layar)")]
    public GameObject arrowToBlacksmith;

    [Header("Yakuza Enemies (Petarung)")]
    public YakuzaEnemy[] fighterYakuza; 

    [Header("Yakuza Leader (Pengamat Jas Rapi)")]
    public GameObject suitYakuzaLeader;
    public float leaderFleeSpeed = 6f;

    public enum BlacksmithState { Locked, ReadyForTalk, InCombat, CombatFinished, QuestCompleted }
    [Header("State")]
    public BlacksmithState currentState = BlacksmithState.Locked;

    private int defeatedCount = 0;
    private bool combatStarted = false;
    private bool isPlayerNearby = false;
    private Transform playerTransform;

    [Header("Dialogue PT1 (Sebelum Berantem)")]
    public DialogueSentence[] dialoguePT1 = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Ito Shun?" },
        new DialogueSentence { speakerName = "Ito Shun", sentence = "Michelle Sato... The ghost slayer. The Yakuza are tearing the city apart looking for you." },
        new DialogueSentence { speakerName = "Ito Shun", sentence = "I have what you seek, but I won't hand a deadly blade to someone who's just looking for a grave." },
        new DialogueSentence { speakerName = "Suit Yakuza", sentence = "Well, well. Michelle Sato cornered like a rat, and without a weapon!" },
        new DialogueSentence { speakerName = "Suit Yakuza", sentence = "Boys, break her limbs! The boss wants her breathing, but broken." },
        new DialogueSentence { speakerName = "Ito Shun", sentence = "Show me your resolve, Sato! Prove to me that the 'Onikoroshi' is still burning inside you!" },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "I don't need a blade to crush your dogs." }
    };

    [Header("Dialogue PT2 (Setelah Kalahkan 2 Yakuza Botak)")]
    public DialogueSentence[] dialoguePT2 = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Suit Yakuza", sentence = "W-what the hell?! Barehanded...?! You're a monster!" },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Tell Aoyama I'm coming. Send him my regards." },
        new DialogueSentence { speakerName = "Suit Yakuza", sentence = "Aoyama-sama will tear you apart! This isn't over!" },
        new DialogueSentence { speakerName = "Ito Shun", sentence = "The legend was true... Even without steel, your edge never dulled." },
        new DialogueSentence { speakerName = "Ito Shun", sentence = "Here’s your katana, Onikoroshi. Go reclaim what was taken from you." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Thank you, Ito Shun." },
        new DialogueSentence { speakerName = "Ito Shun", sentence = "Be careful. Aoyama won't fight fair." }
    };

    [Header("Dialogue Encouragement (Setelah Quest Selesai)")]
    public DialogueSentence[] dialogueCompleted = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Ito Shun", sentence = "What are you waiting for, Sato? Go find Aoyama and take your child back!" },
        new DialogueSentence { speakerName = "Ito Shun", sentence = "Make them remember why they feared the name 'Onikoroshi'. Don't look back!" }
    };

    private void Start()
    {
        if (dialogueManager == null)
            dialogueManager = DialogueManager.Instance != null ? DialogueManager.Instance : FindFirstObjectByType<DialogueManager>();

        if (beggarTrigger == null)
            beggarTrigger = FindFirstObjectByType<BeggarTrigger>();

        FindPlayerTransform();
        HideYakuzaAmbush();

        if (fighterYakuza != null)
        {
            foreach (var yakuza in fighterYakuza)
            {
                if (yakuza != null)
                {
                    CharacterHealth hp = yakuza.GetComponent<CharacterHealth>();
                    if (hp != null) hp.OnDeath += OnFighterKilled;
                }
            }
        }

        RefreshIndicator();
    }

    private void Update()
    {
        RefreshIndicator();
    }

    private void RefreshIndicator()
    {
        bool beggarFinished = beggarTrigger != null && beggarTrigger.currentState == BeggarTrigger.QuestState.QuestCompleted;
        bool isDialogActive = (dialogueManager != null && dialogueManager.isDialogueActive);

        // 1. Bubble di atas kepala Blacksmith
        if (questIndicator != null)
        {
            if (isDialogActive || currentState == BlacksmithState.InCombat || currentState == BlacksmithState.QuestCompleted)
            {
                questIndicator.SetVisible(false);
            }
            else if ((currentState == BlacksmithState.Locked || currentState == BlacksmithState.ReadyForTalk) && beggarFinished)
            {
                questIndicator.SetVisible(true);
            }
            else if (currentState == BlacksmithState.CombatFinished)
            {
                questIndicator.SetVisible(true);
            }
            else
            {
                questIndicator.SetVisible(false);
            }
        }

        // 2. Bubble penunjuk jalan di pinggir layar
        if (arrowToBlacksmith != null)
        {
            if (isDialogActive || currentState == BlacksmithState.InCombat || currentState == BlacksmithState.QuestCompleted)
            {
                arrowToBlacksmith.SetActive(false);
            }
            else if ((currentState == BlacksmithState.Locked || currentState == BlacksmithState.ReadyForTalk) && beggarFinished)
            {
                arrowToBlacksmith.SetActive(true);
            }
            else if (currentState == BlacksmithState.CombatFinished)
            {
                arrowToBlacksmith.SetActive(true);
            }
            else
            {
                arrowToBlacksmith.SetActive(false);
            }
        }
    }

    private void FindPlayerTransform()
    {
        if (playerTransform != null) return;

        if (PlayerMovement.Instance != null)
            playerTransform = PlayerMovement.Instance.transform;
        else
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
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

            if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive && !DialogueManager.Instance.isEngaged)
            {
                DialogueManager.Instance.CancelDialogue();
                HideYakuzaAmbush();
            }
        }
    }

    public void TriggerBlacksmithInteraction()
    {
        if (dialogueManager == null || dialogueManager.isDialogueActive) return;

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
            FindPlayerTransform();

            dialogueManager.StartDialogue(dialoguePT1, 
                onComplete: () =>
                {
                    StartCombatWave();
                },
                onEngage: () =>
                {
                    SpawnYakuzaAmbush();
                }
            );
        }
        else if (currentState == BlacksmithState.CombatFinished)
        {
            dialogueManager.StartDialogue(dialoguePT2, () =>
            {
                currentState = BlacksmithState.QuestCompleted;

                StartCoroutine(MakeLeaderFlee());

                if (InventoryManager.Instance != null && katanaItem != null)
                {
                    InventoryManager.Instance.AddItem(katanaItem);
                }

                if (PlayerMovement.Instance != null)
                {
                    PlayerMovement.Instance.EquipSword(true);
                }

                if (PlayerCombat.Instance != null)
                {
                    PlayerCombat.Instance.EquipSword(true);
                }

                RefreshIndicator();
            });
        }
        else if (currentState == BlacksmithState.QuestCompleted)
        {
            dialogueManager.StartDialogue(dialogueCompleted);
        }
    }

    private void SpawnYakuzaAmbush()
    {
        if (suitYakuzaLeader != null) suitYakuzaLeader.SetActive(true);

        if (fighterYakuza != null)
        {
            foreach (var yakuza in fighterYakuza)
            {
                if (yakuza != null)
                {
                    yakuza.gameObject.SetActive(true);
                    yakuza.enabled = false;

                    Collider2D col = yakuza.GetComponent<Collider2D>();
                    if (col != null) col.enabled = false;
                }
            }
        }
    }

    private void HideYakuzaAmbush()
    {
        if (suitYakuzaLeader != null) suitYakuzaLeader.SetActive(false);

        if (fighterYakuza != null)
        {
            foreach (var yakuza in fighterYakuza)
            {
                if (yakuza != null)
                {
                    yakuza.gameObject.SetActive(false);
                    yakuza.enabled = false;
                    Collider2D col = yakuza.GetComponent<Collider2D>();
                    if (col != null) col.enabled = false;
                }
            }
        }
    }

    private void StartCombatWave()
    {
        currentState = BlacksmithState.InCombat;
        combatStarted = true;

        if (GameAudioManager.Instance != null)
        {
            GameAudioManager.Instance.PlayCombatBGM(0.4f);
        }

        FindPlayerTransform();

        if (fighterYakuza != null && playerTransform != null)
        {
            foreach (var yakuza in fighterYakuza)
            {
                if (yakuza != null)
                {
                    Collider2D col = yakuza.GetComponent<Collider2D>();
                    if (col != null) col.enabled = true;

                    yakuza.enabled = true;
                    yakuza.StartCombat(playerTransform);
                }
            }
        }
    }

    private void OnFighterKilled()
    {
        defeatedCount++;

        if (defeatedCount >= (fighterYakuza != null ? fighterYakuza.Length : 2) && combatStarted)
        {
            combatStarted = false;

            if (GameAudioManager.Instance != null)
            {
                GameAudioManager.Instance.PlayExplorationBGM(1.2f);
            }

            StartCoroutine(FinishCombatRoutine());
        }
    }

    private IEnumerator FinishCombatRoutine()
    {
        yield return new WaitForSeconds(0.6f);
        currentState = BlacksmithState.CombatFinished;
        TriggerBlacksmithInteraction();
    }

    private IEnumerator MakeLeaderFlee()
    {
        if (suitYakuzaLeader == null) yield break;

        SpriteRenderer sr = suitYakuzaLeader.GetComponent<SpriteRenderer>();
        if (sr != null) sr.flipX = true;

        float timer = 0f;
        while (timer < 1.8f)
        {
            suitYakuzaLeader.transform.Translate(Vector3.right * leaderFleeSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        suitYakuzaLeader.SetActive(false);
    }
}
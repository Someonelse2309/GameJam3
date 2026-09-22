using System.Collections;
using UnityEngine;

public class BlacksmithTrigger : MonoBehaviour
{
    [Header("Quest Dependencies")]
    public BeggarTrigger beggarTrigger;
    public ItemData katanaItem;
    public DialogueManager dialogueManager;

    [Header("Yakuza Enemies (Petarung)")]
    [Tooltip("Tarik 2 Yakuza Botak yang akan menyerang MC di sini")]
    public YakuzaEnemy[] fighterYakuza; 

    [Header("Yakuza Leader (Pengamat Jas Rapi)")]
    [Tooltip("Tarik NPC_SuitYakuza di sini")]
    public GameObject suitYakuzaLeader;
    public float leaderFleeSpeed = 6f;

    public enum BlacksmithState { Locked, ReadyForTalk, InCombat, CombatFinished, QuestCompleted }
    [Header("State")]
    public BlacksmithState currentState = BlacksmithState.Locked;

    private int defeatedCount = 0;
    private bool combatStarted = false;
    private bool isPlayerNearby = false;
    private Transform playerTransform;

    [Header("Dialogue PT1 (Datang ke Blacksmith - Pembuktian Dimulai)")]
    public DialogueSentence[] dialoguePT1 = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Ito Shun?" },
        new DialogueSentence { speakerName = "Ito Shun", sentence = "Michelle Sato... The Yakuza are scouring the streets for you." },
        new DialogueSentence { speakerName = "Ito Shun", sentence = "I have your katana, but I won't hand a deadly blade to someone who lost their fire." },
        new DialogueSentence { speakerName = "Suit Yakuza", sentence = "Michelle Sato! Cornered like a rat, and without a weapon!" },
        new DialogueSentence { speakerName = "Suit Yakuza", sentence = "Boys, break her limbs! The boss wants her breathing, but broken." },
        new DialogueSentence { speakerName = "Ito Shun", sentence = "Show me your resolve, Sato! Prove to me that the 'Onikoroshi' is still alive in you!" },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "I don't need steel to break your dogs." }
    };

    [Header("Dialogue PT2 (Setelah Kroco Kalah - Suit Yakuza Kabur & Dapat Katana)")]
    public DialogueSentence[] dialoguePT2 = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Suit Yakuza", sentence = "W-what the hell?! Barehanded...?! You're a monster!" },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Tell Aoyama I'm coming. Send him my regards." },
        new DialogueSentence { speakerName = "Suit Yakuza", sentence = "Aoyama-sama will tear you apart! This isn't over!" },
        new DialogueSentence { speakerName = "Ito Shun", sentence = "The legend was true... Even without steel, your edge never dulled." },
        new DialogueSentence { speakerName = "Ito Shun", sentence = "Here is your katana, Onikoroshi. Go reclaim your revenge." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Thank you, Ito Shun." },
        new DialogueSentence { speakerName = "Ito Shun", sentence = "Be careful. Aoyama won't fight with honor." }
    };

    private void Start()
    {
        if (dialogueManager == null)
            dialogueManager = DialogueManager.Instance != null ? DialogueManager.Instance : FindFirstObjectByType<DialogueManager>();

        if (beggarTrigger == null)
            beggarTrigger = FindFirstObjectByType<BeggarTrigger>();

        // Sembunyikan dan nonaktifkan Yakuza sebelum quest dipicu
        if (suitYakuzaLeader != null)
        {
            suitYakuzaLeader.SetActive(false);
            YakuzaEnemy leaderAi = suitYakuzaLeader.GetComponent<YakuzaEnemy>();
            if (leaderAi != null) leaderAi.enabled = false; // Jas rapi hanya menonton
        }

        if (fighterYakuza != null)
        {
            foreach (var yakuza in fighterYakuza)
            {
                if (yakuza != null)
                {
                    yakuza.enabled = false;
                    yakuza.gameObject.SetActive(false);
                    CharacterHealth hp = yakuza.GetComponent<CharacterHealth>();
                    if (hp != null) hp.OnDeath += OnFighterKilled;
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

        // Cek apakah quest Beggar sudah tuntas
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
            // Munculkan Suit Yakuza & 2 Yakuza Botak ke scene
            if (suitYakuzaLeader != null) suitYakuzaLeader.SetActive(true);

            if (fighterYakuza != null)
            {
                foreach (var yakuza in fighterYakuza)
                {
                    if (yakuza != null)
                    {
                        yakuza.gameObject.SetActive(true);
                        yakuza.enabled = false; // Tahan agar tidak menyerang saat dialog
                    }
                }
            }

            dialogueManager.StartDialogue(dialoguePT1, () =>
            {
                StartCombatWave();
            });
        }
        else if (currentState == BlacksmithState.CombatFinished)
        {
            dialogueManager.StartDialogue(dialoguePT2, () =>
            {
                currentState = BlacksmithState.QuestCompleted;

                // 1. Yakuza jas rapi lari terbirit-birit keluar scene
                StartCoroutine(MakeLeaderFlee());

                // 2. Beri Katana ke inventory
                if (InventoryManager.Instance != null && katanaItem != null)
                {
                    InventoryManager.Instance.AddItem(katanaItem);
                }

                // 3. Otomatis pasang katana ke MC (Damage & Range naik)
                if (PlayerMovement.Instance != null)
                {
                    PlayerMovement.Instance.EquipSword(true);
                }
            });
        }
    }

    private void StartCombatWave()
    {
        currentState = BlacksmithState.InCombat;
        combatStarted = true;

        // Hanya anak buah botak yang menyerang MC
        if (fighterYakuza != null)
        {
            foreach (var yakuza in fighterYakuza)
            {
                if (yakuza != null)
                {
                    yakuza.enabled = true;
                    yakuza.StartCombat(playerTransform);
                }
            }
        }
    }

    private void OnFighterKilled()
    {
        defeatedCount++;

        // Begitu 2 anak buah tumbang, masuki fase dialog selesai berantem
        if (defeatedCount >= (fighterYakuza != null ? fighterYakuza.Length : 2) && combatStarted)
        {
            combatStarted = false;
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
        if (sr != null) sr.flipX = true; // Menghadap ke arah kabur

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
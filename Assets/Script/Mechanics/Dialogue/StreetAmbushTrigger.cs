using System.Collections;
using UnityEngine;

public class StreetAmbushTrigger : MonoBehaviour
{
    [Header("Quest Prerequisite")]
    public BlacksmithTrigger blacksmithTrigger;
    public DialogueManager dialogueManager;

    [Header("Suit Yakuza Boss (Tarik NPC_WaveSuitYakuza2 ke sini)")]
    public YakuzaEnemy suitYakuzaBoss;
    public float leaderFleeSpeed = 7f;

    [Header("Enemies Per Wave (Hanya Kroco/Anak Buah)")]
    public YakuzaEnemy[] wave1Enemies;
    public YakuzaEnemy[] wave2Enemies;

    [Header("Post-Combat Navigation")]
    public GameObject arrowToBossGate;

    public enum AmbushState { Locked, Ready, IntroDialogue, Wave1, Wave2, DuelDialogue, BossDuel, Interrogation, Completed }
    [Header("State")]
    public AmbushState currentState = AmbushState.Locked;

    private int wave1Defeated = 0;
    private int wave2Defeated = 0;
    private Transform playerTransform;
    private bool hasTriggered = false;
    private CharacterHealth bossHealth;
    private Collider2D bossCollider;
    private bool bossDefeated = false;

    [Header("Dialogue 1: Monologue & Intro")]
    public DialogueSentence[] dialogueIntro = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Ten years without steel... yet the weight feels too familiar." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "The streets are dead quiet. They're already waiting for me." },
        new DialogueSentence { speakerName = "Suit Yakuza", sentence = "Did you really think you could walk out of here alive, Sato?!" },
        new DialogueSentence { speakerName = "Suit Yakuza", sentence = "You got lucky at the forge, but this is the syndicate's territory! Tear her apart, boys!" },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "You ran once. You should have kept running." }
    };

    [Header("Dialogue 2: Sebelum By One (Setelah Wave 2 Mati)")]
    public DialogueSentence[] dialogueBeforeDuel = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Suit Yakuza", sentence = "Useless fools! All of them... dead?!" },
        new DialogueSentence { speakerName = "Suit Yakuza", sentence = "Fine, Onikoroshi! I'll take your head to Aoyama-sama myself!" },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Step forward." }
    };

    [Header("Dialogue 3: Interogasi (Setelah Boss Tumbang)")]
    public DialogueSentence[] dialogueInterrogation = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Suit Yakuza", sentence = "N-no... stay back! All of them... in minutes... You really are the Onikoroshi..." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "I told you to send him my regards. Where is Aoyama?" },
        new DialogueSentence { speakerName = "Suit Yakuza", sentence = "The executive building! Top floor, past the courtyard gates!" },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Get out of my sight before I change my mind." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Ryu... Mother is coming." }
    };

    private void Start()
    {
        if (dialogueManager == null)
            dialogueManager = DialogueManager.Instance != null ? DialogueManager.Instance : FindFirstObjectByType<DialogueManager>();

        if (blacksmithTrigger == null)
            blacksmithTrigger = FindFirstObjectByType<BlacksmithTrigger>();

        FindPlayer();

        // Setup Boss
        if (suitYakuzaBoss != null)
        {
            bossHealth = suitYakuzaBoss.GetComponent<CharacterHealth>();
            bossCollider = suitYakuzaBoss.GetComponent<Collider2D>();
        }

        DeactivateAllEnemies();

        if (arrowToBossGate != null)
            arrowToBossGate.SetActive(false);
    }

    private void Update()
    {
        if (!hasTriggered && blacksmithTrigger != null && blacksmithTrigger.currentState == BlacksmithTrigger.BlacksmithState.QuestCompleted)
        {
            currentState = AmbushState.Ready;
        }

        // Pantau HP boss saat sesi By One agar tidak hancur/hilang sebelum interogasi
        if (currentState == AmbushState.BossDuel && bossHealth != null && !bossDefeated)
        {
            if (bossHealth.currentHealth <= 15)
            {
                bossDefeated = true;
                bossHealth.currentHealth = 15; // Kunci HP agar tidak ter-destroy oleh skrip CharacterHealth bawaan
                OnBossDown();
            }
        }
    }

    private void FindPlayer()
    {
        if (playerTransform != null) return;
        if (PlayerMovement.Instance != null) playerTransform = PlayerMovement.Instance.transform;
        else
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }
    }

    private void DeactivateAllEnemies()
    {
        // Suit Yakuza disembunyikan sampai trigger tersentuh
        if (suitYakuzaBoss != null)
        {
            suitYakuzaBoss.gameObject.SetActive(false);
            suitYakuzaBoss.enabled = false;
            if (bossCollider != null) bossCollider.enabled = false;
        }

        SetWaveActive(wave1Enemies, false);
        SetWaveActive(wave2Enemies, false);
    }

    private void SetWaveActive(YakuzaEnemy[] wave, bool active)
    {
        if (wave == null) return;
        foreach (var enemy in wave)
        {
            if (enemy != null)
            {
                enemy.gameObject.SetActive(active);
                enemy.enabled = active;
                Collider2D col = enemy.GetComponent<Collider2D>();
                if (col != null) col.enabled = active;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && currentState == AmbushState.Ready && !hasTriggered)
        {
            hasTriggered = true;
            playerTransform = collision.transform;
            StartIntroEncounter();
        }
    }

    private void StartIntroEncounter()
    {
        currentState = AmbushState.IntroDialogue;

        // Tampilkan Suit Yakuza berdiri menonton di belakang
        if (suitYakuzaBoss != null)
        {
            suitYakuzaBoss.gameObject.SetActive(true);
            suitYakuzaBoss.enabled = false; // Belum ikut serang
            if (bossCollider != null) bossCollider.enabled = false; // Kebal dari tebasan liar
        }

        dialogueManager.StartDialogue(dialogueIntro, onComplete: () =>
        {
            StartWave1();
        });
    }

    private void StartWave1()
    {
        currentState = AmbushState.Wave1;

        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayCombatBGM(0.4f);

        SetWaveActive(wave1Enemies, true);

        foreach (var enemy in wave1Enemies)
        {
            if (enemy != null)
            {
                CharacterHealth hp = enemy.GetComponent<CharacterHealth>();
                if (hp != null) hp.OnDeath += OnWave1EnemyKilled;
                enemy.StartCombat(playerTransform);
            }
        }
    }

    private void OnWave1EnemyKilled()
    {
        wave1Defeated++;
        if (wave1Defeated >= wave1Enemies.Length)
        {
            StartCoroutine(SpawnWave2Routine());
        }
    }

    private IEnumerator SpawnWave2Routine()
    {
        yield return new WaitForSeconds(0.6f);
        currentState = AmbushState.Wave2;

        SetWaveActive(wave2Enemies, true);

        foreach (var enemy in wave2Enemies)
        {
            if (enemy != null)
            {
                CharacterHealth hp = enemy.GetComponent<CharacterHealth>();
                if (hp != null) hp.OnDeath += OnWave2EnemyKilled;
                enemy.StartCombat(playerTransform);
            }
        }
    }

    private void OnWave2EnemyKilled()
    {
        wave2Defeated++;
        if (wave2Defeated >= wave2Enemies.Length)
        {
            StartCoroutine(TriggerBeforeDuelRoutine());
        }
    }

    // Dialog transisi sebelum By One
    private IEnumerator TriggerBeforeDuelRoutine()
    {
        yield return new WaitForSeconds(0.6f);
        currentState = AmbushState.DuelDialogue;

        dialogueManager.StartDialogue(dialogueBeforeDuel, onComplete: () =>
        {
            StartBossDuel();
        });
    }

    // Memulai sesi By One dengan Suit Yakuza
    private void StartBossDuel()
    {
        currentState = AmbushState.BossDuel;

        if (suitYakuzaBoss != null && playerTransform != null)
        {
            if (bossCollider != null) bossCollider.enabled = true;
            suitYakuzaBoss.enabled = true;
            suitYakuzaBoss.StartCombat(playerTransform);
        }
    }

    // Dipanggil saat Suit Yakuza kalah dalam duel
    private void OnBossDown()
    {
        if (suitYakuzaBoss != null)
        {
            suitYakuzaBoss.enabled = false; // Matikan AI menyerang
            if (bossCollider != null) bossCollider.enabled = false;

            Rigidbody2D rb = suitYakuzaBoss.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        StartCoroutine(StartInterrogationRoutine());
    }

    private IEnumerator StartInterrogationRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayExplorationBGM(1.2f);

        currentState = AmbushState.Interrogation;

        // Hadapkan Suit Yakuza ke arah pemain saat bicara
        if (suitYakuzaBoss != null && playerTransform != null)
        {
            SpriteRenderer sr = suitYakuzaBoss.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.flipX = (suitYakuzaBoss.transform.position.x > playerTransform.position.x);
            }
        }

        dialogueManager.StartDialogue(dialogueInterrogation, onComplete: () =>
        {
            currentState = AmbushState.Completed;
            StartCoroutine(MakeSuitYakuzaFlee());

            if (arrowToBossGate != null)
                arrowToBossGate.SetActive(true);
        });
    }

    private IEnumerator MakeSuitYakuzaFlee()
    {
        if (suitYakuzaBoss == null) yield break;

        SpriteRenderer sr = suitYakuzaBoss.GetComponent<SpriteRenderer>();
        if (sr != null) sr.flipX = true; // Hadap kanan untuk kabur

        float timer = 0f;
        while (timer < 2f)
        {
            suitYakuzaBoss.transform.Translate(Vector3.right * leaderFleeSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        suitYakuzaBoss.gameObject.SetActive(false);
    }
}
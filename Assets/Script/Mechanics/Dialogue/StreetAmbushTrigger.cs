using System.Collections;
using UnityEngine;

public class StreetAmbushTrigger : MonoBehaviour
{
    [Header("Quest Prerequisite")]
    public BlacksmithTrigger blacksmithTrigger;
    public DialogueManager dialogueManager;

    [Header("Suit Yakuza Leader")]
    public GameObject suitYakuzaLeader;
    public float leaderFleeSpeed = 7f;

    [Header("Enemies Per Wave")]
    public YakuzaEnemy[] wave1Enemies;
    public YakuzaEnemy[] wave2Enemies;

    [Header("Post-Combat Navigation")]
    public GameObject arrowToBossGate;

    public enum AmbushState { Locked, Ready, InDialogue, Wave1, Wave2, Interrogation, Completed }
    [Header("State")]
    public AmbushState currentState = AmbushState.Locked;

    private int wave1Defeated = 0;
    private int wave2Defeated = 0;
    private Transform playerTransform;
    private bool hasTriggered = false;

    [Header("Dialogue: Monologue & Encounter")]
    public DialogueSentence[] dialogueIntro = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Ten years without steel... yet the weight feels too familiar." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "The streets are dead quiet. They're already waiting for me." },
        new DialogueSentence { speakerName = "Suit Yakuza", sentence = "Did you really think you could walk out of here alive, Sato?!" },
        new DialogueSentence { speakerName = "Suit Yakuza", sentence = "You got lucky at the forge, but this is the full might of the syndicate! Kill her!" },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "You ran once. You should have kept running." }
    };

    [Header("Dialogue: Interogasi Suit Yakuza")]
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
        if (suitYakuzaLeader != null) suitYakuzaLeader.SetActive(false);

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
        currentState = AmbushState.InDialogue;

        // Tampilkan Suit Yakuza di posisi mencegat
        if (suitYakuzaLeader != null)
            suitYakuzaLeader.SetActive(true);

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
        yield return new WaitForSeconds(0.8f);
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
            StartCoroutine(StartInterrogationRoutine());
        }
    }

    private IEnumerator StartInterrogationRoutine()
    {
        yield return new WaitForSeconds(0.6f);

        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayExplorationBGM(1.2f);

        currentState = AmbushState.Interrogation;

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
        if (suitYakuzaLeader == null) yield break;

        SpriteRenderer sr = suitYakuzaLeader.GetComponent<SpriteRenderer>();
        if (sr != null) sr.flipX = true;

        float timer = 0f;
        while (timer < 2f)
        {
            suitYakuzaLeader.transform.Translate(Vector3.right * leaderFleeSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        suitYakuzaLeader.SetActive(false);
    }
}
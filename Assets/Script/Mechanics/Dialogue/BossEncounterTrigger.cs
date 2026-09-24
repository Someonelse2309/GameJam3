using System.Collections;
using UnityEngine;

public class BossEncounterTrigger : MonoBehaviour
{
    [Header("UI & Dialogue")]
    public DialogueManager dialogueManager;
    public GameObject arrowToBoss;

    [Header("Boss Setup")]
    public YakuzaEnemy bossEnemy;
    public CharacterHealth bossHealth;
    [Tooltip("Opsional: Jika ada pengawal elit pendamping boss")]
    public YakuzaEnemy[] bossMinions;

    public enum BossState { WaitingForPlayer, IntroDialogue, InCombat, OutroDialogue, Completed }
    [Header("State")]
    public BossState currentState = BossState.WaitingForPlayer;

    private bool hasTriggered = false;
    private Transform playerTransform;

    [Header("Dialogue: Konfrontasi Sebelum Duel")]
    public DialogueSentence[] dialogueIntro = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Aoyama! Hand over Ryu, now!" },
        new DialogueSentence { speakerName = "Aoyama", sentence = "Michelle... The Legendary Onikoroshi. You crawled through an army of my men just for a brat?" },
        new DialogueSentence { speakerName = "Aoyama", sentence = "Your legend ends on this rooftop. Ten years of peace made you soft, Sato!" },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "You threatened my family, Aoyama. Soft or not... today you die." }
    };

    [Header("Dialogue: Ending / Reuni dengan Ryu")]
    public DialogueSentence[] dialogueOutro = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Aoyama", sentence = "I-impossible... That blade... You truly haven't lost your edge..." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "This was for everyone you took from me. It's over, Aoyama." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Ryu... Mother is here. You're safe now." }
    };

    private void Start()
    {
        if (dialogueManager == null)
            dialogueManager = DialogueManager.Instance != null ? DialogueManager.Instance : FindFirstObjectByType<DialogueManager>();

        FindPlayer();

        // Pastikan Boss diam dan kebal sebelum dialog intro selesai
        if (bossEnemy != null)
        {
            bossEnemy.enabled = false;
            Collider2D bossCol = bossEnemy.GetComponent<Collider2D>();
            if (bossCol != null) bossCol.enabled = false;
        }

        if (bossHealth == null && bossEnemy != null)
            bossHealth = bossEnemy.GetComponent<CharacterHealth>();

        if (bossHealth != null)
            bossHealth.OnDeath += OnBossDefeated;

        SetMinionsActive(false);
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

    private void SetMinionsActive(bool active)
    {
        if (bossMinions == null) return;
        foreach (var minion in bossMinions)
        {
            if (minion != null)
            {
                minion.gameObject.SetActive(active);
                minion.enabled = active;
                Collider2D col = minion.GetComponent<Collider2D>();
                if (col != null) col.enabled = active;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasTriggered && currentState == BossState.WaitingForPlayer)
        {
            hasTriggered = true;
            playerTransform = collision.transform;
            StartBossIntro();
        }
    }

    private void StartBossIntro()
    {
        currentState = BossState.IntroDialogue;

        // Matikan panah penunjuk karena pemain sudah sampai di arena
        if (arrowToBoss != null)
            arrowToBoss.SetActive(false);

        // Bekukan gerakan pemain saat berhadapan
        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.SetFreeze(true);

        dialogueManager.StartDialogue(dialogueIntro, onComplete: () =>
        {
            StartBossFight();
        });
    }

    private void StartBossFight()
    {
        currentState = BossState.InCombat;

        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.SetFreeze(false);

        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayCombatBGM(0.4f);

        // Aktifkan AI Boss
        if (bossEnemy != null)
        {
            Collider2D bossCol = bossEnemy.GetComponent<Collider2D>();
            if (bossCol != null) bossCol.enabled = true;

            bossEnemy.enabled = true;
            bossEnemy.StartCombat(playerTransform);
        }

        // Aktifkan pengawal (jika ada)
        SetMinionsActive(true);
        if (bossMinions != null && playerTransform != null)
        {
            foreach (var minion in bossMinions)
            {
                if (minion != null) minion.StartCombat(playerTransform);
            }
        }
    }

    private void OnBossDefeated()
    {
        StartCoroutine(BossVictoryRoutine());
    }

    private IEnumerator BossVictoryRoutine()
    {
        yield return new WaitForSeconds(0.6f);

        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayExplorationBGM(1.2f);

        currentState = BossState.OutroDialogue;

        // Dialog kemenangan & penyelamatan anak
        dialogueManager.StartDialogue(dialogueOutro, onComplete: () =>
        {
            currentState = BossState.Completed;
            // Akhir babak / misi selesai
        });
    }
}
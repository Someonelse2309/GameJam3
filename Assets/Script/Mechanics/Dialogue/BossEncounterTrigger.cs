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
    public YakuzaEnemy[] bossMinions;

    public enum BossState { WaitingForPlayer, IntroDialogue, InCombat, Defeated }
    [Header("State")]
    public BossState currentState = BossState.WaitingForPlayer;

    private bool hasTriggered = false;
    private Transform playerTransform;

    [Header("Dialogue: Jika Belum Kumpul 4 CD")]
    public DialogueSentence[] dialogueNeedDisksFirst = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "The rooftop gate is heavily barricaded..." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "I can't face Aoyama yet. I need to recover all 4 of Ryu's memory disks scattered in this sector first!" }
    };

    [Header("Dialogue: Konfrontasi Sebelum Duel")]
    public DialogueSentence[] dialogueIntro = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Aoyama! Hand over Ryu, now!" },
        new DialogueSentence { speakerName = "Aoyama", sentence = "Michelle... The Legendary Onikoroshi. You crawled through an army of my men just for a brat?" },
        new DialogueSentence { speakerName = "Aoyama", sentence = "Your legend ends on this rooftop. Ten years of peace made you soft, Sato!" },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "You threatened my family, Aoyama. Soft or not... today you die." }
    };

    private void Start()
    {
        if (dialogueManager == null)
            dialogueManager = DialogueManager.Instance != null ? DialogueManager.Instance : FindFirstObjectByType<DialogueManager>();

        FindPlayer();

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
        if (collision.CompareTag("Player") && currentState == BossState.WaitingForPlayer)
        {
            if (MemoryDiskManager.Instance != null && !MemoryDiskManager.Instance.HasCollectedAllExplorationDisks())
            {
                dialogueManager.StartDialogue(dialogueNeedDisksFirst);
                return;
            }

            if (!hasTriggered)
            {
                hasTriggered = true;
                playerTransform = collision.transform;
                StartBossIntro();
            }
        }
    }

    private void StartBossIntro()
    {
        currentState = BossState.IntroDialogue;

        if (arrowToBoss != null)
            arrowToBoss.SetActive(false);

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

        if (bossEnemy != null)
        {
            Collider2D bossCol = bossEnemy.GetComponent<Collider2D>();
            if (bossCol != null) bossCol.enabled = true;

            bossEnemy.enabled = true;
            bossEnemy.StartCombat(playerTransform);
        }

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
        currentState = BossState.Defeated;
        SetMinionsActive(false);

        // Panggil Outro Cinematic Manager yang independen
        if (OutroCinematicManager.Instance != null)
        {
            OutroCinematicManager.Instance.PlayOutroCinematic();
        }
    }
}
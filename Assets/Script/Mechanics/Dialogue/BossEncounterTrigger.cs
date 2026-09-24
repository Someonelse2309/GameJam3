using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class BossEncounterTrigger : MonoBehaviour
{
    [Header("UI & Dialogue")]
    public DialogueManager dialogueManager;
    public GameObject arrowToBoss;

    [Header("Cinematic UI References")]
    [Tooltip("Tarik objek CinematicBars (panel penampung)")]
    public GameObject cinematicBars;
    public RectTransform topBar;
    public RectTransform bottomBar;
    public float barHeight = 120f;
    public float transitionSpeed = 3f;

    [Tooltip("Daftar UI gameplay yang harus disembunyikan saat ending (Joystick, Attack Button, Bag, HP Bar, Disk Counter, Quest Button)")]
    public GameObject[] gameplayUIElements;

    [Tooltip("Panel hitam satu layar untuk fade to black")]
    public Image fadeOverlay;

    [Tooltip("Image untuk efek flash glitch CD terakhir")]
    public Image glitchOverlay;
    public Sprite[] glitchSprites;

    [Tooltip("Teks epilog akhir cerita")]
    public TMP_Text epilogueText;

    [Tooltip("Tombol untuk kembali ke Main Menu")]
    public Button returnToMenuButton;

    [Header("Cinematic Audio & Camera")]
    public AudioClip emotionalBGM;
    public AudioClip diskRestoredSFX;
    public Camera targetCamera;

    [Header("Boss Setup")]
    public YakuzaEnemy bossEnemy;
    public CharacterHealth bossHealth;
    public YakuzaEnemy[] bossMinions;

    public enum BossState { WaitingForPlayer, IntroDialogue, InCombat, OutroDialogue, CinematicEnding, Completed }
    [Header("State")]
    public BossState currentState = BossState.WaitingForPlayer;

    private bool hasTriggered = false;
    private Transform playerTransform;
    private AudioSource endingAudioSource;

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

    [Header("Dialogue: Ending / Reuni dengan Ryu")]
    public DialogueSentence[] dialogueOutro = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Aoyama", sentence = "I-impossible... That blade... You truly haven't lost your edge..." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "This was for everyone you took from me. It's over, Aoyama." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Ryu... Mother is here. The final memory disk is recovered. You're safe now." }
    };

    private void Start()
    {
        if (dialogueManager == null)
            dialogueManager = DialogueManager.Instance != null ? DialogueManager.Instance : FindFirstObjectByType<DialogueManager>();

        if (targetCamera == null)
            targetCamera = Camera.main;

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

        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            Color c = fadeOverlay.color;
            c.a = 0f;
            fadeOverlay.color = c;
            fadeOverlay.raycastTarget = false;
        }

        if (glitchOverlay != null) glitchOverlay.gameObject.SetActive(false);
        if (epilogueText != null) epilogueText.gameObject.SetActive(false);
        if (returnToMenuButton != null)
        {
            returnToMenuButton.gameObject.SetActive(false);
            returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
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
        StartCoroutine(CinematicEndingRoutine());
    }

    private void StopExistingBGM()
    {
        if (GameAudioManager.Instance != null)
        {
            GameAudioManager.Instance.StopAllCoroutines();
            AudioSource[] allAudioSources = GameAudioManager.Instance.GetComponentsInChildren<AudioSource>();
            foreach (AudioSource src in allAudioSources)
            {
                src.Stop();
            }
        }
    }

    private IEnumerator AnimateBars(bool opening)
    {
        if (topBar == null || bottomBar == null) yield break;

        float targetHeight = opening ? barHeight : 0f;
        float currentHeight = opening ? 0f : barHeight;

        topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, currentHeight);
        bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, currentHeight);

        while (Mathf.Abs(currentHeight - targetHeight) > 1f)
        {
            currentHeight = Mathf.MoveTowards(currentHeight, targetHeight, Time.unscaledDeltaTime * 350f * transitionSpeed);
            topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, currentHeight);
            bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, currentHeight);
            yield return null;
        }

        topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, targetHeight);
        bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, targetHeight);
    }

    private IEnumerator CinematicEndingRoutine()
    {
        currentState = BossState.CinematicEnding;

        // ==========================================
        // TAHAP 1: SLOW-MOTION IMPACT & CUT COMBAT BGM
        // ==========================================
        StopExistingBGM();

        Time.timeScale = 0.2f;
        yield return new WaitForSecondsRealtime(1.2f);
        Time.timeScale = 1.0f;

        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.SetFreeze(true);

        SetMinionsActive(false);

        // ==========================================
        // TAHAP 2: CLEAN UI & ANIMATE LETTERBOX
        // ==========================================
        if (cinematicBars != null)
        {
            cinematicBars.SetActive(true);
            StartCoroutine(AnimateBars(true)); // Bar hitam turun & naik mulus
        }

        if (gameplayUIElements != null)
        {
            foreach (var ui in gameplayUIElements)
            {
                if (ui != null) ui.SetActive(false);
            }
        }

        // ==========================================
        // TAHAP 3: AUDIO EMOSIONAL & KAMERA ZOOM
        // ==========================================
        StopExistingBGM();

        if (emotionalBGM != null)
        {
            if (endingAudioSource == null) endingAudioSource = GetComponent<AudioSource>();
            if (endingAudioSource == null) endingAudioSource = gameObject.AddComponent<AudioSource>();

            endingAudioSource.clip = emotionalBGM;
            endingAudioSource.loop = true;
            endingAudioSource.volume = 0.5f;
            endingAudioSource.Play();
        }

        if (targetCamera != null)
        {
            float elapsedZoom = 0f;
            float startSize = targetCamera.orthographicSize;
            float targetSize = Mathf.Max(2.4f, startSize * 0.7f);

            while (elapsedZoom < 1.5f)
            {
                targetCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, elapsedZoom / 1.5f);
                elapsedZoom += Time.deltaTime;
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.4f);

        // ==========================================
        // TAHAP 4: DIALOG OUTRO & MEMORY GLITCH DISK KE-5
        // ==========================================
        currentState = BossState.OutroDialogue;

        bool dialogueDone = false;
        dialogueManager.StartDialogue(dialogueOutro, onComplete: () =>
        {
            dialogueDone = true;
        });

        yield return new WaitUntil(() => dialogueDone);

        if (MemoryDiskManager.Instance != null)
            MemoryDiskManager.Instance.CollectBossDisk();

        if (glitchOverlay != null && glitchSprites != null && glitchSprites.Length > 0)
        {
            glitchOverlay.gameObject.SetActive(true);
            if (diskRestoredSFX != null && GameAudioManager.Instance != null)
                GameAudioManager.Instance.PlaySFX(diskRestoredSFX);

            for (int i = 0; i < glitchSprites.Length; i++)
            {
                glitchOverlay.sprite = glitchSprites[i];
                yield return new WaitForSeconds(0.08f);
            }
            glitchOverlay.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(1.0f);

        // ==========================================
        // TAHAP 5: FADE TO BLACK & TEKS EPILOG
        // ==========================================
        if (fadeOverlay != null)
        {
            fadeOverlay.raycastTarget = true;
            float elapsedFade = 0f;
            while (elapsedFade < 2.5f)
            {
                Color c = fadeOverlay.color;
                c.a = Mathf.Lerp(0f, 1f, elapsedFade / 2.5f);
                fadeOverlay.color = c;
                elapsedFade += Time.deltaTime;
                yield return null;
            }
            Color finalC = fadeOverlay.color;
            finalC.a = 1f;
            fadeOverlay.color = finalC;
        }

        yield return new WaitForSeconds(1.0f);

        if (epilogueText != null)
        {
            epilogueText.gameObject.SetActive(true);
            epilogueText.text = "The steel has tasted vengeance.\n\nThe fragments of the past are whole once more.\n\nRest easy, Ryu... Mother is home.";
        }

        yield return new WaitForSeconds(3.0f);

        if (returnToMenuButton != null)
        {
            returnToMenuButton.gameObject.SetActive(true);
        }

        currentState = BossState.Completed;
    }

    private void OnReturnToMenuClicked()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("Main Menu");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ForceTestEnding();
        }
    }

    [ContextMenu("Force Test Ending Cinematic")]
    public void ForceTestEnding()
    {
        StopAllCoroutines();
        if (endingAudioSource != null) endingAudioSource.Stop();
        StartCoroutine(CinematicEndingRoutine());
    }
}
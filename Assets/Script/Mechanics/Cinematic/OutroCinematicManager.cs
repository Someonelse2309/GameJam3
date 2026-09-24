using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class OutroCinematicManager : MonoBehaviour
{
    public static OutroCinematicManager Instance;

    [Header("Final Slash & Slow-Motion Settings")]
    [Range(0.01f, 1f)] public float slowmoTimeScale = 0.1f;
    public float slowmoDurationRealtime = 2f;
    public AudioClip finalSlashSFX;
    public Image whiteFlashOverlay;
    public float flashFadeSpeed = 4f;

    [Header("Cinematic Letterbox (Bars)")]
    public GameObject cinematicBarsPanel;
    public RectTransform topBar;
    public RectTransform bottomBar;
    public float barHeight = 120f;
    public float barTransitionSpeed = 3f;

    [Header("Gameplay HUD to Hide")]
    public GameObject[] gameplayUIElements;

    [Header("Audio")]
    public AudioClip emotionalBGM;
    public AudioClip diskRestoredSFX;
    public AudioClip typewriterSFX;
    private AudioSource endingAudioSource;

    [Header("Camera & Glitch Shake")]
    public Camera targetCamera;
    public float cameraZoomDuration = 8f;
    public float cameraTargetSize = 1.6f;
    public float glitchShakeIntensity = 0.45f;

    [Header("Memory Glitch Overlay")]
    public Image glitchOverlay;
    public Sprite[] glitchSprites;

    [Header("Autoplay Settings")]
    [Tooltip("Kecepatan pengetikan dialog outro (detik per huruf).")]
    public float dialogueTypingSpeed = 0.04f;
    [Tooltip("Jeda baca setelah satu kalimat selesai sebelum otomatis pindah ke kalimat berikutnya.")]
    public float dialogueSentencePause = 2.0f;

    [Header("Outro Dialogue (Deep & Emotional)")]
    public DialogueManager dialogueManager;
    public DialogueSentence[] dialogueOutro = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Aoyama", sentence = "I-impossible... That blade... You truly haven't lost your edge..." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "This was for every tear you forced my boy to shed. It ends here, Aoyama." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Ryu... My sweet, precious boy. Can you hear me?" },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Mother fought her way through the dark just to reclaim your memory disks." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Every fragment of your smile, every sound of your voice... I have them all back now." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "You will never be forgotten. Your story lives forever in my heart. Rest easy, my love." }
    };

    [Header("Fade to Black & Epilogue")]
    public Image fadeOverlay;
    public TMP_Text epilogueText;
    [TextArea(3, 5)]
    public string epilogueStory = "THE STEEL HAS TASTED VENGEANCE.\n\nTHE FRAGMENTS OF THE PAST ARE WHOLE ONCE MORE.\n\nREST EASY, RYU...\nMOTHER IS HOME.";
    public float epilogueTypingSpeed = 0.05f;
    public Button returnToMenuButton;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (dialogueManager == null)
            dialogueManager = DialogueManager.Instance != null ? DialogueManager.Instance : FindFirstObjectByType<DialogueManager>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (whiteFlashOverlay != null)
        {
            Color c = whiteFlashOverlay.color;
            c.a = 0f;
            whiteFlashOverlay.color = c;
            whiteFlashOverlay.gameObject.SetActive(true);
        }

        if (fadeOverlay != null)
        {
            Color c = fadeOverlay.color;
            c.a = 0f;
            fadeOverlay.color = c;
            fadeOverlay.raycastTarget = false;
            fadeOverlay.gameObject.SetActive(true);
        }

        if (glitchOverlay != null) glitchOverlay.gameObject.SetActive(false);
        if (epilogueText != null) 
        {
            epilogueText.text = "";
            epilogueText.gameObject.SetActive(false);
        }

        if (returnToMenuButton != null)
        {
            returnToMenuButton.gameObject.SetActive(false);
            returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
        }
    }

    public void PlayOutroCinematic()
    {
        StopAllCoroutines();
        StartCoroutine(CinematicOutroRoutine());
    }

    private IEnumerator CinematicOutroRoutine()
    {
        // 1. Tebasan Terakhir & Slow-mo
        StopAllGameplayAudio();

        if (finalSlashSFX != null && GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlaySFX(finalSlashSFX);

        if (whiteFlashOverlay != null)
            StartCoroutine(TriggerWhiteFlash());

        Time.timeScale = slowmoTimeScale;
        yield return new WaitForSecondsRealtime(slowmoDurationRealtime);
        Time.timeScale = 1.0f;

        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.SetFreeze(true);

        // 2. Sembunyikan HUD Gameplay & Tampilkan Bar Sinematik
        SetGameplayHUDVisible(false);

        if (cinematicBarsPanel != null)
        {
            cinematicBarsPanel.SetActive(true);
            StartCoroutine(AnimateBars(true));
        }

        // 3. Audio Emosional & Zoom Kamera Halus
        PlayEmotionalBGM();

        if (targetCamera != null)
            StartCoroutine(ZoomCamera(targetCamera.orthographicSize, cameraTargetSize, cameraZoomDuration));

        yield return new WaitForSeconds(0.6f);

        // 4. Outro Dialog Sinematik (Autoplay tanpa skip)
        bool dialogueDone = false;
        dialogueManager.StartCinematicDialogue(dialogueOutro, dialogueTypingSpeed, dialogueSentencePause, onComplete: () =>
        {
            dialogueDone = true;
        });

        yield return new WaitUntil(() => dialogueDone);

        // 5. Pemulihan Disk Terakhir & Efek Glitch Bergetar
        if (MemoryDiskManager.Instance != null)
            MemoryDiskManager.Instance.CollectBossDisk();

        if (glitchOverlay != null && glitchSprites != null && glitchSprites.Length > 0)
        {
            glitchOverlay.gameObject.SetActive(true);
            if (diskRestoredSFX != null && GameAudioManager.Instance != null)
                GameAudioManager.Instance.PlaySFX(diskRestoredSFX);

            StartCoroutine(CameraShake(glitchSprites.Length * 0.08f, glitchShakeIntensity));

            for (int i = 0; i < glitchSprites.Length; i++)
            {
                glitchOverlay.sprite = glitchSprites[i];
                yield return new WaitForSeconds(0.08f);
            }
            glitchOverlay.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(1.0f);

        // 6. Fade to Black
        if (fadeOverlay != null)
        {
            fadeOverlay.raycastTarget = true;
            float elapsed = 0f;
            while (elapsed < 2.5f)
            {
                Color c = fadeOverlay.color;
                c.a = Mathf.Lerp(0f, 1f, elapsed / 2.5f);
                fadeOverlay.color = c;
                elapsed += Time.deltaTime;
                yield return null;
            }
            Color finalC = fadeOverlay.color;
            finalC.a = 1f;
            fadeOverlay.color = finalC;
        }

        yield return new WaitForSeconds(1.0f);

        // 7. Pengetikan Teks Epilog Akhir
        if (epilogueText != null)
        {
            epilogueText.gameObject.SetActive(true);
            yield return StartCoroutine(TypewriterTextRoutine(epilogueStory));
        }

        yield return new WaitForSeconds(2.0f);

        if (returnToMenuButton != null)
            returnToMenuButton.gameObject.SetActive(true);
    }

    private IEnumerator TypewriterTextRoutine(string fullText)
    {
        epilogueText.text = "";

        foreach (char letter in fullText.ToCharArray())
        {
            epilogueText.text += letter;

            if (letter != ' ' && letter != '\n' && typewriterSFX != null && GameAudioManager.Instance != null)
            {
                GameAudioManager.Instance.PlaySFX(typewriterSFX);
            }

            if (letter == '.')
                yield return new WaitForSeconds(0.35f);
            else if (letter == '\n')
                yield return new WaitForSeconds(0.2f);
            else
                yield return new WaitForSeconds(epilogueTypingSpeed);
        }
    }

    private IEnumerator TriggerWhiteFlash()
    {
        Color c = whiteFlashOverlay.color;
        c.a = 1f;
        whiteFlashOverlay.color = c;

        while (whiteFlashOverlay.color.a > 0.01f)
        {
            c.a = Mathf.MoveTowards(c.a, 0f, Time.unscaledDeltaTime * flashFadeSpeed);
            whiteFlashOverlay.color = c;
            yield return null;
        }
        c.a = 0f;
        whiteFlashOverlay.color = c;
    }

    private IEnumerator CameraShake(float duration, float magnitude)
    {
        if (targetCamera == null) yield break;

        Vector3 originalPos = targetCamera.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;

            targetCamera.transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        targetCamera.transform.localPosition = originalPos;
    }

    private IEnumerator ZoomCamera(float startSize, float targetSize, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            targetCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        targetCamera.orthographicSize = targetSize;
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
            currentHeight = Mathf.MoveTowards(currentHeight, targetHeight, Time.unscaledDeltaTime * 350f * barTransitionSpeed);
            topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, currentHeight);
            bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, currentHeight);
            yield return null;
        }

        topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, targetHeight);
        bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, targetHeight);
    }

    private void SetGameplayHUDVisible(bool visible)
    {
        if (gameplayUIElements == null) return;
        foreach (var ui in gameplayUIElements)
        {
            if (ui != null) ui.SetActive(visible);
        }
    }

    private void StopAllGameplayAudio()
    {
        if (GameAudioManager.Instance != null)
        {
            GameAudioManager.Instance.StopAllCoroutines();
            AudioSource[] sources = GameAudioManager.Instance.GetComponentsInChildren<AudioSource>();
            foreach (AudioSource s in sources) s.Stop();
        }
    }

    private void PlayEmotionalBGM()
    {
        if (emotionalBGM == null) return;
        if (endingAudioSource == null) endingAudioSource = GetComponent<AudioSource>();
        if (endingAudioSource == null) endingAudioSource = gameObject.AddComponent<AudioSource>();

        endingAudioSource.clip = emotionalBGM;
        endingAudioSource.loop = true;
        endingAudioSource.volume = 0.5f;
        endingAudioSource.Play();
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
            PlayOutroCinematic();
        }
    }

    [ContextMenu("Force Test Outro Cinematic")]
    public void ForceTestFromInspector()
    {
        PlayOutroCinematic();
    }
}
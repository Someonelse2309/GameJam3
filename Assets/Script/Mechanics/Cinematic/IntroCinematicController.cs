using System.Collections;
using UnityEngine;

public class IntroCinematicController : MonoBehaviour
{
    [Header("Cinematic UI")]
    [Tooltip("Panel atau GameObject penampung bar hitam atas & bawah")]
    public GameObject cinematicBarsPanel;
    public RectTransform topBar;
    public RectTransform bottomBar;
    public float barHeight = 120f;
    public float transitionSpeed = 3f;

    [Header("Gameplay HUD to Hide")]
    [Tooltip("Tarik elemen UI gameplay seperti Joystick, Tombol Serang, Healthbar, Bag, dll")]
    public GameObject[] gameplayUIElements;

    [Header("References")]
    public DialogueManager dialogueManager;

    [Header("Opening Monologue Dialogue")]
    public DialogueSentence[] openingMonologue = new DialogueSentence[]
    {
        new DialogueSentence 
        { 
            speakerName = "Michelle Sato", 
            sentence = "Ten years... Ten years I swore never to touch a blade again." 
        },
        new DialogueSentence 
        { 
            speakerName = "Michelle Sato", 
            sentence = "I buried the bloody ghost of 'Onikoroshi' to give my son a peaceful life." 
        },
        new DialogueSentence 
        { 
            speakerName = "Michelle Sato", 
            sentence = "Yet Aoyama's syndicate dragged my sins back into the light. They took Ryu." 
        },
        new DialogueSentence 
        { 
            speakerName = "Michelle Sato", 
            sentence = "They think ten years of quiet motherhood made me soft. They forgot who ruled these streets." 
        },
        new DialogueSentence 
        { 
            speakerName = "Michelle Sato", 
            sentence = "I have no sword right now... but fists will break bones just the same. Ryu, hold on... Mother is here." 
        }
    };

    private void Start()
    {
        if (dialogueManager == null)
            dialogueManager = DialogueManager.Instance != null ? DialogueManager.Instance : FindFirstObjectByType<DialogueManager>();

        StartCoroutine(PlayCinematicIntro());
    }

    private IEnumerator PlayCinematicIntro()
    {
        // 1. Bekukan pemain & sembunyikan UI gameplay
        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.SetFreeze(true);

        SetGameplayHUDVisible(false);

        // 2. Tampilkan dan animasikan bar hitam cinematic (turun dari atas & naik dari bawah)
        if (cinematicBarsPanel != null)
        {
            cinematicBarsPanel.SetActive(true);
            yield return StartCoroutine(AnimateBars(true));
        }

        yield return new WaitForSeconds(0.3f);

        // 3. Mulai monolog MC lewat DialogueManager
        if (dialogueManager != null)
        {
            dialogueManager.StartDialogue(openingMonologue, onComplete: () =>
            {
                StartCoroutine(EndCinematicIntro());
            });
        }
        else
        {
            StartCoroutine(EndCinematicIntro());
        }
    }

    private IEnumerator EndCinematicIntro()
    {
        // 4. Tutup bar hitam secara smooth
        if (cinematicBarsPanel != null)
        {
            yield return StartCoroutine(AnimateBars(false));
            cinematicBarsPanel.SetActive(false);
        }

        // 5. Kembalikan kontrol pemain & tampilkan kembali UI gameplay
        SetGameplayHUDVisible(true);

        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.SetFreeze(false);
    }

    private void SetGameplayHUDVisible(bool visible)
    {
        if (gameplayUIElements == null) return;

        foreach (var ui in gameplayUIElements)
        {
            if (ui != null)
                ui.SetActive(visible);
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
}
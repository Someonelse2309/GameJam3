using System.Collections;
using UnityEngine;

public class IntroCinematicController : MonoBehaviour
{
    [Header("Cinematic UI")]
    public GameObject cinematicBarsPanel;
    public RectTransform topBar;
    public RectTransform bottomBar;
    public float barHeight = 120f;
    public float transitionSpeed = 3f;

    [Header("Autoplay Settings")]
    [Tooltip("Kecepatan ketik huruf (detik). Semakin kecil nilainya, semakin cepat.")]
    public float typingSpeed = 0.038f;
    [Tooltip("Jeda membaca setelah satu kalimat selesai diketik sebelum otomatis berganti.")]
    public float sentencePause = 1.9f;

    [Header("Gameplay HUD to Hide")]
    public GameObject[] gameplayUIElements;

    [Header("References")]
    public DialogueManager dialogueManager;

    [Header("Opening Monologue Dialogue (Deep & Dramatic)")]
    public DialogueSentence[] openingMonologue = new DialogueSentence[]
    {
        new DialogueSentence 
        { 
            speakerName = "Michelle Sato", 
            sentence = "Ten years ago, I threw my katana into the river and swore I'd never stain my hands with blood again." 
        },
        new DialogueSentence 
        { 
            speakerName = "Michelle Sato", 
            sentence = "I erased 'Onikoroshi' from existence... only to become what I cherished most in this cruel world: a mother." 
        },
        new DialogueSentence 
        { 
            speakerName = "Michelle Sato", 
            sentence = "Ryu was my sanctuary. His laughter was the only thing that drowned out the screams of my past." 
        },
        new DialogueSentence 
        { 
            speakerName = "Michelle Sato", 
            sentence = "And yet... Aoyama's syndicate dragged my sins back into the moonlight. They tore him away from my arms." 
        },
        new DialogueSentence 
        { 
            speakerName = "Michelle Sato", 
            sentence = "They shattered his mind, scattered his soul across cold silicon disks, thinking a quiet mother would surrender." 
        },
        new DialogueSentence 
        { 
            speakerName = "Michelle Sato", 
            sentence = "They made a grave mistake. They forgot that when a demon lays down her blade to protect her child... she becomes something far more terrifying." 
        },
        new DialogueSentence 
        { 
            speakerName = "Michelle Sato", 
            sentence = "I will break every bone in this city with my bare fists if I have to. Hold on, Ryu... Mother is coming home." 
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
        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.SetFreeze(true);

        SetGameplayHUDVisible(false);

        if (cinematicBarsPanel != null)
        {
            cinematicBarsPanel.SetActive(true);
            yield return StartCoroutine(AnimateBars(true));
        }

        yield return new WaitForSeconds(0.4f);

        if (dialogueManager != null)
        {
            dialogueManager.StartCinematicDialogue(openingMonologue, typingSpeed, sentencePause, onComplete: () =>
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
        if (cinematicBarsPanel != null)
        {
            yield return StartCoroutine(AnimateBars(false));
            cinematicBarsPanel.SetActive(false);
        }

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
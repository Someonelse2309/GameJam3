using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class CharacterProfile
{
    public string characterName;
    public Sprite portrait;
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Image profileImage;

    [Header("Central Character Database")]
    public List<CharacterProfile> characterProfiles = new List<CharacterProfile>();

    [Header("Typewriter & Skip Settings")]
    [SerializeField] private float defaultTypingSpeed = 0.02f;
    [Tooltip("Jeda waktu minimum setelah skip agar spam klik tidak melompati dialog berikutnya.")]
    [SerializeField] private float skipCooldown = 0.25f;

    private Queue<DialogueSentence> sentences = new Queue<DialogueSentence>();
    public bool isDialogueActive { get; private set; } = false;
    public bool isEngaged { get; private set; } = false;

    // Status Mode Sinematik
    public bool isCinematicMode { get; private set; } = false;
    private float currentTypingSpeed = 0.02f;
    private float cinematicAutoAdvanceDelay = 1.8f;
    private Coroutine autoPlayCoroutine;

    private bool isTyping = false;
    private string currentFullSentence = "";
    private Coroutine typingCoroutine;
    private int lastInputFrame = -1;
    private float lastCompleteTime = -1f;

    private Action onDialogueCompleted;
    private Action onDialogueEngaged;

    private Button panelButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
            panelButton = dialoguePanel.GetComponent<Button>();
            if (panelButton != null)
            {
                panelButton.onClick.AddListener(OnPanelClicked);
            }
        }
    }

    private void Update()
    {
        // KUNCI 1: Jika sedang mode sinematik, abaikan seluruh input keyboard (E / Space)
        if (isDialogueActive && !isCinematicMode)
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                DisplayNextSentence();
            }
        }
    }

    private void OnPanelClicked()
    {
        // KUNCI 2: Jika sedang mode sinematik, abaikan klik mouse / tap layar
        if (isDialogueActive && !isCinematicMode)
        {
            DisplayNextSentence();
        }
    }

    /// <summary>
    /// Dialog gameplay standar (bisa ditekan / skip manual).
    /// </summary>
    public void StartDialogue(DialogueSentence[] dialogue, Action onComplete = null, Action onEngage = null)
    {
        StartDialogueInternal(dialogue, false, defaultTypingSpeed, 1.5f, onComplete, onEngage);
    }

    /// <summary>
    /// Dialog sinematik Intro & Outro: Autoplay murni, dilarang skip, dan SFX tombol mati.
    /// </summary>
    public void StartCinematicDialogue(DialogueSentence[] dialogue, float typingSpeed = 0.038f, float sentencePause = 1.9f, Action onComplete = null)
    {
        StartDialogueInternal(dialogue, true, typingSpeed, sentencePause, onComplete, null);
    }

    private void StartDialogueInternal(DialogueSentence[] dialogue, bool cinematicMode, float typeSpeed, float autoDelay, Action onComplete, Action onEngage)
    {
        if (isDialogueActive)
        {
            if (!isCinematicMode) DisplayNextSentence();
            return;
        }

        isDialogueActive = true;
        isEngaged = false;
        isCinematicMode = cinematicMode;
        currentTypingSpeed = typeSpeed;
        cinematicAutoAdvanceDelay = autoDelay;
        onDialogueCompleted = onComplete;
        onDialogueEngaged = onEngage;

        // KUNCI 3: Nonaktifkan komponen Button saat sinematik agar tidak bisa diklik dan tidak memutar SFX klik UI
        if (panelButton != null)
        {
            panelButton.enabled = !cinematicMode;
            panelButton.interactable = !cinematicMode;
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        sentences.Clear();

        if (dialogue != null)
        {
            foreach (DialogueSentence line in dialogue)
            {
                sentences.Enqueue(line);
            }
        }

        if (sentences.Count > 0)
        {
            DialogueSentence first = sentences.Dequeue();
            RenderSentence(first);
        }
        else
        {
            EndDialogue();
        }
    }

    public void DisplayNextSentence()
    {
        // KUNCI 4 (TERPENTING): Blokir total eksekusi jika mode sinematik sedang berjalan,
        // bahkan jika fungsi ini dipanggil paksa oleh event Button di Unity Inspector
        if (!isDialogueActive || isCinematicMode) return;

        if (Time.frameCount == lastInputFrame) return;
        lastInputFrame = Time.frameCount;

        if (!isEngaged)
        {
            isEngaged = true;
            if (PlayerMovement.Instance != null)
            {
                PlayerMovement.Instance.SetFreeze(true);
            }

            Action engageCallback = onDialogueEngaged;
            onDialogueEngaged = null;
            engageCallback?.Invoke();
        }

        if (isTyping)
        {
            CompleteCurrentSentence();
            return;
        }

        if (Time.unscaledTime - lastCompleteTime < skipCooldown)
        {
            return;
        }

        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueSentence current = sentences.Dequeue();
        RenderSentence(current);
    }

    private void RenderSentence(DialogueSentence current)
    {
        if (nameText != null) nameText.text = current.speakerName;

        Sprite matchedPortrait = GetPortraitByName(current.speakerName);
        if (profileImage != null)
        {
            if (matchedPortrait != null)
            {
                profileImage.sprite = matchedPortrait;
                profileImage.gameObject.SetActive(true);
            }
            else
            {
                profileImage.gameObject.SetActive(false);
            }
        }

        currentFullSentence = current.sentence;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
        }

        typingCoroutine = StartCoroutine(TypeSentence(current.sentence));
    }

    private Sprite GetPortraitByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        CharacterProfile profile = characterProfiles.Find(p => p.characterName.Trim().Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
        return profile != null ? profile.portrait : null;
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = sentence;
        dialogueText.maxVisibleCharacters = 0;

        yield return null;

        int totalChars = sentence.Length;
        for (int i = 0; i <= totalChars; i++)
        {
            if (!isTyping) yield break;
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(currentTypingSpeed);
        }

        dialogueText.maxVisibleCharacters = int.MaxValue;
        isTyping = false;
        lastCompleteTime = 0f;

        // Jika mode sinematik, otomatis pindah ke dialog berikutnya setelah jeda waktu baca
        if (isCinematicMode)
        {
            autoPlayCoroutine = StartCoroutine(AutoAdvanceRoutine());
        }
    }

    private IEnumerator AutoAdvanceRoutine()
    {
        yield return new WaitForSecondsRealtime(cinematicAutoAdvanceDelay);

        if (sentences.Count > 0)
        {
            DialogueSentence next = sentences.Dequeue();
            RenderSentence(next);
        }
        else
        {
            EndDialogue();
        }
    }

    private void CompleteCurrentSentence()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.text = currentFullSentence;
        dialogueText.maxVisibleCharacters = int.MaxValue;
        isTyping = false;
        lastCompleteTime = Time.unscaledTime;
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        isEngaged = false;
        isTyping = false;
        isCinematicMode = false;

        // Kembalikan tombol ke kondisi aktif untuk dialog NPC biasa
        if (panelButton != null)
        {
            panelButton.enabled = true;
            panelButton.interactable = true;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.SetFreeze(false);
        }

        Action callback = onDialogueCompleted;
        onDialogueCompleted = null;
        onDialogueEngaged = null;
        callback?.Invoke();
    }

    public void CancelDialogue()
    {
        isDialogueActive = false;
        isEngaged = false;
        isTyping = false;
        isCinematicMode = false;
        sentences.Clear();

        if (panelButton != null)
        {
            panelButton.enabled = true;
            panelButton.interactable = true;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.SetFreeze(false);
        }

        onDialogueCompleted = null;
        onDialogueEngaged = null;
    }
}
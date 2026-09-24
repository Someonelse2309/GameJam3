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
    [SerializeField] private float typingSpeed = 0.02f;
    [Tooltip("Jeda waktu minimum setelah skip agar spam klik tidak langsung melompati dialog berikutnya.")]
    [SerializeField] private float skipCooldown = 0.25f;

    private Queue<DialogueSentence> sentences = new Queue<DialogueSentence>();
    public bool isDialogueActive { get; private set; } = false;
    public bool isEngaged { get; private set; } = false;

    private bool isTyping = false;
    private string currentFullSentence = "";
    private Coroutine typingCoroutine;
    private int lastInputFrame = -1;
    private float lastCompleteTime = -1f;

    private Action onDialogueCompleted;
    private Action onDialogueEngaged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
            Button panelBtn = dialoguePanel.GetComponent<Button>();
            if (panelBtn != null) panelBtn.onClick.AddListener(DisplayNextSentence);
        }
    }

    private void Update()
    {
        if (isDialogueActive && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)))
        {
            DisplayNextSentence();
        }
    }

    public void StartDialogue(DialogueSentence[] dialogue, Action onComplete = null, Action onEngage = null)
    {
        if (isDialogueActive)
        {
            DisplayNextSentence();
            return;
        }

        isDialogueActive = true;
        isEngaged = false;
        onDialogueCompleted = onComplete;
        onDialogueEngaged = onEngage;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        sentences.Clear();

        foreach (DialogueSentence line in dialogue)
        {
            sentences.Enqueue(line);
        }

        if (sentences.Count > 0)
        {
            DialogueSentence first = sentences.Dequeue();
            RenderSentence(first);
        }
    }

    public void DisplayNextSentence()
    {
        if (!isDialogueActive) return;

        // Cegah eksekusi ganda dalam 1 frame (misal: Space memicu Input.GetKeyDown sekaligus Button.onClick)
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

        // 1. Jika teks masih mengetik, klik pertama WAJIB menyelesaikan kalimat
        if (isTyping)
        {
            CompleteCurrentSentence();
            return;
        }

        // 2. Proteksi spam: jika baru saja skip paksa, klik kedua ditahan selama jeda cooldown
        if (Time.unscaledTime - lastCompleteTime < skipCooldown)
        {
            return;
        }

        // 3. Jika teks sudah penuh dan cooldown selesai, baru buka dialog berikutnya
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

        yield return null; // Tunggu 1 frame agar TextMeshPro menghitung layout teks

        int totalChars = sentence.Length;
        for (int i = 0; i <= totalChars; i++)
        {
            if (!isTyping) yield break;
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        dialogueText.maxVisibleCharacters = int.MaxValue;
        isTyping = false;
        lastCompleteTime = 0f; // Selesai alami tanpa paksaan, bebas klik kapan saja
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
        lastCompleteTime = Time.unscaledTime; // Catat waktu skip paksa untuk cooldown
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        isEngaged = false;
        isTyping = false;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
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
        sentences.Clear();

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
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
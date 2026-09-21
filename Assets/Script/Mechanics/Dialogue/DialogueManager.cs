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

    private Queue<DialogueSentence> sentences = new Queue<DialogueSentence>();
    public bool isDialogueActive { get; private set; } = false;
    private Action onDialogueCompleted;
    private bool isTyping = false;
    private string currentFullSentence = "";

    private void Awake()
    {
        // 1. Pastikan Instance singleton selalu terdaftar
        Instance = this;

        // 2. Kunci 60 FPS
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (isDialogueActive && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)))
        {
            DisplayNextSentence();
        }
    }

    public void StartDialogue(DialogueSentence[] dialogue, Action onComplete = null)
    {
        if (dialogue == null || dialogue.Length == 0) return;

        if (isDialogueActive)
        {
            DisplayNextSentence();
            return;
        }

        isDialogueActive = true;
        onDialogueCompleted = onComplete;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        sentences.Clear();

        foreach (DialogueSentence line in dialogue)
        {
            sentences.Enqueue(line);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        // Jika sedang mengetik teks berjalan, tekan lagi untuk langsung menampilkan teks penuh
        if (isTyping)
        {
            StopAllCoroutines();
            if (dialogueText != null) dialogueText.text = currentFullSentence;
            isTyping = false;
            return;
        }

        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueSentence current = sentences.Dequeue();

        if (nameText != null) nameText.text = current.speakerName;

        // Update foto profil karakter otomatis
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

        StopAllCoroutines();
        StartCoroutine(TypeSentence(current.sentence));
    }

    private Sprite GetPortraitByName(string name)
    {
        if (characterProfiles == null || string.IsNullOrEmpty(name)) return null;

        CharacterProfile profile = characterProfiles.Find(p => 
            p != null && 
            !string.IsNullOrEmpty(p.characterName) && 
            p.characterName.Trim().Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));

        return profile != null ? profile.portrait : null;
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        currentFullSentence = sentence;
        if (dialogueText != null)
        {
            dialogueText.text = "";
            foreach (char letter in sentence.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(0.02f);
            }
        }
        isTyping = false;
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        isTyping = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        Action callback = onDialogueCompleted;
        onDialogueCompleted = null;
        callback?.Invoke();
    }
}
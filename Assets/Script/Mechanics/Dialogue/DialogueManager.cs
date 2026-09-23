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
    public bool isEngaged { get; private set; } = false; // True jika player sudah menekan untuk lanjut bicara
    private Action onDialogueCompleted;
    private Action onDialogueEngaged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
            // Tambahkan listener jika DialoguePanel menggunakan komponen Button
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
        isEngaged = false; // Player belum menekan tombol (hanya preview lewat)
        onDialogueCompleted = onComplete;
        onDialogueEngaged = onEngage;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        sentences.Clear();

        foreach (DialogueSentence line in dialogue)
        {
            sentences.Enqueue(line);
        }

        // Tampilkan kalimat pertama (MC masih bebas jalan jika belum engaged)
        if (sentences.Count > 0)
        {
            DialogueSentence first = sentences.Dequeue();
            RenderSentence(first);
        }
    }

    public void DisplayNextSentence()
    {
        if (!isDialogueActive) return;

        // Saat player menekan tombol pertama kali: Bekukan MC & picu onEngage
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

        StopAllCoroutines();
        StartCoroutine(TypeSentence(current.sentence));
    }

    private Sprite GetPortraitByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        CharacterProfile profile = characterProfiles.Find(p => p.characterName.Trim().Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
        return profile != null ? profile.portrait : null;
    }

    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.02f);
        }
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        isEngaged = false;
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

    // Dipanggil saat player berjalan keluar dari area trigger tanpa menekan dialog
    public void CancelDialogue()
    {
        isDialogueActive = false;
        isEngaged = false;
        sentences.Clear();
        StopAllCoroutines();

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.SetFreeze(false);
        }

        onDialogueCompleted = null;
        onDialogueEngaged = null;
    }
}
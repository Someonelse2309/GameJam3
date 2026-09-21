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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

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
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueSentence current = sentences.Dequeue();

        // Tampilkan Nama
        if (nameText != null) nameText.text = current.speakerName;

        // Cari Foto Profil Otomatis Berdasarkan Nama
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
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        Action callback = onDialogueCompleted;
        onDialogueCompleted = null;
        callback?.Invoke();
    }
}
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
    public static DialogueManager Instance;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Image profileImage;

    [Header("Character Profiles Database")]
    public List<CharacterProfile> characterProfiles; // Daftarkan PP karakter di sini sekali saja

    private Queue<DialogueLine> sentences = new Queue<DialogueLine>();
    private Dictionary<string, Sprite> profileDict = new Dictionary<string, Sprite>();
    private bool isDialogueActive = false;

    void Awake()
    {
        Instance = this;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // Buat Kamus Data Profil Karakter
        InitProfileDictionary();
    }

    void InitProfileDictionary()
    {
        profileDict.Clear();
        foreach (var profile in characterProfiles)
        {
            if (!string.IsNullOrEmpty(profile.characterName) && !profileDict.ContainsKey(profile.characterName))
            {
                profileDict.Add(profile.characterName, profile.portrait);
            }
        }
    }

    void Update()
    {
        if (isDialogueActive && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)))
        {
            DisplayNextSentence();
        }
    }

    public void StartDialogue(DialogueLine[] dialogue)
    {
        if (isDialogueActive)
        {
            DisplayNextSentence();
            return;
        }

        isDialogueActive = true;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        sentences.Clear();

        foreach (DialogueLine line in dialogue)
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

        DialogueLine current = sentences.Dequeue();

        // 1. Set Nama Karakter
        if (nameText != null) nameText.text = current.characterName;

        // 2. Cari Foto Profil Otomatis Berdasarkan Nama
        Sprite portraitSprite = null;

        // Gunakan custom portrait jika diisi khusus pada line, jika tidak cari di database
        if (current.characterPortrait != null)
        {
            portraitSprite = current.characterPortrait;
        }
        else if (!string.IsNullOrEmpty(current.characterName) && profileDict.TryGetValue(current.characterName, out Sprite foundSprite))
        {
            portraitSprite = foundSprite;
        }

        // 3. Tampilkan Foto Profil
        if (profileImage != null)
        {
            if (portraitSprite != null)
            {
                profileImage.sprite = portraitSprite;
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

    IEnumerator TypeSentence(string sentence)
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
    }
}
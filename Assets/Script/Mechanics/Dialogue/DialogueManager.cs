using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TMP_Text nameText;
    public TMP_Text dialogueText;

    private Queue<DialogueSentence> sentences = new Queue<DialogueSentence>();
    private bool isDialogueActive = false;

    void Awake()
{
    Instance = this;
    if (dialoguePanel != null)
    {
        dialoguePanel.SetActive(false);
    }
}

    public void StartDialogue(DialogueSentence[] dialogue)
    {
        if (isDialogueActive)
        {
            DisplayNextSentence();
            return;
        }

        isDialogueActive = true;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        sentences.Clear();

        foreach (DialogueSentence sentence in dialogue)
        {
            sentences.Enqueue(sentence);
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
        if (nameText != null) nameText.text = current.speakerName;
        
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
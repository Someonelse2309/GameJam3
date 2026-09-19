using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class DialogueManagerEP1 : MonoBehaviour
{
    public static DialogueManagerEP1 instanceEP1;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public GameObject continueIndicator;

    [Header("Settings")]
    public float textSpeed = 0.03f;
    public float autoAdvanceDelay = 2f;
    public KeyCode advanceKey = KeyCode.Space;

    private DialogueDataEP1 currentDialogue;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool isDialogueActive = false;
    private Coroutine typingCoroutine;
    private Coroutine autoAdvanceCoroutine;

    public Action OnDialogueStart;
    public Action OnDialogueEnd;
    public Action<int> OnLineChanged;

    private void Awake()
    {
        if (instanceEP1 == null)
        {
            instanceEP1 = this;
            Debug.Log("DialogueManagerEP1: Instance created");
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("DialogueManagerEP1: Instance already exists, destroying duplicate");
            return;
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (!isDialogueActive) return;

        if (autoAdvanceDelay == 0 && (Input.GetKeyDown(advanceKey) || Input.GetMouseButtonDown(0)))
        {
            AdvanceDialogue();
        }
    }

    public void StartDialogue(DialogueDataEP1 dialogue)
    {
        if (dialogue == null || dialogue.lines == null || dialogue.lines.Length == 0)
        {
            Debug.LogError("DialogueManagerEP1.StartDialogue: dialogue NULL atau tidak punya lines!");
            return;
        }

        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueActive = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (OnDialogueStart != null)
            OnDialogueStart.Invoke();

        ShowLine(currentLineIndex);
    }

    private void ShowLine(int index)
    {
        if (currentDialogue == null || index >= currentDialogue.lines.Length) return;

        DialogueDataEP1.DialogueLine line = currentDialogue.lines[index];

        if (nameText != null)
            nameText.text = line.speakerName;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(line.text));

        if (line.voiceLine != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlayVoice(0);
        }

        if (OnLineChanged != null)
            OnLineChanged.Invoke(index);

        if (autoAdvanceCoroutine != null)
            StopCoroutine(autoAdvanceCoroutine);

        if (autoAdvanceDelay > 0)
        {
            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfterDelay(autoAdvanceDelay));
        }
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        if (dialogueText != null)
            dialogueText.text = "";

        foreach (char c in text)
        {
            if (dialogueText != null)
                dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;

        if (continueIndicator != null)
        {
            if (autoAdvanceDelay > 0)
                continueIndicator.SetActive(false);
            else
                continueIndicator.SetActive(true);
        }
    }

    private IEnumerator AutoAdvanceAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        AdvanceDialogue();
    }

    private void AdvanceDialogue()
    {
        if (isTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            if (dialogueText != null && currentDialogue != null)
                dialogueText.text = currentDialogue.lines[currentLineIndex].text;

            isTyping = false;

            if (continueIndicator != null && autoAdvanceDelay == 0)
                continueIndicator.SetActive(true);
        }
        else
        {
            if (continueIndicator != null)
                continueIndicator.SetActive(false);

            DialogueDataEP1.DialogueLine currentLine = currentDialogue.lines[currentLineIndex];

            if (currentLine.delayAfter > 0)
            {
                StartCoroutine(DelayAndNext(currentLine.delayAfter));
            }
            else
            {
                NextLine();
            }
        }
    }

    private IEnumerator DelayAndNext(float delay)
    {
        yield return new WaitForSeconds(delay);
        NextLine();
    }

    private void NextLine()
    {
        currentLineIndex++;

        if (currentLineIndex >= currentDialogue.lines.Length)
        {
            EndDialogue();
        }
        else
        {
            ShowLine(currentLineIndex);
        }
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        currentDialogue = null;
        currentLineIndex = 0;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (OnDialogueEnd != null)
            OnDialogueEnd.Invoke();

        Debug.Log("DialogueManagerEP1: Dialogue ended");
    }

    public void ForceEndDialogue()
    {
        EndDialogue();
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    public int GetCurrentLineIndex()
    {
        return currentLineIndex;
    }

    public int GetTotalLines()
    {
        if (currentDialogue == null || currentDialogue.lines == null) return 0;
        return currentDialogue.lines.Length;
    }
}

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class DialogueManagerEP1 : MonoBehaviour
{
    public static DialogueManagerEP1 instanceEP1;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public GameObject continueIndicator;
    public Image characterImage;

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

    // Track which trigger started this dialogue
    private DialogueTriggerEP1 currentTrigger;

    public Action OnDialogueStart;
    public Action OnDialogueEnd;
    public Action<int> OnLineChanged;

    private void Awake()
    {
        if (instanceEP1 == null)
        {
            instanceEP1 = this;
            // Debug.Log("DialogueManagerEP1: Instance created");
        }
        else
        {
            Destroy(gameObject);
            // Debug.LogWarning("DialogueManagerEP1: Instance already exists, destroying duplicate");
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
        StartDialogue(dialogue, null);
    }

    public void StartDialogue(DialogueDataEP1 dialogue, DialogueTriggerEP1 trigger)
    {
        if (dialogue == null || dialogue.lines == null || dialogue.lines.Length == 0)
        {
            // Debug.LogError("DialogueManagerEP1.StartDialogue: dialogue NULL atau tidak punya lines!");
            return;
        }

        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueActive = true;
        currentTrigger = trigger;

        // Debug.Log($"DialogueManagerEP1: StartDialogue - dialogue={dialogue.name}, trigger={trigger?.name}");

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (OnDialogueStart != null)
            OnDialogueStart.Invoke();

        ShowLine(currentLineIndex);
    }

    public DialogueTriggerEP1 GetCurrentTrigger()
    {
        return currentTrigger;
    }

    private void ShowLine(int index)
    {
        if (currentDialogue == null || index >= currentDialogue.lines.Length) return;

        DialogueDataEP1.DialogueLine line = currentDialogue.lines[index];

        // Safety check
        if (line == null || string.IsNullOrEmpty(line.text))
        {
            // Debug.LogWarning($"DialogueManagerEP1: Line {index} is null or empty, skipping");
            NextLine();
            return;
        }

        if (nameText != null)
            nameText.text = line.speakerName;

        // Avatar
        if (characterImage != null)
        {
            if (line.characterImage != null)
            {
                characterImage.sprite = line.characterImage;
                characterImage.gameObject.SetActive(true);
            }
            else
            {
                characterImage.gameObject.SetActive(false);
            }
        }

        // Debug.Log($"DialogueManagerEP1: Showing line {index} - {line.speakerName}: {line.text}");

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
        // Debug.Log($"TypeText: Completed typing line (text length: {text.Length})");

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
        // Debug.Log($"AutoAdvanceAfterDelay: Waiting {delay} seconds");
        yield return new WaitForSeconds(delay);
        // Debug.Log("AutoAdvanceAfterDelay: Delay complete, advancing");
        AdvanceDialogue();
    }

    private void AdvanceDialogue()
    {
        if (isTyping)
        {
            // Saat typing, langsung tampilkan full text dan stop typing
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            if (dialogueText != null && currentDialogue != null)
                dialogueText.text = currentDialogue.lines[currentLineIndex].text;

            isTyping = false;

            if (continueIndicator != null && autoAdvanceDelay == 0)
                continueIndicator.SetActive(true);

            // Kalau auto-advance, tunggu delayAfter sebelum next
            if (autoAdvanceDelay > 0 && currentDialogue != null)
            {
                float delay = currentDialogue.lines[currentLineIndex].delayAfter;
                StartCoroutine(DelayAndNext(delay));
            }
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
        // Debug.Log($"DialogueManagerEP1: Moving to line {currentLineIndex} (total: {currentDialogue?.lines?.Length})");

        if (currentLineIndex >= currentDialogue.lines.Length)
        {
            // Debug.Log("DialogueManagerEP1: All lines complete, ending dialogue");
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

        // Debug.Log($"DialogueManagerEP1: EndDialogue - currentTrigger={currentTrigger?.name}");

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (OnDialogueEnd != null)
            OnDialogueEnd.Invoke();

        currentTrigger = null; // Clear after invoking

        // Debug.Log("DialogueManagerEP1: Dialogue ended");
    }

    public void ForceEndDialogue()
    {
        EndDialogue();
    }

    public void PauseDialogue()
    {
        isDialogueActive = false;
        // Debug.Log("DialogueManagerEP1: Dialogue paused");
    }

    public void ResumeDialogue()
    {
        if (currentDialogue == null)
        {
            // Debug.LogWarning("DialogueManagerEP1: ResumeDialogue called but currentDialogue is null");
            return;
        }

        isDialogueActive = true;
        // Debug.Log($"DialogueManagerEP1: Resuming dialogue from line {currentLineIndex}");
        ShowLine(currentLineIndex);
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

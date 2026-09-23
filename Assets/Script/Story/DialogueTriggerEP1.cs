using UnityEngine;
using System.Collections.Generic;

public class DialogueTriggerEP1 : MonoBehaviour
{
    public DialogueDataEP1 dialogueData;

    [Header("Settings")]
    public bool triggerOnce = true;
    public bool deactivateAfterTrigger = false;
    public int momenIndex = 0; // Index untuk tracking save file

    [Header("AutoWalk - Character yang auto jalan")]
    public AutoWalk autoWalkTarget;

    [Header("Waypoints untuk Momen ini (2 waypoints: start & end)")]
    public Transform[] waypointsOverride;

    [Header("Ryu Separation")]
    public FollowTarget ryuFollowTarget;
    public AutoWalk ryuAutoWalk;
    public bool startRyuAutoWalkOnDialogueEnd = false;
    public bool hideRyuAfterAutoWalk = false;

    [Header("NPC Show/Hide")]
    public GameObject[] npcsToShow;
    public GameObject[] npcsToHide;

    [Header("Next Trigger (Sequential)")]
    public GameObject nextTriggerToActivate; // Trigger yang muncul setelah ini selesai

    [Header("Scene Transition")]
    public bool fadeToBlackOnEnter = false;
    public float delayBeforeFade = 0f;

    [Header("Screen Shake (untuk momen tragedi)")]
    public bool enableScreenShake = false;
    public int shakeOnLineIndex = 8; // Line ke berapa shake aktif
    public float shakeDuration = 0.8f;
    public float shakeIntensity = 1.5f;

    [Header("Effects After Dialogue End")]
    public bool playSFXOnEnd = false;
    public AudioClip sfxToPlayOnEnd;
    public bool fadeToBlackOnEnd = false;
    public float fadeToBlackDelay = 0f;
    public bool fadeBackAfterBlack = false; // Fade dari hitam balik ke normal
    public float fadeBackDelay = 2f; // Waktu sebelum fade back

    private bool hasTriggered = false;
    private bool dialogueEndedHandled = false; // Prevent double trigger

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && CanTrigger())
        {
            Debug.Log("DialogueTriggerEP1: Player masuk trigger - " + gameObject.name);
            TriggerDialogue();
        }
    }

    public void TriggerDialogue()
    {
        if (CanTrigger())
        {
            Debug.Log("DialogueTriggerEP1: Triggering dialogue - " + (dialogueData != null ? dialogueData.name : "NULL"));

            hasTriggered = true;
            dialogueEndedHandled = false; // Reset flag

            // Fade to black if enabled
            if (fadeToBlackOnEnter)
            {
                StartCoroutine(FadeToBlackSequence());
            }

            if (deactivateAfterTrigger)
                gameObject.SetActive(false);

            // Show NPCs
            ShowNPCs();

            if (autoWalkTarget != null)
            {
                // Override waypoints jika ada
                if (waypointsOverride != null && waypointsOverride.Length >= 2)
                {
                    autoWalkTarget.SetWaypoints(waypointsOverride);
                    Debug.Log("DialogueTriggerEP1: Using override waypoints - " + waypointsOverride.Length + " waypoints");
                }

                Debug.Log("DialogueTriggerEP1: Starting AutoWalk - " + autoWalkTarget.gameObject.name);
                autoWalkTarget.StartWalking();
            }

            if (DialogueManagerEP1.instanceEP1 == null)
            {
                Debug.LogError("DialogueTriggerEP1 ERROR: DialogueManagerEP1.instanceEP1 NULL!");
                return;
            }

            if (dialogueData == null)
            {
                Debug.LogError("DialogueTriggerEP1 ERROR: dialogueData NULL!");
                return;
            }

            // Subscribe to dialogue end
            DialogueManagerEP1.instanceEP1.OnDialogueEnd += OnDialogueEnded;

            // Subscribe to line change (untuk screen shake)
            DialogueManagerEP1.instanceEP1.OnLineChanged += OnLineChanged;

            DialogueManagerEP1.instanceEP1.StartDialogue(dialogueData, this);
            Debug.Log($"TriggerDialogue: Started dialogue with trigger={gameObject.name}");
        }
    }

    private void OnDialogueEnded()
    {
        Debug.Log($"OnDialogueEnded: Called by {gameObject.name}, handled={dialogueEndedHandled}");

        // Prevent double trigger
        if (dialogueEndedHandled)
        {
            Debug.Log($"OnDialogueEnded: Already handled, skipping - {gameObject.name}");
            return;
        }

        // Cek apakah ini trigger yang memulai dialogue ini
        if (DialogueManagerEP1.instanceEP1 != null)
        {
            DialogueTriggerEP1 currentTrigger = DialogueManagerEP1.instanceEP1.GetCurrentTrigger();
            Debug.Log($"OnDialogueEnded: currentTrigger={currentTrigger?.name}, this={gameObject.name}");

            if (currentTrigger != this)
            {
                Debug.Log($"OnDialogueEnded: Not my dialogue, skipping");
                return;
            }
        }

        dialogueEndedHandled = true;
        Debug.Log("DialogueTriggerEP1: Processing dialogue end - " + gameObject.name);

        // Unsubscribe
        if (DialogueManagerEP1.instanceEP1 != null)
        {
            DialogueManagerEP1.instanceEP1.OnDialogueEnd -= OnDialogueEnded;
            DialogueManagerEP1.instanceEP1.OnLineChanged -= OnLineChanged;
        }

        // ===== EFFECTS AFTER DIALOGUE END - JALANKAN TERLEBIH DAHULU =====

        Debug.Log($"OnDialogueEnded Effects: enableScreenShake={enableScreenShake}, playSFXOnEnd={playSFXOnEnd}, fadeToBlackOnEnd={fadeToBlackOnEnd}");

        // Screen shake on end
        if (enableScreenShake)
        {
            ScreenShake shaker = UnityEngine.Object.FindAnyObjectByType<ScreenShake>();
            if (shaker != null)
            {
                Debug.Log("DialogueTriggerEP1: Triggering screen shake on dialogue end");
                shaker.TriggerShake(shakeDuration, shakeIntensity);
            }
        }

        // Play SFX on end
        if (playSFXOnEnd && sfxToPlayOnEnd != null)
        {
            if (AudioManager.instance != null)
            {
                Debug.Log("DialogueTriggerEP1: Playing SFX on dialogue end - " + sfxToPlayOnEnd.name);
                AudioManager.instance.PlaySFX(sfxToPlayOnEnd);
            }
        }

        // Fade to black on end - LANGSUNG CALL TANPA COROUTINE
        if (fadeToBlackOnEnd)
        {
            Debug.Log("DialogueTriggerEP1: Calling FadeToBlack DIRECT...");

            SceneTransition transition = null;
            SceneTransition[] allTransitions = FindObjectsByType<SceneTransition>(FindObjectsInactive.Exclude);
            foreach (var t in allTransitions)
            {
                if (t != null)
                {
                    transition = t;
                    break;
                }
            }

            if (transition != null)
            {
                Debug.Log($"DialogueTriggerEP1: Calling FadeInBlack on {transition.name}");
                transition.FadeInBlack(null);

                // Fade back to normal setelah beberapa waktu
                if (fadeBackAfterBlack)
                {
                    Debug.Log($"DialogueTriggerEP1: Scheduling fade back in {fadeBackDelay}s...");
                    // Pakai Invoke untuk jadwalkan fade back
                    Invoke(nameof(FadeBackToNormal), fadeBackDelay);
                }
            }
            else
            {
                Debug.LogError("DialogueTriggerEP1: SceneTransition NOT FOUND!");
            }
        }

        // ===== DEACTIVATE SETELAH EFFECTS =====

        // Deactivate trigger AFTER effects complete
        if (fadeToBlackOnEnd && fadeBackAfterBlack)
        {
            // Tunggu fade back selesai baru deactivate
            float totalDelay = fadeBackDelay + 1f; // +1f untuk fade out duration
            Debug.Log($"DialogueTriggerEP1: Scheduling deactivation in {totalDelay}s...");
            Invoke(nameof(DeactivateTrigger), totalDelay);
        }
        else if (fadeToBlackOnEnd)
        {
            // Tanpa fade back, deactivate setelah fade in selesai (~1s)
            Debug.Log("DialogueTriggerEP1: Scheduling deactivation in 1.5s...");
            Invoke(nameof(DeactivateTrigger), 1.5f);
        }
        else if (deactivateAfterTrigger)
        {
            DeactivateTrigger();
        }

        // Activate next trigger
        if (nextTriggerToActivate != null)
        {
            nextTriggerToActivate.SetActive(true);
            Debug.Log("Activated next trigger: " + nextTriggerToActivate.name);
        }

        // Handle Ryu Separation
        if (startRyuAutoWalkOnDialogueEnd)
        {
            StartRyuSeparation();
        }

        // Hide NPCs - TAPI JANGAN hide Ryu kalau hideRyuAfterAutoWalk = true
        if (hideRyuAfterAutoWalk && npcsToHide != null)
        {
            // Filter out Ryu from npcsToHide
            List<GameObject> npcsToHideFiltered = new List<GameObject>();
            foreach (GameObject npc in npcsToHide)
            {
                if (npc != null && npc.name != "Ryu")
                {
                    npcsToHideFiltered.Add(npc);
                }
            }
            HideNPCsFiltered(npcsToHideFiltered.ToArray());
        }
        else
        {
            HideNPCs();
        }
    }

    private System.Collections.IEnumerator DeactivateAfterDelay(float delay)
    {
        Debug.Log($"DeactivateAfterDelay: Waiting {delay}s before deactivating");
        yield return new WaitForSeconds(delay);

        if (deactivateAfterTrigger)
        {
            gameObject.SetActive(false);
            Debug.Log("Deactivated trigger after delay: " + gameObject.name);
        }
    }

    private void DeactivateTrigger()
    {
        if (deactivateAfterTrigger)
        {
            gameObject.SetActive(false);
            Debug.Log("Deactivated trigger: " + gameObject.name);
        }
    }

    private void FadeBackToNormal()
    {
        Debug.Log("DialogueTriggerEP1: FadeBackToNormal called");

        SceneTransition transition = null;
        SceneTransition[] allTransitions = FindObjectsByType<SceneTransition>(FindObjectsInactive.Exclude);
        foreach (var t in allTransitions)
        {
            if (t != null)
            {
                transition = t;
                break;
            }
        }

        if (transition != null)
        {
            Debug.Log("DialogueTriggerEP1: Fading back to normal...");
            transition.FadeOutBlack(null);
        }
    }

    private void HideNPCsFiltered(GameObject[] npcs)
    {
        if (npcs != null)
        {
            foreach (GameObject npc in npcs)
            {
                if (npc != null)
                {
                    AutoWalk npcAutoWalk = npc.GetComponent<AutoWalk>();
                    if (npcAutoWalk != null)
                    {
                        npcAutoWalk.StopWalking();
                    }
                    npc.SetActive(false);
                    Debug.Log("Hidden NPC: " + npc.name);
                }
            }
        }
    }

    private void StartRyuSeparation()
    {
        Debug.Log("DialogueTriggerEP1: Starting Ryu separation");

        // Stop Ryu following Michelle
        if (ryuFollowTarget != null)
        {
            ryuFollowTarget.StopFollowing();
            Debug.Log("Ryu stopped following Michelle");
        }

        // Start Ryu AutoWalk
        if (ryuAutoWalk != null)
        {
            ryuAutoWalk.StartWalking();
            Debug.Log("Ryu started AutoWalk");

            // Subscribe to AutoWalk completion
            ryuAutoWalk.onAutoWalkComplete.AddListener(OnRyuAutoWalkComplete);
        }
    }

    private void OnRyuAutoWalkComplete()
    {
        Debug.Log("DialogueTriggerEP1: Ryu AutoWalk complete");

        // Unsubscribe
        if (ryuAutoWalk != null)
        {
            ryuAutoWalk.onAutoWalkComplete.RemoveListener(OnRyuAutoWalkComplete);
        }

        // Hide Ryu
        if (hideRyuAfterAutoWalk && ryuAutoWalk != null)
        {
            ryuAutoWalk.gameObject.SetActive(false);
            Debug.Log("Ryu hidden after AutoWalk complete");
        }
    }

    private void ShowNPCs()
    {
        if (npcsToShow != null)
        {
            foreach (GameObject npc in npcsToShow)
            {
                if (npc != null)
                {
                    npc.SetActive(true);
                    Debug.Log("Showed NPC: " + npc.name);

                    // Start AutoWalk untuk NPC jika ada
                    AutoWalk npcAutoWalk = npc.GetComponent<AutoWalk>();
                    if (npcAutoWalk != null)
                    {
                        if (npcAutoWalk.waypoints == null || npcAutoWalk.waypoints.Length == 0)
                        {
                            Debug.LogWarning($"NPC {npc.name} has AutoWalk but NO WAYPOINTS!");
                        }
                        else
                        {
                            npcAutoWalk.StartWalking();
                            Debug.Log($"Started AutoWalk for {npc.name}, waypoints: {npcAutoWalk.waypoints.Length}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"NPC {npc.name} has NO AutoWalk component!");
                    }
                }
            }
        }
    }

    private void HideNPCs()
    {
        if (npcsToHide != null)
        {
            foreach (GameObject npc in npcsToHide)
            {
                if (npc != null)
                {
                    // Stop AutoWalk dulu
                    AutoWalk npcAutoWalk = npc.GetComponent<AutoWalk>();
                    if (npcAutoWalk != null)
                    {
                        npcAutoWalk.StopWalking();
                    }

                    npc.SetActive(false);
                    Debug.Log("Hidden NPC: " + npc.name);
                }
            }
        }
    }

    private bool CanTrigger()
    {
        if (triggerOnce && hasTriggered)
        {
            Debug.Log("DialogueTriggerEP1: Sudah pernah trigger, skip.");
            return false;
        }
        if (DialogueManagerEP1.instanceEP1 != null && DialogueManagerEP1.instanceEP1.IsDialogueActive())
        {
            Debug.Log("DialogueTriggerEP1: Dialogue sedang aktif, skip.");
            return false;
        }

        return true;
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
        gameObject.SetActive(true);
    }

    private System.Collections.IEnumerator FadeToBlackSequence()
    {
        if (delayBeforeFade > 0)
            yield return new WaitForSeconds(delayBeforeFade);

        SceneTransition transition = UnityEngine.Object.FindAnyObjectByType<SceneTransition>();
        if (transition != null)
        {
            transition.FadeInBlack(() => {
                Debug.Log("Screen faded to black!");
            });
        }
    }

    private void OnLineChanged(int lineIndex)
    {
        // Cek apakah ini trigger yang sedang aktif
        if (DialogueManagerEP1.instanceEP1 != null)
        {
            DialogueTriggerEP1 currentTrigger = DialogueManagerEP1.instanceEP1.GetCurrentTrigger();
            if (currentTrigger != this) return;
        }

        // Ambil line data
        if (dialogueData == null || dialogueData.lines == null || lineIndex >= dialogueData.lines.Length)
            return;

        DialogueDataEP1.DialogueLine line = dialogueData.lines[lineIndex];

        // ===== TRIGGER EFFECTS =====

        // Screen Shake
        if (line.triggerScreenShake || (enableScreenShake && lineIndex == shakeOnLineIndex))
        {
            Debug.Log($"DialogueTriggerEP1: Triggering screen shake on line {lineIndex}");
            ScreenShake shaker = UnityEngine.Object.FindAnyObjectByType<ScreenShake>();
            if (shaker != null)
            {
                shaker.TriggerShake(shakeDuration, shakeIntensity);
            }
        }

        // Sound Effect (SFX)
        if (line.soundEffect != null && AudioManager.instance != null)
        {
            Debug.Log($"DialogueTriggerEP1: Playing SFX - {line.soundEffect.name}");
            AudioManager.instance.PlaySFX(line.soundEffect);
        }

        // Fade to Black
        if (line.triggerFadeToBlack)
        {
            Debug.Log($"DialogueTriggerEP1: Triggering fade to black on line {lineIndex}");
            StartCoroutine(FadeToBlackAfterLine());
        }
    }

    private System.Collections.IEnumerator FadeToBlackAfterLine()
    {
        // Tunggu sedikit biar dialogue selesai
        yield return new WaitForSeconds(1f);

        SceneTransition transition = UnityEngine.Object.FindAnyObjectByType<SceneTransition>();
        if (transition != null)
        {
            transition.FadeInBlack(() => {
                Debug.Log("Faded to black!");
                // Bisa trigger next momen atau scene transition di sini
            });
        }
    }
}

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
    public bool autoStartAutoWalkOnShow = false;

    [Header("Next Trigger (Sequential)")]
    public GameObject nextTriggerToActivate;
    public float delayBeforeActivateNext = 0f;

    [Header("Scene Transition")]
    public bool fadeToBlackOnEnter = false;
    public float delayBeforeFade = 0f;

    [Header("Screen Shake")]
    public bool enableScreenShake = false;
    public int shakeOnLineIndex = 8;
    public float shakeDuration = 0.8f;
    public float shakeIntensity = 1.5f;

    [Header("Transformasi Setelah Dialogue")]
    public GameObject[] objectsToHideAfterDialogue;
    public GameObject[] objectsToShowAfterDialogue;

    [Header("Line-Based Events")]
    public GameObject objectToHideOnLine;
    public int hideObjectOnLineIndex = 2;

    [Header("AutoWalk NPCs on Line")]
    public AutoWalk[] npcsToAutoWalkOnLine;
    public int autoWalkOnLineIndex = 3;
    public bool resumeDialogueAfterAutoWalk = true;

    [Header("Effects After Dialogue End")]
    public bool playSFXOnEnd = false;
    public AudioClip sfxToPlayOnEnd;
    public bool fadeToBlackOnEnd = false;
    public float fadeToBlackDelay = 0f;
    public bool fadeBackAfterBlack = false;
    public float fadeBackDelay = 2f;

    [Header("Pilih Sistem Fade")]
    public bool useSceneTransition = true; // true = SceneTransition, false = TransitionEffects

    [Header("Cinematic Text After Dialogue")]
    public bool playCinematicAfterEnd = false;
    public CinematicText cinematicText;
    public bool fadeToBlackAfterCinematic = true;

    [Header("Hide UI During Cinematic")]
    public GameObject[] uiToHideDuringCinematic; // UI yang di-hide pas cinematic (misal: joystick)

    [Header("Music")]
    public bool changeMusicOnTrigger = false;
    public int musicIndex = 0; // Index di AudioManager.musicLibrary
    public float musicVolume = 1f;

    [Header("Tilemap Tint")]
    public bool applyTilemapTintOnEnter = false;
    public TilemapTintManager.TintType tintType = TilemapTintManager.TintType.None;

    private bool hasTriggered = false;
    private bool dialogueEndedHandled = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && CanTrigger())
        {
            TriggerDialogue();
        }
    }

    public void TriggerDialogue()
    {
        if (CanTrigger())
        {
            hasTriggered = true;
            dialogueEndedHandled = false;

            if (fadeToBlackOnEnter)
            {
                StartCoroutine(FadeToBlackSequence());
            }

            // Apply tilemap tint if set
            if (applyTilemapTintOnEnter)
            {
                TilemapTintManager tintMgr = FindFirstObjectByType<TilemapTintManager>();
                if (tintMgr != null)
                {
                    tintMgr.ApplyTint(tintType);
                }
            }

            if (deactivateAfterTrigger)
                gameObject.SetActive(false);

            ShowNPCs();

            // Play music jika di-set
            if (changeMusicOnTrigger && AudioManager.instance != null)
            {
                AudioManager.instance.PlayMusic(musicIndex, musicVolume);
            }

            if (autoWalkTarget != null)
            {
                if (waypointsOverride != null && waypointsOverride.Length >= 2)
                {
                    autoWalkTarget.SetWaypoints(waypointsOverride);
                }
                autoWalkTarget.StartWalking();
            }

            if (DialogueManagerEP1.instanceEP1 == null) return;
            if (dialogueData == null) return;

            DialogueManagerEP1.instanceEP1.OnDialogueEnd += OnDialogueEnded;
            DialogueManagerEP1.instanceEP1.OnLineChanged += OnLineChanged;

            DialogueManagerEP1.instanceEP1.StartDialogue(dialogueData, this);
        }
    }

    private void OnDialogueEnded()
    {
        if (dialogueEndedHandled) return;

        // Cek apakah ini trigger yang memulai dialogue ini
        if (DialogueManagerEP1.instanceEP1 != null)
        {
            DialogueTriggerEP1 currentTrigger = DialogueManagerEP1.instanceEP1.GetCurrentTrigger();
            if (currentTrigger != this) return;
        }

        dialogueEndedHandled = true;

        // Unsubscribe
        if (DialogueManagerEP1.instanceEP1 != null)
        {
            DialogueManagerEP1.instanceEP1.OnDialogueEnd -= OnDialogueEnded;
            DialogueManagerEP1.instanceEP1.OnLineChanged -= OnLineChanged;
        }

        // Screen shake on end
        if (enableScreenShake)
        {
            ScreenShake shaker = UnityEngine.Object.FindAnyObjectByType<ScreenShake>();
            if (shaker != null)
            {
                shaker.TriggerShake(shakeDuration, shakeIntensity);
            }
        }

        // Play SFX on end
        if (playSFXOnEnd && sfxToPlayOnEnd != null)
        {
            AudioManager.instance?.PlaySFX(sfxToPlayOnEnd);
        }

        // Fade to black on end
        if (fadeToBlackOnEnd)
        {
            if (useSceneTransition)
            {
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
                transition?.FadeInBlack(null);
            }
            else
            {
                TransitionEffects effects = null;
                TransitionEffects[] allEffects = FindObjectsByType<TransitionEffects>(FindObjectsInactive.Exclude);
                foreach (var e in allEffects)
                {
                    if (e != null)
                    {
                        effects = e;
                        break;
                    }
                }
                effects?.FadeToBlack(null);
            }

            if (fadeBackAfterBlack)
            {
                Invoke(nameof(FadeBackUsingSceneTransition), fadeBackDelay);
            }
        }

        // Handle deactivation
        if (playCinematicAfterEnd)
        {
            // Deactivation handled by cinematic callback
        }
        else if (fadeToBlackOnEnd && fadeBackAfterBlack)
        {
            SceneTransition transition = FindFirstObjectByType<SceneTransition>();
            float totalDelay = fadeBackDelay + (transition != null ? transition.fadeOutSpeed : 1f);
            Invoke(nameof(DeactivateTrigger), totalDelay);
        }
        else if (fadeToBlackOnEnd)
        {
            SceneTransition transition = FindFirstObjectByType<SceneTransition>();
            float delay = transition != null ? (1f / transition.fadeInSpeed) : 0.8f;
            Invoke(nameof(DeactivateTrigger), delay);
        }
        else if (deactivateAfterTrigger)
        {
            DeactivateTrigger();
        }

        // Activate next trigger
        if (nextTriggerToActivate != null)
        {
            if (delayBeforeActivateNext > 0)
            {
                Invoke(nameof(ActivateNextTriggerDelayed), delayBeforeActivateNext);
            }
            else
            {
                nextTriggerToActivate.SetActive(true);
            }
        }

        // Cinematic Text - flow: fade to black -> fade out -> play cinematic -> fade to black final
        if (playCinematicAfterEnd && cinematicText != null)
        {
            // Setup callback untuk fade to black final setelah cinematic selesai
            cinematicText.onComplete = () => {
                if (fadeToBlackAfterCinematic)
                {
                    if (useSceneTransition)
                    {
                        SceneTransition transition = FindFirstObjectByType<SceneTransition>();
                        transition?.FadeInBlack(null);
                    }
                    else
                    {
                        TransitionEffects effects = FindFirstObjectByType<TransitionEffects>();
                        effects?.FadeToBlack(null);
                    }
                }
                Invoke(nameof(DeactivateTrigger), 1f);
            };

            // Initial fade to black
            if (fadeToBlackOnEnd)
            {
                if (useSceneTransition)
                {
                    SceneTransition transition = FindFirstObjectByType<SceneTransition>();
                    transition?.FadeInBlack(null);
                }
                else
                {
                    TransitionEffects effects = FindFirstObjectByType<TransitionEffects>();
                    effects?.FadeToBlack(null);
                }
            }

            // Fade out cepat, lalu play cinematic
            Invoke(nameof(FadeOutThenPlayCinematic), 0.3f);
        }

        // Handle Ryu Separation
        if (startRyuAutoWalkOnDialogueEnd)
        {
            StartRyuSeparation();
        }

        // Hide NPCs
        if (hideRyuAfterAutoWalk && npcsToHide != null)
        {
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

    private void DeactivateTrigger()
    {
        if (deactivateAfterTrigger)
        {
            gameObject.SetActive(false);
        }
    }

    private void FadeBackUsingSceneTransition()
    {
        TransformObjects();

        if (useSceneTransition)
        {
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
            transition?.FadeOutBlack(null);
        }
        else
        {
            TransitionEffects effects = null;
            TransitionEffects[] allEffects = FindObjectsByType<TransitionEffects>(FindObjectsInactive.Exclude);
            foreach (var e in allEffects)
            {
                if (e != null)
                {
                    effects = e;
                    break;
                }
            }
            effects?.FadeFromBlack(null);
        }
    }

    private void ActivateNextTriggerDelayed()
    {
        nextTriggerToActivate?.SetActive(true);
    }

    private void PlayCinematicWithDelay()
    {
        HideUICinematic();
        cinematicText?.Play();
    }

    private void HideUICinematic()
    {
        if (uiToHideDuringCinematic == null) return;
        foreach (var ui in uiToHideDuringCinematic)
        {
            if (ui != null)
                ui.SetActive(false);
        }
    }

    private void ShowUICinematic()
    {
        if (uiToHideDuringCinematic == null) return;
        foreach (var ui in uiToHideDuringCinematic)
        {
            if (ui != null)
                ui.SetActive(true);
        }
    }

    // Fade out cepat, lalu play cinematic
    private void FadeOutThenPlayCinematic()
    {
        if (useSceneTransition)
        {
            SceneTransition transition = FindFirstObjectByType<SceneTransition>();
            if (transition != null)
            {
                transition.FadeOutBlack(() => {
                    PlayCinematicWithDelay();
                });
                return;
            }
        }
        else
        {
            TransitionEffects effects = FindFirstObjectByType<TransitionEffects>();
            if (effects != null)
            {
                float originalDuration = effects.fadeDuration;
                effects.fadeDuration = 0.3f; // Fast fade
                effects.FadeFromBlack(() => {
                    effects.fadeDuration = originalDuration;
                    PlayCinematicWithDelay();
                });
                return;
            }
        }

        // Fallback: play cinematic langsung
        PlayCinematicWithDelay();
    }

    private void TransformObjects()
    {
        if (objectsToHideAfterDialogue != null)
        {
            foreach (GameObject obj in objectsToHideAfterDialogue)
            {
                obj?.SetActive(false);
            }
        }

        if (objectsToShowAfterDialogue != null)
        {
            foreach (GameObject obj in objectsToShowAfterDialogue)
            {
                obj?.SetActive(true);
            }
        }
    }

    private void HideNPCsFiltered(GameObject[] npcs)
    {
        if (npcs == null) return;

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
            }
        }
    }

    private bool ryuSeparationStarted = false;
    private void StartRyuSeparation()
    {
        if (ryuSeparationStarted) return;
        ryuSeparationStarted = true;

        if (ryuFollowTarget != null)
        {
            ryuFollowTarget.StopFollowing();
        }

        if (ryuAutoWalk != null)
        {
            ryuAutoWalk.StartWalking();
            ryuAutoWalk.onAutoWalkComplete.AddListener(OnRyuAutoWalkComplete);
        }
    }

    private void OnRyuAutoWalkComplete()
    {
        if (ryuAutoWalk != null)
        {
            ryuAutoWalk.onAutoWalkComplete.RemoveListener(OnRyuAutoWalkComplete);
        }

        if (hideRyuAfterAutoWalk && ryuAutoWalk != null)
        {
            ryuAutoWalk.gameObject.SetActive(false);
        }
    }

    private void ShowNPCs()
    {
        if (npcsToShow == null) return;

        foreach (GameObject npc in npcsToShow)
        {
            if (npc != null)
            {
                npc.SetActive(true);

                if (autoStartAutoWalkOnShow)
                {
                    AutoWalk npcAutoWalk = npc.GetComponent<AutoWalk>();
                    if (npcAutoWalk != null && npcAutoWalk.waypoints != null && npcAutoWalk.waypoints.Length > 0)
                    {
                        npcAutoWalk.StartWalking();
                    }
                }
            }
        }
    }

    private void HideNPCs()
    {
        if (npcsToHide == null) return;

        foreach (GameObject npc in npcsToHide)
        {
            if (npc != null)
            {
                AutoWalk npcAutoWalk = npc.GetComponent<AutoWalk>();
                if (npcAutoWalk != null)
                {
                    npcAutoWalk.StopWalking();
                }
                npc.SetActive(false);
            }
        }
    }

    private bool CanTrigger()
    {
        if (triggerOnce && hasTriggered) return false;
        if (DialogueManagerEP1.instanceEP1 != null && DialogueManagerEP1.instanceEP1.IsDialogueActive()) return false;
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
        transition?.FadeInBlack(null);
    }

    private void OnLineChanged(int lineIndex)
    {
        // Cek apakah ini trigger yang sedang aktif
        if (DialogueManagerEP1.instanceEP1 != null)
        {
            DialogueTriggerEP1 currentTrigger = DialogueManagerEP1.instanceEP1.GetCurrentTrigger();
            if (currentTrigger != this) return;
        }

        if (dialogueData == null || dialogueData.lines == null || lineIndex >= dialogueData.lines.Length)
            return;

        DialogueDataEP1.DialogueLine line = dialogueData.lines[lineIndex];

        // Screen Shake
        if (line.triggerScreenShake || (enableScreenShake && lineIndex == shakeOnLineIndex))
        {
            ScreenShake shaker = UnityEngine.Object.FindAnyObjectByType<ScreenShake>();
            shaker?.TriggerShake(shakeDuration, shakeIntensity);
        }

        // Sound Effect
        if (line.soundEffect != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(line.soundEffect);
        }

        // Fade to Black
        if (line.triggerFadeToBlack)
        {
            StartCoroutine(FadeToBlackAfterLine());
        }

        // Hide object on specific line
        if (objectToHideOnLine != null && lineIndex == hideObjectOnLineIndex)
        {
            objectToHideOnLine.SetActive(false);
        }

        // Trigger AutoWalk on specific line
        if (npcsToAutoWalkOnLine != null && npcsToAutoWalkOnLine.Length > 0 && lineIndex == autoWalkOnLineIndex)
        {
            StartAutoWalkForNPCs();
        }
    }

    private int autoWalkCompletedCount = 0;
    private bool isWaitingForAutoWalk = false;

    private void StartAutoWalkForNPCs()
    {
        if (npcsToAutoWalkOnLine == null || npcsToAutoWalkOnLine.Length == 0)
            return;

        isWaitingForAutoWalk = true;
        autoWalkCompletedCount = 0;

        foreach (AutoWalk npc in npcsToAutoWalkOnLine)
        {
            if (npc != null)
            {
                npc.StartWalking();
                npc.onAutoWalkComplete.AddListener(OnNPCAutoWalkComplete);
            }
            else
            {
                autoWalkCompletedCount++;
            }
        }
    }

    private void OnNPCAutoWalkComplete()
    {
        autoWalkCompletedCount++;
        int totalNPCs = npcsToAutoWalkOnLine != null ? npcsToAutoWalkOnLine.Length : 0;

        if (isWaitingForAutoWalk && autoWalkCompletedCount >= totalNPCs)
        {
            isWaitingForAutoWalk = false;

            foreach (AutoWalk npc in npcsToAutoWalkOnLine)
            {
                if (npc != null)
                {
                    npc.onAutoWalkComplete.RemoveListener(OnNPCAutoWalkComplete);
                }
            }

            StartCoroutine(ResumeDialogueAfterDelay(0.5f));
        }
    }

    private System.Collections.IEnumerator ResumeDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        DialogueManagerEP1.instanceEP1?.ResumeDialogue();
    }

    private System.Collections.IEnumerator FadeToBlackAfterLine()
    {
        yield return new WaitForSeconds(1f);

        SceneTransition transition = UnityEngine.Object.FindAnyObjectByType<SceneTransition>();
        transition?.FadeInBlack(null);
    }
}

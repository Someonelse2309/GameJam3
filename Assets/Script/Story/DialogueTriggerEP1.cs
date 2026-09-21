using UnityEngine;
using System.Collections.Generic;

public class DialogueTriggerEP1 : MonoBehaviour
{
    public DialogueDataEP1 dialogueData;

    [Header("Settings")]
    public bool triggerOnce = true;
    public bool deactivateAfterTrigger = false;

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
        DialogueManagerEP1.instanceEP1.OnDialogueEnd -= OnDialogueEnded;

        // Deactivate this trigger
        if (deactivateAfterTrigger)
        {
            gameObject.SetActive(false);
            Debug.Log("Deactivated trigger: " + gameObject.name);
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
}

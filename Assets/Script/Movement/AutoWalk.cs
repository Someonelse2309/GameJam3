using UnityEngine;

public class AutoWalk : MonoBehaviour
{
    [Header("Waypoints - titik yang harus dilalui")]
    public Transform[] waypoints;

    [Header("Settings")]
    public float walkSpeed = 1.5f;
    public bool flipSprite = true;
    public bool autoStart = false; // Mulai otomatis saat awake

    [Header("Control Settings")]
    public bool disablePlayerControl = true; // Matikan player control saat auto walk
    public GameObject playerToDisable; // Player yang kontrolnya dimatikan

    private int currentWaypointIndex = 0;
    private bool isWalking = false;
    private bool isPaused = false;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    [Header("Events")]
    public DialogueDataEP1 dialogueToPlayOnStart;
    public DialogueDataEP1 dialogueToPlayOnReach;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        if (autoStart && waypoints != null && waypoints.Length > 0)
        {
            StartWalking();
        }
    }

    void Update()
    {
        // Pause saat dialogue aktif
        if (DialogueManagerEP1.instanceEP1 != null && DialogueManagerEP1.instanceEP1.IsDialogueActive())
        {
            if (isWalking)
            {
                isPaused = true;
                isWalking = false;
                StopRigidbody();
            }
            return;
        }

        // Resume kalau dialogue selesai
        if (isPaused && (DialogueManagerEP1.instanceEP1 == null || !DialogueManagerEP1.instanceEP1.IsDialogueActive()))
        {
            isPaused = false;
            ResumeWalking();
        }

        if (!isWalking) return;

        WalkToWaypoint();
    }

    void WalkToWaypoint()
    {
        if (currentWaypointIndex >= waypoints.Length || waypoints[currentWaypointIndex] == null)
        {
            StopWalking();
            OnReachDestination();
            return;
        }

        Vector3 target = waypoints[currentWaypointIndex].position;
        Vector3 direction = (target - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, target);

        if (distance < 0.05f)
        {
            // Sampai di waypoint
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Length)
            {
                StopWalking();
                OnReachDestination();
            }
            return;
        }

        // Gerak
        transform.position += direction * walkSpeed * Time.deltaTime;

        // Flip sprite
        if (flipSprite && spriteRenderer != null)
        {
            if (direction.x < 0)
                spriteRenderer.flipX = true;
            else if (direction.x > 0)
                spriteRenderer.flipX = false;
        }

        MoveRigidbody(direction);
    }

    void MoveRigidbody(Vector3 direction)
    {
        if (rb != null)
        {
            rb.MovePosition(transform.position);
        }
    }

    void StopRigidbody()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void ResumeWalking()
    {
        isWalking = true;
    }

    void OnReachDestination()
    {
        // Matikan player control kalau diperlukan
        if (disablePlayerControl && playerToDisable != null)
        {
            EnablePlayerControl();
        }

        // Play dialogue kalau ada
        if (dialogueToPlayOnReach != null && DialogueManagerEP1.instanceEP1 != null)
        {
            DialogueManagerEP1.instanceEP1.StartDialogue(dialogueToPlayOnReach);
        }
    }

    public void StartWalking()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        // Matikan player control
        if (disablePlayerControl && playerToDisable != null)
        {
            DisablePlayerControl();
        }

        // Reset ke waypoint pertama
        if (waypoints[0] != null)
        {
            transform.position = waypoints[0].position;
        }
        currentWaypointIndex = 1; // Mulai dari waypoint 1 (waypoint 0 = start position)

        isWalking = true;
        isPaused = false;

        // Play dialogue kalau ada
        if (dialogueToPlayOnStart != null && DialogueManagerEP1.instanceEP1 != null)
        {
            DialogueManagerEP1.instanceEP1.StartDialogue(dialogueToPlayOnStart);
        }
    }

    public void StopWalking()
    {
        isWalking = false;
        StopRigidbody();
    }

    public void ResetAndStart()
    {
        currentWaypointIndex = 0;
        isWalking = false;
        isPaused = false;

        if (waypoints != null && waypoints.Length > 0 && waypoints[0] != null)
        {
            transform.position = waypoints[0].position;
        }

        StartWalking();
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    public bool IsFinished()
    {
        return !isWalking && currentWaypointIndex >= waypoints.Length;
    }

    void DisablePlayerControl()
    {
        PlayerMovementEP1 pm = playerToDisable.GetComponent<PlayerMovementEP1>();
        if (pm != null) pm.enabled = false;

        // Disable juga Rigidbody player kalau perlu
    }

    void EnablePlayerControl()
    {
        PlayerMovementEP1 pm = playerToDisable.GetComponent<PlayerMovementEP1>();
        if (pm != null) pm.enabled = true;
    }
}

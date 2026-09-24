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
    private RigidbodyType2D originalBodyType;

    [Header("Events")]
    public DialogueDataEP1 dialogueToPlayOnStart;
    public DialogueDataEP1 dialogueToPlayOnReach;
    public UnityEngine.Events.UnityEvent onAutoWalkComplete;

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
        // Jalan terus meskipun dialogue aktif (karakter auto-walk saat dialogue)
        if (!isWalking || isPaused) return;

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
        Debug.Log($"AutoWalk.OnReachDestination called on {gameObject.name}");

        if (disablePlayerControl && playerToDisable != null)
        {
            EnablePlayerControl();
        }

        if (onAutoWalkComplete != null)
        {
            onAutoWalkComplete.Invoke();
        }

        if (dialogueToPlayOnReach != null && DialogueManagerEP1.instanceEP1 != null)
        {
            DialogueManagerEP1.instanceEP1.StartDialogue(dialogueToPlayOnReach);
        }
    }

    public void StartWalking()
    {
        Debug.Log($"AutoWalk.StartWalking called on {gameObject.name}, waypoints={(waypoints?.Length ?? 0)}, isWalking={isWalking}");
        if (waypoints == null || waypoints.Length == 0) return;

        // Matikan player control
        if (disablePlayerControl && playerToDisable != null)
        {
            DisablePlayerControl();
        }

        // Jangan teleport - mulai dari posisi sekarang
        currentWaypointIndex = 0;

        isWalking = true;
        isPaused = false;

        // Set Rigidbody ke Kinematic agar tidak nabrak saat AutoWalk
        if (rb != null)
        {
            originalBodyType = rb.bodyType;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (dialogueToPlayOnStart != null && DialogueManagerEP1.instanceEP1 != null)
        {
            DialogueManagerEP1.instanceEP1.StartDialogue(dialogueToPlayOnStart);
        }
    }

    public void StopWalking()
    {
        Debug.Log($"AutoWalk.StopWalking called on {gameObject.name}");
        isWalking = false;
        StopRigidbody();

        if (rb != null)
        {
            rb.bodyType = originalBodyType;
        }

        if (disablePlayerControl && playerToDisable != null)
        {
            EnablePlayerControl();
        }
    }

    public void SetWaypoints(Transform[] newWaypoints)
    {
        if (newWaypoints == null || newWaypoints.Length < 1) return;

        waypoints = newWaypoints;
        currentWaypointIndex = 0; // Mulai dari posisi sekarang
        isWalking = false;
        isPaused = false;

        Debug.Log("AutoWalk: Waypoints set to " + newWaypoints.Length + " waypoints");
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

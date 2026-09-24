using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimateEP1 : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite[] idleSprites;
    public Sprite[] walkSprites;
    public Sprite[] happySprites; // MC_Happy.png

    [Header("Settings")]
    public float frameRate = 8f;
    public bool autoDetectMovement = true;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private int frame = 0;
    private string currentState = "Idle";
    private bool isPaused = false;
    private bool isMoving = false;

    private Vector3 lastPosition;
    private float velocityThreshold = 0.01f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        lastPosition = transform.position;
    }

    private void Start()
    {
        if (idleSprites != null && idleSprites.Length > 0)
        {
            spriteRenderer.sprite = idleSprites[0];
        }
        StartAnimation();
    }

    private void Update()
    {
        // Cek apakah sedang di-follow oleh FollowTarget
        FollowTarget followTarget = GetComponent<FollowTarget>();
        bool isFollowing = followTarget != null && followTarget.IsFollowing();

        // Cek apakah AutoWalk aktif
        AutoWalk autoWalk = GetComponent<AutoWalk>();
        bool isAutoWalking = autoWalk != null && autoWalk.IsWalking();

        // Cek dialogue
        if (DialogueManagerEP1.instanceEP1 != null && DialogueManagerEP1.instanceEP1.IsDialogueActive())
        {
            // Jika AutoWalk aktif atau di-follow, pakai walk animation
            if (isAutoWalking || isFollowing)
            {
                SetMoving(true);
            }
            else
            {
                if (idleSprites != null && idleSprites.Length > 0)
                    spriteRenderer.sprite = idleSprites[0];
                SetMoving(false);
            }
            return;
        }

        // Jangan auto-detect kalau lagi di-follow (FollowTarget yang handle)
        if (isFollowing) return;

        // Auto-detect movement (untuk player manual control)
        if (autoDetectMovement && !isAutoWalking)
        {
            Vector3 velocity = transform.position - lastPosition;
            bool moving = velocity.sqrMagnitude > velocityThreshold * velocityThreshold;
            SetMoving(moving);
            lastPosition = transform.position;
        }
    }

    private void StartAnimation()
    {
        CancelInvoke(nameof(NextFrame));
        frame = 0;
        Invoke(nameof(NextFrame), 1f / frameRate);
    }

    private void NextFrame()
    {
        if (isPaused) return;

        Sprite[] currentSprites = GetCurrentSprites();
        if (currentSprites == null || currentSprites.Length == 0) return;

        frame++;

        if (frame >= currentSprites.Length)
        {
            frame = 0;
        }

        if (frame >= 0 && frame < currentSprites.Length)
        {
            spriteRenderer.sprite = currentSprites[frame];
        }

        Invoke(nameof(NextFrame), 1f / frameRate);
    }

    private Sprite[] GetCurrentSprites()
    {
        switch (currentState)
        {
            case "Walk": return walkSprites;
            case "Happy": return happySprites;
            default: return idleSprites;
        }
    }

    public void SetMoving(bool moving)
    {
        if (isMoving == moving) return;
        isMoving = moving;

        if (moving)
        {
            currentState = "Walk";
        }
        else
        {
            currentState = "Idle";
            frame = 0;
        }

        StartAnimation();
    }

    public void SetFacingDirection(float directionX)
    {
        if (directionX < 0)
            spriteRenderer.flipX = true;
        else if (directionX > 0)
            spriteRenderer.flipX = false;
    }

    public void PlayHappy()
    {
        currentState = "Happy";
        frame = 0;
        StartAnimation();
    }

    public void PlayIdle()
    {
        currentState = "Idle";
        frame = 0;
        StartAnimation();
    }

    public void PauseAnimation()
    {
        isPaused = true;
        CancelInvoke(nameof(NextFrame));
    }

    public void ResumeAnimation()
    {
        isPaused = false;
        StartAnimation();
    }
}

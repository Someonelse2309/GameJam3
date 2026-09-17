using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Components")]
    public SpriteRenderer spriteRenderer;

    [Header("Sprite Animation Frames")]
    public Sprite[] idleSprites;
    public Sprite[] walkSprites;
    public Sprite[] attackSprites;
    public Sprite[] shurikenSprites;
    public float frameRate = 0.12f; // Kecepatan animasi (detik per frame)

    private Rigidbody2D rb;
    private Vector2 movement;
    private float animTimer;
    private int currentFrame;
    private Sprite[] lastAnimation;

    // State Aksi
    private bool isAttacking = false;
    private bool isThrowingShuriken = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 1. Input Serangan (J = Attack, K = Shuriken)
        if (Input.GetKeyDown(KeyCode.J) && !isAttacking && !isThrowingShuriken)
        {
            TriggerAction(attackSprites, true, false);
        }
        else if (Input.GetKeyDown(KeyCode.K) && !isAttacking && !isThrowingShuriken)
        {
            TriggerAction(shurikenSprites, false, true);
        }

        // 2. Input Gerak (Karakter diam saat menyerang)
        if (!isAttacking && !isThrowingShuriken)
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
            movement = movement.normalized;
        }
        else
        {
            movement = Vector2.zero;
        }

        // 3. Flip Badan Kiri / Kanan
        if (movement.x < 0) spriteRenderer.flipX = true;
        else if (movement.x > 0) spriteRenderer.flipX = false;

        // 4. Jalankan Animasi lewat Code
        HandleAnimation();
    }

    void TriggerAction(Sprite[] actionSprites, bool attacking, bool throwing)
    {
        if (actionSprites == null || actionSprites.Length == 0) return;

        isAttacking = attacking;
        isThrowingShuriken = throwing;
        currentFrame = 0;
        animTimer = 0f;
        lastAnimation = actionSprites;
    }

    void HandleAnimation()
    {
        Sprite[] currentAnimation;

        // Prioritas: Attack / Shuriken > Walk > Idle
        if (isAttacking)
        {
            currentAnimation = attackSprites;
        }
        else if (isThrowingShuriken)
        {
            currentAnimation = shurikenSprites;
        }
        else
        {
            currentAnimation = (movement.sqrMagnitude > 0) ? walkSprites : idleSprites;
        }

        // Reset frame index jika berganti status
        if (currentAnimation != lastAnimation)
        {
            currentFrame = 0;
            animTimer = 0f;
            lastAnimation = currentAnimation;
        }

        // Timer untuk berpindah frame
        if (currentAnimation != null && currentAnimation.Length > 0)
        {
            animTimer += Time.deltaTime;
            if (animTimer >= frameRate)
            {
                animTimer = 0f;
                currentFrame++;

                // Jika animasi aksi selesai 1 putaran, balik ke Idle/Walk
                if (currentFrame >= currentAnimation.Length)
                {
                    if (isAttacking || isThrowingShuriken)
                    {
                        isAttacking = false;
                        isThrowingShuriken = false;
                        currentFrame = 0;
                        return;
                    }
                    currentFrame = 0; // Looping untuk Idle/Walk
                }

                spriteRenderer.sprite = currentAnimation[currentFrame];
            }
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
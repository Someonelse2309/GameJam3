using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Components")]
    public SpriteRenderer spriteRenderer;

    [Header("Sprite Animation Frames")]
    public Sprite[] idleSprites;
    public Sprite[] walkSprites;
    public float frameRate = 0.12f; // Kecepatan animasi (detik per frame)

    private Rigidbody2D rb;
    private Vector2 movement;
    private float animTimer;
    private int currentFrame;
    private Sprite[] lastAnimation;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 1. Input Gerak
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        // 2. Flip Badan Kiri / Kanan
        if (movement.x < 0) spriteRenderer.flipX = true;
        else if (movement.x > 0) spriteRenderer.flipX = false;

        // 3. Jalankan Animasi lewat Code
        HandleAnimation();
    }

    void HandleAnimation()
    {
        // Tentukan daftar sprite mana yang dipakai (Jalan atau Diam)
        Sprite[] currentAnimation = (movement.sqrMagnitude > 0) ? walkSprites : idleSprites;

        // Reset frame index jika berganti status (misal dari diam ke jalan)
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
                currentFrame = (currentFrame + 1) % currentAnimation.Length;
                spriteRenderer.sprite = currentAnimation[currentFrame];
            }
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
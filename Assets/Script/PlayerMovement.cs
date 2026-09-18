using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Components")]
    public SpriteRenderer spriteRenderer;

    [Header("UI Controls")]
    public VirtualJoystick joystick; // Langsung menggunakan class VirtualJoystick

    [Header("Sprite Animation Frames")]
    public Sprite[] idleSprites;
    public Sprite[] walkSprites;
    public Sprite[] attackSprites;
    public Sprite[] shurikenSprites;
    public float frameRate = 0.12f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private float animTimer;
    private int currentFrame;
    private Sprite[] lastAnimation;

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

        // 2. Input Gerak
        if (!isAttacking && !isThrowingShuriken)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");

            if (joystick != null)
            {
                float jX = GetJoystickAxis("Horizontal");
                float jY = GetJoystickAxis("Vertical");

                if (Mathf.Abs(jX) > 0.05f || Mathf.Abs(jY) > 0.05f)
                {
                    moveX = jX;
                    moveY = jY;
                }
            }

            movement.x = moveX;
            movement.y = moveY;

            if (movement.sqrMagnitude > 1f)
            {
                movement = movement.normalized;
            }
        }
        else
        {
            movement = Vector2.zero;
        }

        // 3. Flip Badan
        if (movement.x < 0) spriteRenderer.flipX = true;
        else if (movement.x > 0) spriteRenderer.flipX = false;

        // 4. Jalankan Animasi
        HandleAnimation();
    }

    float GetJoystickAxis(string axis)
    {
        if (joystick == null) return 0f;

        // Coba baca properti atau variabel umum pada VirtualJoystick
        var prop = joystick.GetType().GetProperty(axis) ?? joystick.GetType().GetProperty(axis.ToLower());
        if (prop != null) return (float)prop.GetValue(joystick);

        var field = joystick.GetType().GetField(axis) ?? joystick.GetType().GetField(axis.ToLower());
        if (field != null) return (float)field.GetValue(joystick);

        var vecProp = joystick.GetType().GetProperty("InputVector") ?? joystick.GetType().GetProperty("inputVector");
        if (vecProp != null)
        {
            Vector2 vec = (Vector2)vecProp.GetValue(joystick);
            return axis == "Horizontal" ? vec.x : vec.y;
        }

        var vecField = joystick.GetType().GetField("InputVector") ?? joystick.GetType().GetField("inputVector");
        if (vecField != null)
        {
            Vector2 vec = (Vector2)vecField.GetValue(joystick);
            return axis == "Horizontal" ? vec.x : vec.y;
        }

        return 0f;
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

        if (isAttacking) currentAnimation = attackSprites;
        else if (isThrowingShuriken) currentAnimation = shurikenSprites;
        else currentAnimation = (movement.sqrMagnitude > 0) ? walkSprites : idleSprites;

        if (currentAnimation != lastAnimation)
        {
            currentFrame = 0;
            animTimer = 0f;
            lastAnimation = currentAnimation;
        }

        if (currentAnimation != null && currentAnimation.Length > 0)
        {
            animTimer += Time.deltaTime;
            if (animTimer >= frameRate)
            {
                animTimer = 0f;
                currentFrame++;

                if (currentFrame >= currentAnimation.Length)
                {
                    if (isAttacking || isThrowingShuriken)
                    {
                        isAttacking = false;
                        isThrowingShuriken = false;
                        currentFrame = 0;
                        return;
                    }
                    currentFrame = 0;
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
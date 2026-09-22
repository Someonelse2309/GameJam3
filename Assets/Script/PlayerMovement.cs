using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get; private set; }

    public float moveSpeed = 5f;

    [Header("Components")]
    public SpriteRenderer spriteRenderer;

    [Header("UI Controls")]
    public VirtualJoystick joystick; 

    [Header("Combat Animations Setup")]
    public Sprite[] punchSprites;       
    public Sprite[] swordSprites;       
    public Sprite[] shurikenSprites;    
    public Sprite[] attackSprites;      
    public bool isSwordEquipped { get; private set; } = false;

    [Header("Sprite Animation Frames")]
    public Sprite[] idleSprites;
    public Sprite[] walkSprites;
    public float frameRate = 0.12f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private float animTimer;
    private int currentFrame;
    private Sprite[] lastAnimation;

    public bool IsAttacking => isAttacking;
    private bool isAttacking = false;
    private bool isThrowingShuriken = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (punchSprites != null && punchSprites.Length > 0)
        {
            attackSprites = punchSprites;
        }
    }

    public void EquipSword(bool equip)
    {
        isSwordEquipped = equip;
        attackSprites = equip ? swordSprites : punchSprites;
    }

    void Update()
    {
        // 1. Input Serangan Keyboard (J = Attack, K = Shuriken)
        if (Input.GetKeyDown(KeyCode.J) && !isAttacking && !isThrowingShuriken)
        {
            PlayerCombat combat = GetComponent<PlayerCombat>();
            if (combat != null)
            {
                combat.PerformAttack();
            }
            else
            {
                TriggerAttack();
            }
        }
        else if (Input.GetKeyDown(KeyCode.K) && !isAttacking && !isThrowingShuriken)
        {
            TriggerAction(shurikenSprites, false, true);
        }

        // 2. Input Gerak (Tetap membaca analog/keyboard meski sedang menyerang)
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

        // 3. Flip Badan Kiri / Kanan sesuai arah jalan
        if (movement.x < 0) spriteRenderer.flipX = true;
        else if (movement.x > 0) spriteRenderer.flipX = false;

        // 4. Jalankan Animasi
        HandleAnimation();
    }

    float GetJoystickAxis(string axis)
    {
        if (joystick == null) return 0f;

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

    public void TriggerAttack()
    {
        if (!isAttacking && !isThrowingShuriken)
        {
            TriggerAction(attackSprites, true, false);
        }
    }

    public void TriggerAction(Sprite[] actionSprites, bool attacking, bool throwing)
    {
        if (actionSprites == null || actionSprites.Length == 0) return;

        isAttacking = attacking;
        isThrowingShuriken = throwing;
        currentFrame = 0;
        animTimer = 0f;
        lastAnimation = actionSprites;
    }

    public void TriggerActionFromExternal(Sprite[] sprites, bool attacking, bool throwing)
    {
        TriggerAction(sprites, attacking, throwing);
    }

    void HandleAnimation()
    {
        Sprite[] currentAnimation;

        // Prioritas visual: Animasi serang dimainkan di atas pergerakan
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
        // Karakter tetap meluncur sesuai arah analog saat memukul
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get; private set; }

    public float moveSpeed = 5f;

    [Header("Components")]
    public SpriteRenderer spriteRenderer;

    [Header("UI Controls")]
    public VirtualJoystick joystick; // Slot drag JoystickBase

    [Header("Combat Animations Setup")]
    public Sprite[] punchSprites;       // MOM_Punch1, MOM_Punch2
    public Sprite[] swordSprites;       // MOM_Attack1, MOM_Attack2
    public Sprite[] shurikenSprites;    // Animasi shuriken
    public Sprite[] attackSprites;      // Sprites serangan aktif (otomatis berganti)
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

        // Default awal menggunakan tinju (punch)
        if (punchSprites != null && punchSprites.Length > 0)
        {
            attackSprites = punchSprites;
        }
    }

    // Fungsi ganti senjata (dipanggil saat Katana di-klik di Inventory)
    public void EquipSword(bool equip)
    {
        isSwordEquipped = equip;
        attackSprites = equip ? swordSprites : punchSprites;
        Debug.Log(equip ? "Katana Berhasil Dipasang!" : "Kembali ke mode Tinju");
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

        // 2. Input Gerak (Keyboard WASD & Virtual Joystick Aman)
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

        // 3. Flip Badan Kiri / Kanan
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

    // Tambahkan fungsi ini di dalam PlayerMovement:
    public void TriggerAttack()
    {
        if (!isAttacking && !isThrowingShuriken)
        {
            TriggerAction(attackSprites, true, false);
        }
    }
}
using System;
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
    public bool isFrozen { get; private set; } = false;

    // Cache fungsi joystick agar tidak lag di HP
    private Func<float> getJoyX;
    private Func<float> getJoyY;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Kunci 60 FPS langsung di dalam scene
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (punchSprites != null && punchSprites.Length > 0)
        {
            attackSprites = punchSprites;
        }

        SetupJoystickReader();
    }

    private void SetupJoystickReader()
    {
        if (joystick == null) return;
        var t = joystick.GetType();

        var vecProp = t.GetProperty("InputVector") ?? t.GetProperty("inputVector");
        if (vecProp != null)
        {
            getJoyX = () => ((Vector2)vecProp.GetValue(joystick)).x;
            getJoyY = () => ((Vector2)vecProp.GetValue(joystick)).y;
            return;
        }

        var vecField = t.GetField("InputVector") ?? t.GetField("inputVector");
        if (vecField != null)
        {
            getJoyX = () => ((Vector2)vecField.GetValue(joystick)).x;
            getJoyY = () => ((Vector2)vecField.GetValue(joystick)).y;
            return;
        }

        var hProp = t.GetProperty("Horizontal") ?? t.GetProperty("horizontal");
        var vProp = t.GetProperty("Vertical") ?? t.GetProperty("vertical");
        if (hProp != null && vProp != null)
        {
            getJoyX = () => (float)hProp.GetValue(joystick);
            getJoyY = () => (float)vProp.GetValue(joystick);
            return;
        }

        var hField = t.GetField("Horizontal") ?? t.GetField("horizontal");
        var vField = t.GetField("Vertical") ?? t.GetField("vertical");
        if (hField != null && vField != null)
        {
            getJoyX = () => (float)hField.GetValue(joystick);
            getJoyY = () => (float)vField.GetValue(joystick);
        }
    }

    public void SetFreeze(bool freeze)
    {
        isFrozen = freeze;
        if (isFrozen)
        {
            movement = Vector2.zero;
            isAttacking = false;
            isThrowingShuriken = false;
        }
    }

    public void EquipSword(bool equip)
    {
        isSwordEquipped = equip;
        attackSprites = equip ? swordSprites : punchSprites;
    }

    void Update()
    {
        if (isFrozen)
        {
            movement = Vector2.zero;
            HandleAnimation();
            if (GameAudioManager.Instance != null) GameAudioManager.Instance.ProcessFootstep(false);
            return;
        }

        // 1. Input Serang
        if (Input.GetKeyDown(KeyCode.J) && !isAttacking && !isThrowingShuriken)
        {
            PlayerCombat combat = GetComponent<PlayerCombat>();
            if (combat != null) combat.PerformAttack();
            else TriggerAttack();
        }
        else if (Input.GetKeyDown(KeyCode.K) && !isAttacking && !isThrowingShuriken)
        {
            TriggerAction(shurikenSprites, false, true);
        }

        // 2. Input Gerak
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        if (joystick != null)
        {
            float jX = getJoyX != null ? getJoyX() : 0f;
            float jY = getJoyY != null ? getJoyY() : 0f;

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

        // 3. Audio Footstep (Hanya aktif saat bergerak dan tidak menyerang)
        bool isMoving = movement.sqrMagnitude > 0.05f && !isAttacking;
        if (GameAudioManager.Instance != null)
        {
            GameAudioManager.Instance.ProcessFootstep(isMoving);
        }

        // 4. Flip Visual
        if (movement.x < 0) spriteRenderer.flipX = true;
        else if (movement.x > 0) spriteRenderer.flipX = false;

        // 5. Animasi
        HandleAnimation();
    }

    public void TriggerAttack()
    {
        if (!isAttacking && !isThrowingShuriken && !isFrozen)
        {
            TriggerAction(attackSprites, true, false);
        }
    }

    public void TriggerAction(Sprite[] actionSprites, bool attacking, bool throwing)
    {
        if (isFrozen || actionSprites == null || actionSprites.Length == 0) return;

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
        if (isFrozen) return;

        // Posisi bergerak mulus tanpa penumpukan audio di thread fisika
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
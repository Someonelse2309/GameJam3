using UnityEngine;

public class PlayerMovementEP1 : MonoBehaviour
{
    public float moveSpeed = 3f; // Lebih lambat dari Ep2 (combat)
    private Rigidbody2D rb;
    private PlayerAnimateEP1 playerAnimate;
    private SpriteRenderer spriteRenderer;
    private Vector2 movement;

    public VirtualJoystick joystick;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAnimate = GetComponent<PlayerAnimateEP1>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        movement = Vector2.zero;
    }

    void Update()
    {
        // Tidak bisa gerak saat dialogue aktif
        if (DialogueManager.instance != null && DialogueManager.instance.IsDialogueActive())
        {
            movement = Vector2.zero;
            return;
        }

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // Jika analog disentuh, utamakan input analog
        if (joystick != null && joystick.InputVector != Vector2.zero)
        {
            moveX = joystick.InputVector.x;
            moveY = joystick.InputVector.y;
        }

        movement = new Vector2(moveX, moveY).normalized;

        // Flip sprite
        if (spriteRenderer != null)
        {
            if (movement.x < 0)
                spriteRenderer.flipX = true;
            else if (movement.x > 0)
                spriteRenderer.flipX = false;
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }
}

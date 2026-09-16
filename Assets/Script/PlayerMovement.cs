using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Vector2 movement;

    public VirtualJoystick joystick; // Drag JoystickBase ke slot ini via Inspector

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // Jika analog disentuh, utamakan input analog
        if (joystick != null && joystick.InputVector != Vector2.zero)
        {
            moveX = joystick.InputVector.x;
            moveY = joystick.InputVector.y;
        }

        movement = new Vector2(moveX, moveY).normalized;

        bool isMoving = movement.sqrMagnitude > 0;
        anim.SetBool("isMoving", isMoving); // Diperbaiki dari animator ke anim

        if (movement.x < 0) spriteRenderer.flipX = true;
        else if (movement.x > 0) spriteRenderer.flipX = false;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }
}
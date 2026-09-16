using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer; // 1. Deklarasi SpriteRenderer
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // 2. Ambil komponen SpriteRenderer
    }

    void Update()
    {
        // Input Pergerakan (A/D = Kiri/Kanan, W/S = Atas/Bawah)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Set animasi gerak
        bool isMoving = movement.sqrMagnitude > 0;
        anim.SetBool("isMoving", isMoving);

        // 3. Logika Flip Sprite
        if (movement.x < 0)
        {
            spriteRenderer.flipX = true;  // Nge-flip ke kiri
        }
        else if (movement.x > 0)
        {
            spriteRenderer.flipX = false; // Kembali hadap kanan
        }

        // Input Lompat (Tombol Space)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            anim.SetTrigger("Jump");
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }
}
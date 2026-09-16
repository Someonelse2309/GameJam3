using UnityEngine;

public class KnightAmbush : MonoBehaviour
{
    [Header("References")]
    public Transform knight;        // Objek NPC Knight (Run_0)
    public Transform beggar;        // Objek Pengemis Merah (Ble_0)
    public GameObject kickButton;   // Slot untuk KickButton UI
    public float moveSpeed = 3f;
    public float attackRange = 0.8f;

    private bool isTriggered = false;
    private bool isAttacking = false;
    private Animator knightAnim;

    private void Awake()
    {
        if (knight != null)
            knightAnim = knight.GetComponent<Animator>();
    }

    private void Start()
    {
        // Sembunyikan tombol Kick di awal game
        if (kickButton != null)
            kickButton.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
        }
    }

    private void Update()
    {
        if (!isTriggered || isAttacking || knight == null || beggar == null) return;

        knight.position = Vector2.MoveTowards(knight.position, beggar.position, moveSpeed * Time.deltaTime);

        if (knight.position.x > beggar.position.x)
            knight.localScale = new Vector3(-Mathf.Abs(knight.localScale.x), knight.localScale.y, knight.localScale.z);
        else
            knight.localScale = new Vector3(Mathf.Abs(knight.localScale.x), knight.localScale.y, knight.localScale.z);

        // Saat Knight sudah dekat dengan pengemis
        if (Vector2.Distance(knight.position, beggar.position) <= attackRange)
        {
            isAttacking = true;
            if (knightAnim != null)
            {
                knightAnim.SetTrigger("Attack");
            }

            // Tombol Kick dimunculkan di sini!
            if (kickButton != null)
                kickButton.SetActive(true);
        }
    }
}
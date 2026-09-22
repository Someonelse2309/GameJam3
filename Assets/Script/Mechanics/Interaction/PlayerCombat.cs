using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public int attackDamage = 25;
    public float attackRange = 0.85f;
    public Vector2 attackOffset = new Vector2(0.5f, 0f);

    [Header("Cooldown")]
    public float attackCooldown = 0.35f;
    private float nextAttackTime = 0f;

    private SpriteRenderer spriteRenderer;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        playerMovement = GetComponent<PlayerMovement>();
    }

    public void PerformAttack()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;

        // 1. Jalankan animasi serang pada MC
        if (playerMovement != null)
        {
            playerMovement.TriggerAttack();
        }

        // 2. Hitung titik pukulan di depan MC
        bool facingRight = (spriteRenderer != null) ? !spriteRenderer.flipX : true;
        Vector2 punchOrigin = (Vector2)transform.position + new Vector2(
            facingRight ? attackOffset.x : -attackOffset.x,
            attackOffset.y
        );

        // 3. Deteksi musuh tanpa ketergantungan Layer (otomatis via Tag & Komponen)
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(punchOrigin, attackRange);

        foreach (Collider2D col in hitColliders)
        {
            if (col.gameObject == gameObject) continue;

            if (col.CompareTag("Enemy") || col.GetComponent<YakuzaEnemy>() != null)
            {
                CharacterHealth enemyHealth = col.GetComponent<CharacterHealth>();
                if (enemyHealth != null && !enemyHealth.isDead)
                {
                    enemyHealth.TakeDamage(attackDamage);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        bool facingRight = (spriteRenderer != null) ? !spriteRenderer.flipX : true;
        Vector2 punchOrigin = (Vector2)transform.position + new Vector2(
            facingRight ? attackOffset.x : -attackOffset.x,
            attackOffset.y
        );

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(punchOrigin, attackRange);
    }
}
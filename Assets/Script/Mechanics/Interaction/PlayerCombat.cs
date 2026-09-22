using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public int attackDamage = 25;
    public float attackRange = 0.85f;
    public Vector2 attackOffset = new Vector2(0.5f, 0f);

    [Header("Cooldown & Delay")]
    [Tooltip("Waktu jeda antar pukulan (detik)")]
    public float attackCooldown = 0.8f;      // Nilai ideal jeda serangan
    public float damageDelay = 0.12f;        // Delay agar damage masuk pas animasi memukul

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
        // 1. Cek timer cooldown
        if (Time.time < nextAttackTime) return;

        // 2. Kunci: Jangan serang jika animasi pukulan sebelumnya belum selesai
        if (playerMovement != null && playerMovement.IsAttacking) return;

        // Pasang waktu jeda berikutnya
        nextAttackTime = Time.time + attackCooldown;

        // Jalankan animasi serang
        if (playerMovement != null)
        {
            playerMovement.TriggerAttack();
        }

        // Berikan damage dengan timing yang pas
        StartCoroutine(DealDamageRoutine());
    }

    private IEnumerator DealDamageRoutine()
    {
        yield return new WaitForSeconds(damageDelay);

        bool facingRight = (spriteRenderer != null) ? !spriteRenderer.flipX : true;
        Vector2 punchOrigin = (Vector2)transform.position + new Vector2(
            facingRight ? attackOffset.x : -attackOffset.x,
            attackOffset.y
        );

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
using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Punch Stats (Mode Tinju)")]
    public int punchDamage = 20;
    public float punchRange = 0.85f;
    public Vector2 punchOffset = new Vector2(0.5f, 0f);

    [Header("Sword Stats (Mode Katana)")]
    public int swordDamage = 50;
    public float swordRange = 1.4f;
    public Vector2 swordOffset = new Vector2(0.75f, 0f);

    [Header("Cooldown & Delay")]
    public float attackCooldown = 0.8f;
    public float damageDelay = 0.12f;

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
        if (playerMovement != null && playerMovement.IsAttacking) return;

        nextAttackTime = Time.time + attackCooldown;

        if (playerMovement != null)
        {
            playerMovement.TriggerAttack();
        }

        StartCoroutine(DealDamageRoutine());
    }

    private IEnumerator DealDamageRoutine()
    {
        yield return new WaitForSeconds(damageDelay);

        // Pilih stat berdasarkan status equip pedang
        bool isSword = (playerMovement != null && playerMovement.isSwordEquipped);
        int damage = isSword ? swordDamage : punchDamage;
        float range = isSword ? swordRange : punchRange;
        Vector2 offset = isSword ? swordOffset : punchOffset;

        bool facingRight = (spriteRenderer != null) ? !spriteRenderer.flipX : true;
        Vector2 attackOrigin = (Vector2)transform.position + new Vector2(
            facingRight ? offset.x : -offset.x,
            offset.y
        );

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackOrigin, range);

        foreach (Collider2D col in hitColliders)
        {
            if (col.gameObject == gameObject) continue;

            if (col.CompareTag("Enemy") || col.GetComponent<YakuzaEnemy>() != null)
            {
                CharacterHealth enemyHealth = col.GetComponent<CharacterHealth>();
                if (enemyHealth != null && !enemyHealth.isDead)
                {
                    enemyHealth.TakeDamage(damage);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        bool isSword = (playerMovement != null && playerMovement.isSwordEquipped);
        float range = isSword ? swordRange : punchRange;
        Vector2 offset = isSword ? swordOffset : punchOffset;

        bool facingRight = (spriteRenderer != null) ? !spriteRenderer.flipX : true;
        Vector2 attackOrigin = (Vector2)transform.position + new Vector2(
            facingRight ? offset.x : -offset.x,
            offset.y
        );

        Gizmos.color = isSword ? Color.cyan : Color.red;
        Gizmos.DrawWireSphere(attackOrigin, range);
    }
}
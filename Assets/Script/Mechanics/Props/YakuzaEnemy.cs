using System.Collections;
using UnityEngine;

public class YakuzaEnemy : MonoBehaviour
{
    [Header("Combat Settings")]
    public int attackDamage = 15;
    public float attackSpeed = 1.5f;     // Jeda antar serangan
    public float moveSpeed = 1.6f;       // Kecepatan jalan
    public float attackDistance = 0.45f; // Jarak serang rapat
    public float hitRadius = 0.35f;      // Radius jangkauan tinju

    [Header("Layer Detection")]
    public LayerMask playerLayer;        // Pastikan di Inspector diubah dari Nothing ke Default / Player

    [Header("Sprites Animation")]
    public SpriteRenderer spriteRenderer;
    public Sprite[] idleSprites;
    public Sprite[] walkSprites;
    public Sprite[] attackSprites;
    public float frameRate = 0.15f;

    private Transform playerTarget;
    private CharacterHealth health;
    private bool isFighting = false;
    private bool isAttacking = false;
    private float nextAttackTime = 0f;

    private float animTimer;
    private int currentFrame;

    private void Awake()
    {
        health = GetComponent<CharacterHealth>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (health != null) health.OnDeath += OnDefeated;
    }

    public void StartCombat(Transform target)
    {
        playerTarget = target;
        isFighting = true;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!isFighting || (health != null && health.isDead) || playerTarget == null) return;

        float distance = Vector2.Distance(transform.position, playerTarget.position);

        // Cek posisi MC: true jika MC ada di sebelah kanan Yakuza
        bool isTargetOnRight = playerTarget.position.x > transform.position.x;

        // Balik flipX: jika target di kiri maka flipX = true, jika di kanan flipX = false
        spriteRenderer.flipX = !isTargetOnRight;

        if (isAttacking) return;

        if (distance > attackDistance)
        {
            // 1. Bergerak mendekati MC
            transform.position = Vector2.MoveTowards(transform.position, playerTarget.position, moveSpeed * Time.deltaTime);
            
            // 2. Jalankan animasi berjalan (Walk)
            AnimateSprites(walkSprites);
        }
        else
        {
            // 3. Pukul jika cooldown selesai
            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(PerformPunch(isTargetOnRight));
                nextAttackTime = Time.time + attackSpeed;
            }
            else
            {
                AnimateSprites(idleSprites);
            }
        }
    }

    private void AnimateSprites(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0) return;

        animTimer += Time.deltaTime;
        if (animTimer >= frameRate)
        {
            animTimer = 0f;
            currentFrame = (currentFrame + 1) % sprites.Length;
            spriteRenderer.sprite = sprites[currentFrame];
        }
    }

    private IEnumerator PerformPunch(bool isTargetOnRight)
    {
        isAttacking = true;

        if (attackSprites != null && attackSprites.Length > 0)
        {
            for (int i = 0; i < attackSprites.Length; i++)
            {
                spriteRenderer.sprite = attackSprites[i];
                yield return new WaitForSeconds(0.12f);
            }
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }

        // Arahkan jangkauan tinju ke arah MC berada
        Vector2 punchOffset = isTargetOnRight ? new Vector2(0.25f, 0f) : new Vector2(-0.25f, 0f);
        Vector2 punchPosition = (Vector2)transform.position + punchOffset;

        // Deteksi pukulan ke MC
        Collider2D hit = Physics2D.OverlapCircle(punchPosition, hitRadius, playerLayer);
        if (hit == null)
        {
            Collider2D[] allHits = Physics2D.OverlapCircleAll(punchPosition, hitRadius);
            foreach (var h in allHits)
            {
                if (h.CompareTag("Player")) { hit = h; break; }
            }
        }

        if (hit != null)
        {
            CharacterHealth playerHealth = hit.GetComponent<CharacterHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }

        isAttacking = false;
    }

    private void OnDefeated()
    {
        isFighting = false;
        StopAllCoroutines();
        StartCoroutine(FadeOutAndDie());
    }

    private IEnumerator FadeOutAndDie()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Color c = spriteRenderer.color;
        while (c.a > 0f)
        {
            c.a -= Time.deltaTime * 2.5f;
            spriteRenderer.color = c;
            yield return null;
        }

        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        float direction = (spriteRenderer != null && spriteRenderer.flipX) ? -0.25f : 0.25f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector2)transform.position + new Vector2(direction, 0f), hitRadius);
    }
}
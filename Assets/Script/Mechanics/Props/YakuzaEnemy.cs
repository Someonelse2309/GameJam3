using System.Collections;
using UnityEngine;

public class YakuzaEnemy : MonoBehaviour
{
    [Header("Combat Settings")]
    public int attackDamage = 15;
    public float attackSpeed = 1.5f;
    public float moveSpeed = 1.6f;
    public float attackDistance = 0.45f;
    public float hitRadius = 0.35f;

    [Header("Layer Detection")]
    public LayerMask playerLayer;        // Pilih "Default" atau layer MC Anda

    [Header("Shakedown / Pre-Combat Settings")]
    public Transform beggarTarget;       // Drag NPC_Beggar ke sini (agar Yakuza menghadap Beggar)
    public Vector2 shakedownPosition = new Vector2(0.397f, 1.277f); // Koordinat pas di samping Beggar

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

        if (shakedownPosition == Vector2.zero)
        {
            shakedownPosition = transform.position;
        }
    }

    // Dipanggil saat Yakitori didapatkan dari pedagang
    public void PrepareShakedown()
    {
        isFighting = false;
        isAttacking = false;
        transform.position = shakedownPosition;

        // Menghadap ke arah Beggar
        if (beggarTarget != null)
        {
            // Sprite Yakuza aslinya menghadap kanan. Jika Beggar di kiri, balik sprite (flipX = true)
            spriteRenderer.flipX = (beggarTarget.position.x < transform.position.x);
        }
        else
        {
            spriteRenderer.flipX = false; // Default hadap kanan (ke Beggar)
        }

        gameObject.SetActive(true);
    }

    // Dipanggil setelah Dialogue PT2 selesai untuk mulai baku hantam
    public void StartCombat(Transform target)
    {
        playerTarget = target;
        isFighting = true;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (health != null && health.isDead) return;

        // 1. Kondisi saat sedang memalak Beggar (Belum masuk mode tempur)
        if (!isFighting)
        {
            AnimateSprites(idleSprites);
            return;
        }

        if (playerTarget == null || isAttacking) return;

        float distance = Vector2.Distance(transform.position, playerTarget.position);

        // 2. Arah hadap saat bertarung:
        // Jika MC di sebelah kiri Yakuza -> flipX = true (hadap kiri)
        // Jika MC di sebelah kanan Yakuza -> flipX = false (hadap kanan)
        bool isPlayerOnLeft = playerTarget.position.x < transform.position.x;
        spriteRenderer.flipX = isPlayerOnLeft;

        if (distance > attackDistance)
        {
            // Kejar MC
            transform.position = Vector2.MoveTowards(transform.position, playerTarget.position, moveSpeed * Time.deltaTime);
            AnimateSprites(walkSprites);
        }
        else
        {
            // Pukul MC jika cooldown selesai
            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(PerformPunch(isPlayerOnLeft));
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

    private IEnumerator PerformPunch(bool isPlayerOnLeft)
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

        // Pukulan mengarah ke posisi MC
        Vector2 punchOffset = isPlayerOnLeft ? new Vector2(-0.25f, 0f) : new Vector2(0.25f, 0f);
        Vector2 punchPosition = (Vector2)transform.position + punchOffset;

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
        float dir = (spriteRenderer != null && spriteRenderer.flipX) ? -0.25f : 0.25f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector2)transform.position + new Vector2(dir, 0f), hitRadius);
    }
}
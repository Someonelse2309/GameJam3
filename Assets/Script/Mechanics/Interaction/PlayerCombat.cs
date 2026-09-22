using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public int attackDamage = 25; // 4 kali hit mati jika HP Yakuza 100
    public float attackRange = 0.8f;
    public Transform attackPoint;
    public LayerMask enemyLayer;

    private PlayerMovement movementScript;

    private void Awake()
    {
        movementScript = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            PerformAttack();
        }
    }

    public void PerformAttack()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive) return;

        // Picu animasi serangan MC (Punch atau Sword tergantung item yang di-equip)
        if (movementScript != null)
        {
            movementScript.TriggerAction(movementScript.attackSprites, true, false);
        }

        Vector3 point = attackPoint != null ? attackPoint.position : transform.position;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(point, attackRange);

        foreach (Collider2D col in hitColliders)
        {
            // Deteksi jika collider adalah musuh (berdasarkan Tag "Enemy" atau LayerMask)
            if (col.CompareTag("Enemy") || (enemyLayer.value != 0 && ((1 << col.gameObject.layer) & enemyLayer) != 0))
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
        Vector3 point = attackPoint != null ? attackPoint.position : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(point, attackRange);
    }
}
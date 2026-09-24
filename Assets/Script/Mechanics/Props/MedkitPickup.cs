using UnityEngine;

public class MedkitPickup : MonoBehaviour
{
    [Header("Pengaturan Heal")]
    public int healAmount = 50;
    public bool pickupOnlyIfDamaged = true;

    [Header("Efek Melayang")]
    public float floatSpeed = 4f;
    public float floatHeight = 0.08f;

    [Header("Efek Suara")]
    public AudioClip collectSound;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CharacterHealth playerHealth = collision.GetComponent<CharacterHealth>();
            if (playerHealth != null)
            {
                if (pickupOnlyIfDamaged && playerHealth.currentHealth >= playerHealth.maxHealth)
                {
                    return;
                }

                playerHealth.Heal(healAmount);

                // Mainkan suara via GameAudioManager agar volumenya ikut slider SFX
                if (collectSound != null)
                {
                    if (GameAudioManager.Instance != null)
                    {
                        GameAudioManager.Instance.PlaySFX(collectSound);
                    }
                    else
                    {
                        AudioSource.PlayClipAtPoint(collectSound, transform.position);
                    }
                }

                Destroy(gameObject);
            }
        }
    }
}
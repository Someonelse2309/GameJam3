using UnityEngine;

public class MedkitPickup : MonoBehaviour
{
    [Header("Pengaturan Heal")]
    public int healAmount = 50;
    public bool pickupOnlyIfDamaged = true; // Tidak terambil jika HP MC masih penuh

    [Header("Efek Melayang")]
    public float floatSpeed = 4f;
    public float floatHeight = 0.08f;
    private Vector3 startPos;

    [Header("Efek Suara")]
    public AudioClip collectSound;

    private bool isCollected = false;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        if (isCollected) return;

        // Gerakan melayang vertikal halus
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            CharacterHealth playerHealth = other.GetComponent<CharacterHealth>();
            if (playerHealth != null)
            {
                // Lewati jika HP player sudah maksimal
                if (pickupOnlyIfDamaged && playerHealth.currentHealth >= playerHealth.maxHealth)
                {
                    return;
                }

                isCollected = true;
                playerHealth.Heal(healAmount);

                // Bunyikan audio saat ditabrak
                if (collectSound != null)
                {
                    AudioSource.PlayClipAtPoint(collectSound, transform.position);
                }

                Destroy(gameObject);
            }
        }
    }
}
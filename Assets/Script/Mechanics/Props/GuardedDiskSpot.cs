using System.Collections;
using UnityEngine;

public class GuardedDiskSpot : MonoBehaviour
{
    [Header("Disk Object & Pickup")]
    public GameObject diskVisual;
    public Collider2D diskPickupCollider;

    [Header("Guards (1 - 3 Yakuza, Kosongkan jika 0)")]
    public YakuzaEnemy[] guards;

    [Header("Efek Melayang (Floating)")]
    public float floatSpeed = 4f;
    public float floatHeight = 0.08f;

    [Header("Efek Suara")]
    public AudioClip collectSound;

    private int guardsDefeated = 0;
    private int totalValidGuards = 0;
    private bool arenaTriggered = false;
    private bool isUnlocked = false;
    private bool hasPickedUp = false;
    private Vector3 initialVisualLocalPos;

    private void Start()
    {
        if (diskVisual != null)
        {
            initialVisualLocalPos = diskVisual.transform.localPosition;

            // Pasang trigger helper ke child DiskItem agar hanya CircleCollider2D yang bisa mengambil disk
            DiskPickupTrigger receiver = diskVisual.GetComponent<DiskPickupTrigger>();
            if (receiver == null) receiver = diskVisual.AddComponent<DiskPickupTrigger>();
            receiver.Init(this);
        }

        totalValidGuards = 0;
        if (guards != null)
        {
            foreach (var guard in guards)
            {
                if (guard != null) totalValidGuards++;
            }
        }

        // Jika 0 Guard: disk langsung aktif dan bisa disentuh
        if (totalValidGuards == 0)
        {
            UnlockDisk();
        }
        else
        {
            // Matikan collider pickup sampai kroco tumbang
            if (diskPickupCollider != null)
                diskPickupCollider.enabled = false;

            foreach (var guard in guards)
            {
                if (guard != null)
                {
                    guard.gameObject.SetActive(false);
                    guard.enabled = false;
                    Collider2D col = guard.GetComponent<Collider2D>();
                    if (col != null) col.enabled = false;
                }
            }
        }
    }

    private void Update()
    {
        if (diskVisual != null && diskVisual.activeSelf)
        {
            float newY = initialVisualLocalPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            diskVisual.transform.localPosition = new Vector3(initialVisualLocalPos.x, newY, initialVisualLocalPos.z);
        }
    }

    // BoxCollider2D di parent HANYA untuk memicu spawn Yakuza
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") || hasPickedUp) return;

        if (!arenaTriggered && totalValidGuards > 0)
        {
            arenaTriggered = true;
            SpawnGuards(collision.transform);
        }
    }

    // Dipanggil HANYA ketika Player menabrak CircleCollider2D milik DiskItem
    public void OnDiskTouched()
    {
        if (isUnlocked && !hasPickedUp)
        {
            PickupDisk();
        }
    }

    private void SpawnGuards(Transform playerTransform)
    {
        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayCombatBGM(0.4f);

        foreach (var guard in guards)
        {
            if (guard != null)
            {
                guard.gameObject.SetActive(true);
                guard.enabled = true;

                Collider2D col = guard.GetComponent<Collider2D>();
                if (col != null) col.enabled = true;

                CharacterHealth hp = guard.GetComponent<CharacterHealth>();
                if (hp != null) hp.OnDeath += OnGuardKilled;

                guard.StartCombat(playerTransform);
            }
        }
    }

    private void OnGuardKilled()
    {
        guardsDefeated++;

        if (guardsDefeated >= totalValidGuards)
        {
            UnlockDisk();
        }
    }

    private void UnlockDisk()
    {
        isUnlocked = true;

        if (diskPickupCollider != null)
            diskPickupCollider.enabled = true;

        if (totalValidGuards > 0 && GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayExplorationBGM(1f);
    }

    private void PickupDisk()
    {
        if (hasPickedUp) return;
        hasPickedUp = true;

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

        if (MemoryDiskManager.Instance != null)
        {
            MemoryDiskManager.Instance.AddDisk();
        }

        if (diskVisual != null) 
            diskVisual.SetActive(false);

        Destroy(gameObject);
    }
}

// Receiver khusus untuk child DiskItem
public class DiskPickupTrigger : MonoBehaviour
{
    private GuardedDiskSpot parentSpot;

    public void Init(GuardedDiskSpot spot)
    {
        parentSpot = spot;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && parentSpot != null)
        {
            parentSpot.OnDiskTouched();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && parentSpot != null)
        {
            parentSpot.OnDiskTouched();
        }
    }
}
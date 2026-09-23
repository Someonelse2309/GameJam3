using UnityEngine;

public class PlayerAudioBridge : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerMovement playerMovement;
    private bool wasAttacking = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (GameAudioManager.Instance == null) return;

        // 1. Suara Langkah Kaki saat Berjalan
        bool isMoving = rb != null && rb.linearVelocity.sqrMagnitude > 0.05f;
        GameAudioManager.Instance.ProcessFootstep(isMoving);

        // 2. Deteksi Ayunan Pukulan / Pedang
        if (playerMovement != null)
        {
            if (playerMovement.IsAttacking && !wasAttacking)
            {
                if (playerMovement.isSwordEquipped)
                    GameAudioManager.Instance.PlaySwordSFX();
                else
                    GameAudioManager.Instance.PlayPunchSFX();
            }
            wasAttacking = playerMovement.IsAttacking;
        }
    }
}
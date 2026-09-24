using UnityEngine;

public class PlayerAudioBridge : MonoBehaviour
{
    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    // Dipanggil oleh Animation Event saat animasi menyerang/memukul berjalan
    public void PlayAttackSound()
    {
        PlayAttackOrPunchSound();
    }

    public void PlayPunchSound()
    {
        PlayAttackOrPunchSound();
    }

    public void PlaySwordSound()
    {
        if (GameAudioManager.Instance != null)
        {
            GameAudioManager.Instance.PlaySwordSFX();
        }
    }

    private void PlayAttackOrPunchSound()
    {
        bool isSword = false;

        if (PlayerMovement.Instance != null)
        {
            isSword = PlayerMovement.Instance.isSwordEquipped;
        }
        else if (playerMovement != null)
        {
            isSword = playerMovement.isSwordEquipped;
        }

        if (GameAudioManager.Instance != null)
        {
            if (isSword)
            {
                GameAudioManager.Instance.PlaySwordSFX();
            }
            else
            {
                GameAudioManager.Instance.PlayPunchSFX();
            }
        }
    }

    // Dipanggil oleh Animation Event saat langkah kaki
    public void PlayFootstep()
    {
        if (GameAudioManager.Instance != null)
        {
            GameAudioManager.Instance.PlayFootstep();
        }
    }
}
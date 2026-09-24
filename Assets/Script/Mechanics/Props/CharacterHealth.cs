using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CharacterHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public bool isDead = false;

    [Header("UI Bar")]
    public Image healthBarFill;

    [Header("Hit Flash (White Blip)")]
    public Material flashWhiteMaterial; // Drag Mat_WhiteFlash ke sini
    public float flashDuration = 0.06f; // Kecepatan kedip (0.06 detik)
    public int flashBlinks = 2;         // Berkedip 2 kali (blip-blip)

    public event Action OnDeath;

    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
        }

        // Otomatis cari shader jika material belum sempat di-drag di Inspector
        if (flashWhiteMaterial == null)
        {
            Shader flashShader = Shader.Find("Custom/SpriteWhiteFlash");
            if (flashShader != null)
            {
                flashWhiteMaterial = new Material(flashShader);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthBar();
        TriggerFlash();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    public void TriggerFlash()
    {
        if (spriteRenderer == null) return;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        Material flashMat = flashWhiteMaterial;

        for (int i = 0; i < flashBlinks; i++)
        {
            // Ubah jadi siluet putih
            if (flashMat != null)
            {
                spriteRenderer.material = flashMat;
            }
            else
            {
                spriteRenderer.color = new Color(1f, 0.2f, 0.2f, 1f); // Fallback merah jika material belum ada
            }

            yield return new WaitForSeconds(flashDuration);

            // Kembalikan ke material normal
            if (flashMat != null)
            {
                spriteRenderer.material = originalMaterial;
            }
            else
            {
                spriteRenderer.color = Color.white;
            }

            // Jeda singkat antar blip
            if (i < flashBlinks - 1)
            {
                yield return new WaitForSeconds(flashDuration * 0.75f);
            }
        }

        flashCoroutine = null;
    }

    private void Die()
    {
        isDead = true;

        if (spriteRenderer != null && originalMaterial != null)
        {
            spriteRenderer.material = originalMaterial;
            spriteRenderer.color = Color.white;
        }

        OnDeath?.Invoke();
    }

    private void OnDisable()
    {
        if (spriteRenderer != null && originalMaterial != null)
        {
            spriteRenderer.material = originalMaterial;
            spriteRenderer.color = Color.white;
        }
    }
}
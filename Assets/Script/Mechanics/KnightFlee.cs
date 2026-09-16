using System.Collections;
using UnityEngine;

public class KnightFlee : MonoBehaviour
{
    [Header("Settings")]
    public float fleeSpeed = 4f;
    public float fadeDuration = 1.2f;
    public GameObject kickButton;   // Slot untuk KickButton UI

    private SpriteRenderer spriteRenderer;
    private bool isFleeing = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void KickAndFlee()
    {
        if (isFleeing) return;

        // Sembunyikan tombol Kick saat dipencet
        if (kickButton != null)
            kickButton.SetActive(false);

        StartCoroutine(FleeRoutine());
    }

    private IEnumerator FleeRoutine()
    {
        isFleeing = true;

        KnightAmbush ambush = GetComponent<KnightAmbush>();
        if (ambush == null && transform.parent != null) 
            ambush = transform.parent.GetComponentInChildren<KnightAmbush>();
        if (ambush != null) ambush.enabled = false;

        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        float timer = 0f;
        Color startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            transform.Translate(Vector3.right * fleeSpeed * Time.deltaTime);

            if (spriteRenderer != null)
            {
                float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
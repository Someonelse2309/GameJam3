using UnityEngine;
using UnityEngine.UI;

public class TransitionEffects : MonoBehaviour
{
    [Header("References")]
    public Image fadeImage; // Hitam overlay untuk fade

    [Header("Settings")]
    public float fadeDuration = 1f;

    private Color fadeColor;

    void Start()
    {
        if (fadeImage != null)
        {
            fadeColor = fadeImage.color;
            fadeColor.a = 0;
            fadeImage.color = fadeColor;
        }
    }

    // ======== SCREEN SHAKE ========

    public void TriggerShake(float duration = 0.5f, float intensity = 1f)
    {
        ScreenShake shaker = UnityEngine.Object.FindAnyObjectByType<ScreenShake>();
        if (shaker != null)
        {
            shaker.TriggerShake(duration, intensity);
        }
    }

    // ======== FADE EFFECT ========

    public void FadeToBlack(System.Action onComplete = null)
    {
        StartCoroutine(Fade(true, onComplete));
    }

    public void FadeFromBlack(System.Action onComplete = null)
    {
        StartCoroutine(Fade(false, onComplete));
    }

    System.Collections.IEnumerator Fade(bool toBlack, System.Action onComplete)
    {
        if (fadeImage == null) yield break;

        float targetAlpha = toBlack ? 1f : 0f;
        float startAlpha = fadeImage.color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            fadeColor.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            fadeImage.color = fadeColor;
            yield return null;
        }

        // Ensure exact
        fadeColor.a = targetAlpha;
        fadeImage.color = fadeColor;

        onComplete?.Invoke();
    }

    // ======== QUICK FLASH ========

    public void QuickFlash(Color flashColor, float duration = 0.1f)
    {
        if (fadeImage == null) return;
        StartCoroutine(Flash(flashColor, duration));
    }

    System.Collections.IEnumerator Flash(Color color, float duration)
    {
        Color original = fadeImage.color;
        fadeImage.color = color;
        yield return new WaitForSeconds(duration);
        fadeImage.color = original;
    }
}

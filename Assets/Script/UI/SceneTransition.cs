using UnityEngine;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public Image blackOverlay;
    public float fadeInSpeed = 1f;
    public float fadeOutSpeed = 1f;
    public float holdDuration = 1f;
    public float targetAlpha = 0.7f;

    [Header("Overlays to control")]
    public GameObject[] overlaysToDisable; // Overlay yang dimatiin saat transisi

    private bool isTransitioning = false;

    void Start()
    {
        if (blackOverlay != null)
        {
            // Start with transparent
            Color c = blackOverlay.color;
            c.a = 0;
            blackOverlay.color = c;
        }
    }

    public void StartTransition(System.Action onMidpoint = null, System.Action onComplete = null)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionCoroutine(onMidpoint, onComplete));
    }

    System.Collections.IEnumerator TransitionCoroutine(System.Action onMidpoint, System.Action onComplete)
    {
        isTransitioning = true;

        // Fade to black
        if (blackOverlay != null)
        {
            float t = 0;
            while (t < targetAlpha)
            {
                t += Time.deltaTime * fadeInSpeed;
                Color c = blackOverlay.color;
                c.a = Mathf.Clamp01(t);
                blackOverlay.color = c;
                yield return null;
            }
        }

        // Call midpoint action (e.g., change scene)
        onMidpoint?.Invoke();

        // Hold
        yield return new WaitForSeconds(holdDuration);

        // Fade out
        if (blackOverlay != null)
        {
            float t = 1f;
            while (t > 0f)
            {
                t -= Time.deltaTime * fadeOutSpeed;
                Color c = blackOverlay.color;
                c.a = Mathf.Clamp01(t);
                blackOverlay.color = c;
                yield return null;
            }
        }

        isTransitioning = false;
        onComplete?.Invoke();
    }

    public void FadeInBlack(System.Action onComplete = null)
    {
        if (isTransitioning) return;
        StartCoroutine(FadeIn(onComplete));
    }

    System.Collections.IEnumerator FadeIn(System.Action onComplete)
    {
        isTransitioning = true;

        // Disable overlays
        if (overlaysToDisable != null)
        {
            foreach (GameObject overlay in overlaysToDisable)
            {
                if (overlay != null)
                    overlay.SetActive(false);
            }
        }

        if (blackOverlay != null)
        {
            float t = 0;
            while (t < targetAlpha)
            {
                t += Time.deltaTime * fadeInSpeed;
                Color c = blackOverlay.color;
                c.a = Mathf.Clamp01(t);
                blackOverlay.color = c;
                yield return null;
            }
            // Ensure exact target alpha
            Color finalColor = blackOverlay.color;
            finalColor.a = targetAlpha;
            blackOverlay.color = finalColor;
        }

        isTransitioning = false;
        onComplete?.Invoke();
    }

    public void FadeOutBlack(System.Action onComplete = null)
    {
        if (isTransitioning) return;
        StartCoroutine(FadeOut(onComplete));
    }

    System.Collections.IEnumerator FadeOut(System.Action onComplete)
    {
        isTransitioning = true;

        if (blackOverlay != null)
        {
            float t = 1f;
            while (t > 0f)
            {
                t -= Time.deltaTime * fadeOutSpeed;
                Color c = blackOverlay.color;
                c.a = Mathf.Clamp01(t);
                blackOverlay.color = c;
                yield return null;
            }
        }

        isTransitioning = false;
        onComplete?.Invoke();
    }
}

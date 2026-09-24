using UnityEngine;
using UnityEngine.UI;

public class FilmGrainOverlay : MonoBehaviour
{
    private RawImage rawImage;
    private float targetAlpha = 0.3f;
    private float fadeSpeed = 1f;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        if (rawImage == null)
        {
            Debug.LogWarning("FilmGrainOverlay: No RawImage found!");
        }
    }

    public void SetOpacity(float alpha)
    {
        if (rawImage == null) return;

        Color c = rawImage.color;
        c.a = Mathf.Clamp01(alpha);
        rawImage.color = c;
    }

    public void FadeOpacity(float targetAlpha, float speed = 1f)
    {
        this.targetAlpha = targetAlpha;
        this.fadeSpeed = speed;
        enabled = true;
    }

    void Update()
    {
        if (rawImage == null) return;

        Color c = rawImage.color;
        float diff = targetAlpha - c.a;

        if (Mathf.Abs(diff) > 0.01f)
        {
            c.a += diff * fadeSpeed * Time.deltaTime;
            rawImage.color = c;
        }
        else
        {
            enabled = false;
        }
    }
}

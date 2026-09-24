using UnityEngine;

public class CDIndicator : MonoBehaviour
{
    [Header("Settings")]
    public Color glowColor = Color.yellow;
    public float pulseSpeed = 2f;
    public float minAlpha = 0.3f;
    public float maxAlpha = 0.9f;
    public float glowScale = 1f;
    public float glowSize = 0.5f;

    private SpriteRenderer sr;
    private GameObject glowObj;

    void Start()
    {
        // Buat glow circle
        glowObj = new GameObject("CDGlow");
        glowObj.transform.SetParent(transform);
        glowObj.transform.localPosition = Vector3.zero;

        sr = glowObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(glowColor.r, glowColor.g, glowColor.b, minAlpha);
        sr.sortingOrder = 50;
        glowObj.transform.localScale = Vector3.one * glowScale;
    }

    Sprite CreateCircleSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);

                if (dist <= radius)
                {
                    // Soft edge glow
                    float edge = 1f - (dist / radius);
                    edge = edge * edge; // Quadratic falloff
                    colors[y * size + x] = Color.white * edge;
                }
                else
                {
                    colors[y * size + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(colors);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    void Update()
    {
        if (sr == null) return;

        // Pulse animation
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
        sr.color = new Color(glowColor.r, glowColor.g, glowColor.b, alpha);

        // Scale pulse (dari luar ke dalam = 1.3 ke 0.7)
        float scalePulse = Mathf.Lerp(glowScale * 1.3f, glowScale * 0.7f, t);
        glowObj.transform.localScale = Vector3.one * scalePulse;
    }

    public void Hide()
    {
        if (glowObj != null)
            glowObj.SetActive(false);
    }

    public void Show()
    {
        if (glowObj != null)
            glowObj.SetActive(true);
    }
}

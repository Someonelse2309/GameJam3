using UnityEngine;

public class ArrowIndicatorVisual : MonoBehaviour
{
    [Header("Settings")]
    public float offsetY = 1.5f;
    public float arrowScale = 0.3f;
    public Color color = Color.yellow;

    [Header("Animation")]
    public float pulseSpeed = 2f;
    public float minAlpha = 0.3f;
    public float maxAlpha = 0.9f;

    [Header("Next Target")]
    public GameObject nextArrow;
    public GameObject finalTrigger;

    private SpriteRenderer sr;
    private GameObject arrowObj;
    private bool hasTriggered = false;

    void Start()
    {
        // Buat arrow object
        arrowObj = new GameObject("ArrowIndicator");
        arrowObj.transform.SetParent(transform);
        arrowObj.transform.localPosition = new Vector3(0, offsetY, 0);

        sr = arrowObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateArrowSprite();
        sr.color = new Color(color.r, color.g, color.b, minAlpha);
        sr.sortingOrder = 100;
        arrowObj.transform.localScale = Vector3.one * arrowScale;

        // Pastikan rotation mengikuti parent
        arrowObj.transform.localRotation = Quaternion.identity;

        // Hide next targets initially
        if (nextArrow != null)
            nextArrow.SetActive(false);
        if (finalTrigger != null)
            finalTrigger.SetActive(false);
    }

    Sprite CreateArrowSprite()
    {
        int width = 32;
        int height = 32;
        Texture2D tex = new Texture2D(width, height);
        Color[] colors = new Color[width * height];

        Vector2 tip = new Vector2(width / 2f, height - 2);
        Vector2 leftBase = new Vector2(width / 4f, height / 4f);
        Vector2 rightBase = new Vector2(width * 3f / 4f, height / 4f);
        Vector2 bottomLeft = new Vector2(width / 3f, 2);
        Vector2 bottomRight = new Vector2(width * 2f / 3f, 2);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 p = new Vector2(x, y);

                bool inTopTriangle = IsPointInTriangle(p, tip, leftBase, rightBase);
                bool inBottomRect = p.x >= bottomLeft.x && p.x <= bottomRight.x && p.y >= 0 && p.y <= bottomLeft.y + 2;

                if (inTopTriangle || inBottomRect)
                {
                    colors[y * width + x] = Color.white;
                }
                else
                {
                    colors[y * width + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(colors);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.3f), width);
    }

    bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Sign(p, a, b);
        float d2 = Sign(p, b, c);
        float d3 = Sign(p, c, a);

        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(hasNeg && hasPos);
    }

    float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    void Update()
    {
        if (sr == null) return;

        // Reset rotation ke parent
        arrowObj.transform.localRotation = Quaternion.identity;

        // Pulse animation
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
        sr.color = new Color(color.r, color.g, color.b, alpha);

        // Bob up and down
        float bobOffset = Mathf.Sin(Time.time * pulseSpeed * 0.5f) * 0.1f;
        arrowObj.transform.localPosition = new Vector3(0, offsetY + bobOffset, 0);

        // Hide during dialogue
        if (DialogueManagerEP1.instanceEP1 != null && DialogueManagerEP1.instanceEP1.IsDialogueActive())
        {
            sr.enabled = false;
        }
        else
        {
            sr.enabled = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            TriggerNext();
        }
    }

    void TriggerNext()
    {
        if (arrowObj != null)
            arrowObj.SetActive(false);

        if (nextArrow != null)
        {
            nextArrow.SetActive(true);
        }
        else if (finalTrigger != null)
        {
            finalTrigger.SetActive(true);
        }

        gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}

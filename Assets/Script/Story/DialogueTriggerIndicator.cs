using UnityEngine;

public class DialogueTriggerIndicator : MonoBehaviour
{
    [Header("Indicator Object (drag dari Hierarchy, optional)")]
    public GameObject indicatorObject;

    [Header("Settings")]
    public float radius = 0.5f;
    public float offsetY = 1f;
    public Color color = Color.yellow;

    private SpriteRenderer sr;
    private DialogueTrigger dialogueTrigger;
    private float time = 0f;

    void Start()
    {
        dialogueTrigger = GetComponent<DialogueTrigger>();

        // Kalau indicatorObject belum di-assign, buat sendiri
        if (indicatorObject == null)
        {
            indicatorObject = new GameObject("Indicator");
            indicatorObject.transform.SetParent(transform);
            indicatorObject.transform.localPosition = new Vector3(0, offsetY, 0);
        }

        sr = indicatorObject.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = indicatorObject.AddComponent<SpriteRenderer>();
        }

        sr.sprite = CreateCircleSprite();
        sr.color = new Color(color.r, color.g, color.b, 0.5f);
        sr.sortingOrder = 100;
    }

    Sprite CreateCircleSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float innerRadius = size / 2f - 2;
        float outerRadius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);

                if (dist <= innerRadius)
                {
                    // Isi circle dengan fade di tepi
                    float edge = Mathf.InverseLerp(innerRadius, innerRadius - 2, dist);
                    colors[y * size + x] = Color.white * (1f - edge * 0.3f);
                }
                else if (dist <= outerRadius)
                {
                    // Outline
                    colors[y * size + x] = Color.white;
                }
                else
                {
                    colors[y * size + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(colors);
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    void Update()
    {
        if (sr == null) return;

        // Pulse effect
        time += Time.deltaTime * 3f;
        float alpha = Mathf.Lerp(0.2f, 0.7f, (Mathf.Sin(time) + 1f) / 2f);
        sr.color = new Color(color.r, color.g, color.b, alpha);

        // Hide kalau dialogue aktif
        if (DialogueManager.instance != null && DialogueManager.instance.IsDialogueActive())
        {
            sr.enabled = false;
        }
        else
        {
            sr.enabled = true;
        }
    }

    // Gizmo di Scene View
    void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    void OnDestroy()
    {
        // Jangan destroy indicator kalau ini bukan yang buat
        // karena indicator bisa di-assign dari luar
    }
}

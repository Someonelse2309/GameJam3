using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ArrowIndicator : MonoBehaviour
{
    public enum DirectionMode { PointToTargetNPC, FixedDirection }

    [Header("Target & Direction")]
    public DirectionMode mode = DirectionMode.PointToTargetNPC;
    [Tooltip("Drag NPC yang ingin dituju (contoh: NPC_Pedagang atau NPC_Beggar)")]
    public Transform targetNPC;
    [Tooltip("Digunakan jika memilih FixedDirection (-90 = Kanan, 90 = Kiri, 0 = Atas, 180 = Bawah)")]
    public float fixedAngle = -90f; 

    [Header("Visual Settings")]
    public Sprite customArrowSprite; 
    public float offsetY = 0f;
    public float arrowScale = 0.6f;
    public Color arrowColor = new Color(1f, 0.85f, 0.1f, 1f); // Kuning cerah
    public string sortingLayerName = "Default";
    public int sortingOrder = 20; // Agar selalu di atas jalan / tilemap

    [Header("Animation")]
    public float pulseSpeed = 3f;
    public float minAlpha = 0.4f;
    public float maxAlpha = 1.0f;
    public float bounceDistance = 0.18f;

    [Header("Waypoint Sequence (Chaining)")]
    [Tooltip("Panah berikutnya yang akan aktif saat player melewati trigger ini")]
    public GameObject nextArrow;
    public GameObject finalTrigger;

    private SpriteRenderer sr;
    private GameObject arrowVisual;
    private bool hasTriggered = false;

    void Awake()
    {
        // Pastikan Collider2D selalu Trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void Start()
    {
        // Buat child visual panah
        arrowVisual = new GameObject("Visual_Arrow");
        arrowVisual.transform.SetParent(transform);
        arrowVisual.transform.localPosition = new Vector3(0, offsetY, 0);
        arrowVisual.transform.localScale = Vector3.one * arrowScale;

        sr = arrowVisual.AddComponent<SpriteRenderer>();
        sr.sprite = customArrowSprite != null ? customArrowSprite : GeneratePixelArrowSprite();
        sr.color = arrowColor;
        sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = sortingOrder;

        // Matikan panah berikutnya di awal
        if (nextArrow != null) nextArrow.SetActive(false);
        if (finalTrigger != null) finalTrigger.SetActive(false);
    }

    void Update()
    {
        if (sr == null || arrowVisual == null) return;

        // 1. Sembunyikan panah jika sedang ada dialog aktif
        bool isDialogActive = DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive;
        sr.enabled = !isDialogActive;

        if (!sr.enabled) return;

        // 2. Rotasi Panah Menghadap Target
        UpdateRotation();

        // 3. Animasi Denyut Transparansi (Pulse)
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
        Color c = arrowColor;
        c.a = alpha;
        sr.color = c;

        // 4. Animasi Maju-Mundur Sesuai Arah Panah (Bounce)
        float bounce = Mathf.Sin(Time.time * pulseSpeed * 1.5f) * bounceDistance;
        arrowVisual.transform.localPosition = new Vector3(0, offsetY, 0) + (arrowVisual.transform.up * bounce);
    }

    private void UpdateRotation()
    {
        if (mode == DirectionMode.PointToTargetNPC && targetNPC != null)
        {
            Vector2 dir = targetNPC.position - transform.position;
            // -90 derajat karena sprite dasar menghadap ke ATAS
            float angle = (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg) - 90f;
            arrowVisual.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            arrowVisual.transform.rotation = Quaternion.Euler(0, 0, fixedAngle);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            TriggerNext();
        }
    }

    private void TriggerNext()
    {
        if (nextArrow != null) nextArrow.SetActive(true);
        if (finalTrigger != null) finalTrigger.SetActive(true);
        gameObject.SetActive(false);
    }

    // Generator Sprite Pixel Art Panah Tajam (32x32) dengan border outline
    private Sprite GeneratePixelArrowSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point; // Pixel art tajam tanpa blur

        Color empty = Color.clear;
        Color fill = Color.white;
        Color border = new Color(0.1f, 0.1f, 0.1f, 0.9f); // Outline gelap tipis

        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = empty;

        // Matriks gambar panah menghadap ke ATAS (1 = Isi Putih, 2 = Outline Hitam)
        int[,] map = new int[32, 32];

        // Gambar Kepala Panah (Segitiga)
        for (int y = 14; y <= 28; y++)
        {
            int span = 28 - y; // Semakin ke bawah semakin melebar
            int left = 16 - span;
            int right = 15 + span;

            for (int x = left; x <= right; x++)
            {
                if (x >= 0 && x < 32)
                {
                    if (x == left || x == right || y == 14 || y == 28)
                        map[x, y] = 2; // Border
                    else
                        map[x, y] = 1; // Fill
                }
            }
        }

        // Gambar Batang Panah (Persegi)
        for (int y = 4; y <= 14; y++)
        {
            for (int x = 12; x <= 19; x++)
            {
                if (x == 12 || x == 19 || y == 4)
                    map[x, y] = 2; // Border
                else
                    map[x, y] = 1; // Fill
            }
        }

        // Terapkan pixel
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (map[x, y] == 1) pixels[y * size + x] = fill;
                else if (map[x, y] == 2) pixels[y * size + x] = border;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 24);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = arrowColor;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
        if (targetNPC != null)
        {
            Gizmos.DrawLine(transform.position, targetNPC.position);
        }
    }
}
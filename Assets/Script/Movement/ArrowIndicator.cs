using UnityEngine;

public class ArrowIndicator : MonoBehaviour
{
    [Header("Target NPC")]
    public Transform targetNPC;

    [Header("Screen Edge Clamping")]
    [Tooltip("Padding dari pinggir layar (0.08 = 8%)")]
    [Range(0.01f, 0.2f)]
    public float edgePadding = 0.08f;

    [Header("On-Screen Settings")]
    public float headOffsetY = 1.3f;
    public bool hideWhenOnScreen = true;

    [Header("Visual Bubble")]
    public Sprite bubbleSprite;
    public Color bubbleColor = Color.white;
    public Vector2 bubbleScale = new Vector2(0.18f, 0.18f);
    public string sortingLayerName = "Default";
    public int sortingOrder = 50;

    [Header("Animation")]
    public float bobSpeed = 4f;
    public float bobHeight = 0.1f;
    public float smoothSpeed = 15f;

    private Camera mainCamera;
    private SpriteRenderer sr;

    private void Awake()
    {
        mainCamera = Camera.main;

        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

        ApplyVisualProperties();
    }

    private void OnValidate()
    {
        ApplyVisualProperties();
    }

    private void ApplyVisualProperties()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (bubbleSprite != null) sr.sprite = bubbleSprite;
            sr.color = bubbleColor;
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = sortingOrder;
        }
        transform.localScale = new Vector3(bubbleScale.x, bubbleScale.y, 1f);
    }

    private void LateUpdate()
    {
        if (targetNPC == null)
        {
            SetVisible(false);
            return;
        }

        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 targetPos = targetNPC.position;
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(targetPos);

        bool isOffScreen = viewportPos.x <= 0f || viewportPos.x >= 1f ||
                           viewportPos.y <= 0f || viewportPos.y >= 1f ||
                           viewportPos.z < 0f;

        if (isOffScreen)
        {
            SetVisible(true);

            // Kunci posisi di batas viewport layar
            viewportPos.x = Mathf.Clamp(viewportPos.x, edgePadding, 1f - edgePadding);
            viewportPos.y = Mathf.Clamp(viewportPos.y, edgePadding, 1f - edgePadding);

            Vector3 worldPos = mainCamera.ViewportToWorldPoint(viewportPos);
            worldPos.z = 0f;

            transform.position = Vector3.Lerp(transform.position, worldPos, Time.deltaTime * smoothSpeed);
        }
        else
        {
            if (hideWhenOnScreen)
            {
                SetVisible(false);
            }
            else
            {
                SetVisible(true);

                float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                Vector3 headPos = targetPos + new Vector3(0f, headOffsetY + bobOffset, 0f);

                transform.position = Vector3.Lerp(transform.position, headPos, Time.deltaTime * smoothSpeed);
            }
        }

        transform.rotation = Quaternion.identity;
    }

    private void SetVisible(bool visible)
    {
        if (sr != null && sr.enabled != visible)
        {
            sr.enabled = visible;
        }
    }
}
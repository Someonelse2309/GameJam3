using UnityEngine;

public class QuestIndicator : MonoBehaviour
{
    [Header("Floating Animation")]
    public float floatSpeed = 3.5f;
    public float floatHeight = 0.08f;

    private Vector3 baseLocalPosition;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        baseLocalPosition = transform.localPosition;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Animasi melayang naik-turun halus khas RPG
        float newY = baseLocalPosition.y + (Mathf.Sin(Time.time * floatSpeed) * floatHeight);
        transform.localPosition = new Vector3(baseLocalPosition.x, newY, baseLocalPosition.z);
    }

    public void SetVisible(bool isVisible)
    {
        if (gameObject.activeSelf != isVisible)
        {
            gameObject.SetActive(isVisible);
        }
    }
}
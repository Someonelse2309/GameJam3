using UnityEngine;
using UnityEngine.Tilemaps;

public class RoofFade : MonoBehaviour
{
    [Header("Target Tilemap Atap/Tembok")]
    public Tilemap roofTilemap;

    [Header("Pengaturan Transparansi")]
    [Range(0f, 1f)] public float indoorAlpha = 0.2f; // Nilai transparansi saat di dalam (0 = hilang total, 0.2 = agak samar)
    public float fadeSpeed = 5f;

    private float targetAlpha = 1f;

    private void Update()
    {
        if (roofTilemap != null)
        {
            Color color = roofTilemap.color;
            color.a = Mathf.MoveTowards(color.a, targetAlpha, fadeSpeed * Time.deltaTime);
            roofTilemap.color = color;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            targetAlpha = indoorAlpha;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            targetAlpha = 1f;
        }
    }
}
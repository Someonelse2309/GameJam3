using UnityEngine;

public class SpriteMultiplyEffect : MonoBehaviour
{
    [Header("Multiply Settings")]
    public Color multiplyColor = new Color(0.2f, 0.15f, 0.1f); // coklat gelap
    [Range(0f, 1f)]
    public float multiplyStrength = 0.8f;

    [Header("Quick Presets")]
    public bool useSepia = false;
    public bool useDarkBrown = true;
    public bool useBlackWhite = false;

    private Material spriteMaterial;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;

        // Buat material baru (unique per objek)
        spriteMaterial = new Material(Shader.Find("Custom/SpriteMultiply"));
        spriteRenderer.material = spriteMaterial;

        ApplyPreset();
    }

    void ApplyPreset()
    {
        if (spriteMaterial == null) return;

        if (useSepia)
        {
            spriteMaterial.SetColor("_MultiplyColor", new Color(0.4f, 0.35f, 0.3f));
            spriteMaterial.SetFloat("_MultiplyStrength", 0.6f);
        }
        else if (useDarkBrown)
        {
            spriteMaterial.SetColor("_MultiplyColor", multiplyColor);
            spriteMaterial.SetFloat("_MultiplyStrength", multiplyStrength);
        }
        else if (useBlackWhite)
        {
            spriteMaterial.SetColor("_MultiplyColor", new Color(0.3f, 0.3f, 0.3f));
            spriteMaterial.SetFloat("_MultiplyStrength", 0.7f);
        }
    }

    void OnDestroy()
    {
        // Cleanup material
        if (spriteMaterial != null)
        {
            Destroy(spriteMaterial);
        }
    }

    // Update values dari Inspector
    void OnValidate()
    {
        if (spriteMaterial != null)
        {
            spriteMaterial.SetColor("_MultiplyColor", multiplyColor);
            spriteMaterial.SetFloat("_MultiplyStrength", multiplyStrength);
        }
    }
}

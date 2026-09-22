using UnityEngine;
using System.Collections.Generic;

public class DarkSpotsTextureGenerator : MonoBehaviour
{
    public int textureSize = 512;
    public int spotCount = 100;
    public float minSpotSize = 5f;
    public float maxSpotSize = 30f;
    public float spotOpacity = 0.3f;

    void Start()
    {
        GenerateDarkSpotsTexture();
    }

    public void GenerateDarkSpotsTexture()
    {
        Texture2D tex = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Repeat;

        Color[] pixels = new Color[textureSize * textureSize];

        // Start with transparent
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        // Random spots
        for (int i = 0; i < spotCount; i++)
        {
            float x = Random.Range(0, textureSize);
            float y = Random.Range(0, textureSize);
            float size = Random.Range(minSpotSize, maxSpotSize);
            float opacity = Random.Range(spotOpacity * 0.5f, spotOpacity);

            DrawSpot(pixels, (int)x, (int)y, size, opacity);
        }

        tex.SetPixels(pixels);
        tex.Apply();

        // Save
        byte[] bytes = tex.EncodeToPNG();
        string path = Application.dataPath + "/Shader/DarkSpotsTexture.png";
        System.IO.File.WriteAllBytes(path, bytes);
        Debug.Log("Dark spots texture saved to: " + path);
    }

    void DrawSpot(Color[] pixels, int cx, int cy, float radius, float opacity)
    {
        int minX = Mathf.Max(0, cx - (int)radius - 2);
        int maxX = Mathf.Min(textureSize - 1, cx + (int)radius + 2);
        int minY = Mathf.Max(0, cy - (int)radius - 2);
        int maxY = Mathf.Min(textureSize - 1, cy + (int)radius + 2);

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                if (dist < radius)
                {
                    // Soft edge
                    float edgeFade = 1f - (dist / radius);
                    edgeFade = edgeFade * edgeFade; // Quadratic falloff
                    float finalOpacity = opacity * edgeFade;

                    int idx = y * textureSize + x;
                    Color existing = pixels[idx];
                    Color spotColor = new Color(0, 0, 0, finalOpacity);

                    // Blend
                    pixels[idx] = Color.Lerp(existing, spotColor, finalOpacity);
                }
            }
        }
    }
}

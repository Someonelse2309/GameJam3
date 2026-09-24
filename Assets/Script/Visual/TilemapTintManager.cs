using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapTintManager : MonoBehaviour
{
    public enum TintType
    {
        None,
        Brown,
        Black,
        Clear
    }

    [Header("Target")]
    public GameObject targetGrid;

    [Header("Default Tint (Brown)")]
    public Color tintColor = new Color(0.6f, 0.45f, 0.3f);
    [Range(0f, 1f)]
    public float tintStrength = 0.4f;

    [Header("Black Tint")]
    public Color blackTintColor = new Color(0.1f, 0.1f, 0.1f);
    [Range(0f, 1f)]
    public float blackTintStrength = 0.6f;

    private Tilemap[] tilemaps;
    private Color[] originalColors;

    void Start()
    {
        SetupTint();
        ApplyBrownTint();
    }

    void SetupTint()
    {
        if (targetGrid == null)
        {
            Grid grid = FindFirstObjectByType<Grid>();
            if (grid != null)
            {
                targetGrid = grid.gameObject;
            }
        }

        if (targetGrid == null) return;

        tilemaps = targetGrid.GetComponentsInChildren<Tilemap>();
        if (tilemaps == null || tilemaps.Length == 0) return;

        originalColors = new Color[tilemaps.Length];
        for (int i = 0; i < tilemaps.Length; i++)
        {
            if (tilemaps[i] != null)
            {
                originalColors[i] = tilemaps[i].color;
            }
        }
    }

    public void ApplyTint(TintType type)
    {
        switch (type)
        {
            case TintType.Brown:
                ApplyBrownTint();
                break;
            case TintType.Black:
                ApplyBlackTint();
                break;
            case TintType.Clear:
                ClearTint();
                break;
        }
    }

    public void ApplyBrownTint()
    {
        Color newColor = Color.Lerp(Color.white, tintColor, tintStrength);
        SetTilemapColors(newColor);
    }

    public void ApplyBlackTint()
    {
        Color newColor = Color.Lerp(Color.white, blackTintColor, blackTintStrength);
        SetTilemapColors(newColor);
    }

    public void ClearTint()
    {
        if (originalColors != null)
        {
            for (int i = 0; i < tilemaps.Length; i++)
            {
                if (tilemaps[i] != null)
                {
                    tilemaps[i].color = originalColors[i];
                }
            }
        }
    }

    void SetTilemapColors(Color color)
    {
        if (tilemaps == null) return;

        foreach (var tm in tilemaps)
        {
            if (tm != null)
            {
                tm.color = color;
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MedkitSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject medkitPrefab;
    public Tilemap floorTilemap;

    [Header("Spawn Settings")]
    [Range(1, 50)]
    public int spawnCount = 5;          // Jumlah medkit yang disebar
    public LayerMask obstacleLayer;     // Layer bangunan/tembok agar medkit tidak spawn tertimpa tembok
    public float checkRadius = 0.25f;

    private void Start()
    {
        SpawnMedkits();
    }

    [ContextMenu("Spawn Medkits")]
    public void SpawnMedkits()
    {
        if (medkitPrefab == null || floorTilemap == null)
        {
            Debug.LogWarning("Medkit Prefab atau Floor Tilemap belum dipasang!");
            return;
        }

        // 1. Ambil semua koordinat tile yang aktif di layer Floor
        List<Vector3Int> availableTiles = new List<Vector3Int>();
        BoundsInt bounds = floorTilemap.cellBounds;

        foreach (Vector3Int cellPos in bounds.allPositionsWithin)
        {
            if (floorTilemap.HasTile(cellPos))
            {
                Vector3 worldPos = floorTilemap.GetCellCenterWorld(cellPos);

                // Pastikan titik tersebut tidak sedang menabrak dinding / bangunan
                if (obstacleLayer.value == 0 || !Physics2D.OverlapCircle(worldPos, checkRadius, obstacleLayer))
                {
                    availableTiles.Add(cellPos);
                }
            }
        }

        if (availableTiles.Count == 0) return;

        // 2. Acak urutan posisi cell (Shuffle)
        for (int i = 0; i < availableTiles.Count; i++)
        {
            Vector3Int temp = availableTiles[i];
            int randomIndex = Random.Range(i, availableTiles.Count);
            availableTiles[i] = availableTiles[randomIndex];
            availableTiles[randomIndex] = temp;
        }

        // 3. Spawn medkit sesuai jumlah yang ditentukan
        int totalToSpawn = Mathf.Min(spawnCount, availableTiles.Count);
        for (int i = 0; i < totalToSpawn; i++)
        {
            Vector3 spawnWorldPos = floorTilemap.GetCellCenterWorld(availableTiles[i]);
            Instantiate(medkitPrefab, spawnWorldPos, Quaternion.identity, transform);
        }
    }
}
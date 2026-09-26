using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [Header("Batas Peta (World Bounds)")]
    public Vector2 worldMin = new Vector2(-45f, 2.6f);
    public Vector2 worldMax = new Vector2(117f, 11.7f);

    [Header("Target")]
    public Transform playerTransform;
    public List<Transform> diskSpots = new List<Transform>();

    [Header("UI References")]
    public RectTransform mapArea;         // MapArea
    public RectTransform playerMarker;    // PlayerMarker
    public GameObject diskMarkerPrefab;   // Prefab DiskMarker_Red
    public Button minimapButton;          // Opsional (bisa dikosongkan jika tidak butuh tombol)

    [Header("Settings")]
    public bool alwaysShowDiskMarkers = true; // Langsung aktif otomatis

    private List<RectTransform> diskMarkerInstances = new List<RectTransform>();
    private bool showDiskMarkers = true;

    private void Start()
    {
        // Cari player otomatis jika kosong
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        // Cari GuardedDiskSpot otomatis jika list kosong
        if (diskSpots.Count == 0)
        {
            GuardedDiskSpot[] spots = FindObjectsByType<GuardedDiskSpot>(FindObjectsSortMode.None);
            foreach (var spot in spots)
            {
                diskSpots.Add(spot.transform);
            }
        }

        showDiskMarkers = alwaysShowDiskMarkers;

        // Spawn dan langsung tampilkan
        SpawnDiskMarkers();

        // Tetap pasang listener jika sewaktu-waktu minimap ingin bisa diklik untuk toggle
        if (minimapButton != null)
        {
            minimapButton.onClick.AddListener(ToggleDiskMarkers);
        }
    }

    private void Update()
    {
        UpdatePlayerMarker();

        if (showDiskMarkers)
        {
            UpdateDiskMarkers();
        }
    }

    private void SpawnDiskMarkers()
    {
        foreach (Transform disk in diskSpots)
        {
            if (disk == null) continue;

            GameObject markerObj = Instantiate(diskMarkerPrefab, mapArea);
            RectTransform markerRect = markerObj.GetComponent<RectTransform>();
            
            // Langsung tampilkan sejak awal
            markerObj.SetActive(showDiskMarkers);
            diskMarkerInstances.Add(markerRect);
        }

        // Langsung posisikan marker di frame pertama
        if (showDiskMarkers)
        {
            UpdateDiskMarkers();
        }
    }

    private void UpdatePlayerMarker()
    {
        if (playerTransform != null && playerMarker != null)
        {
            playerMarker.anchoredPosition = WorldToMinimapPosition(playerTransform.position);
        }
    }

    private void UpdateDiskMarkers()
    {
        for (int i = 0; i < diskSpots.Count; i++)
        {
            // Jika disk sudah hancur/diambil, hilangkan dot merahnya
            if (diskSpots[i] == null || !diskSpots[i].gameObject.activeInHierarchy)
            {
                if (i < diskMarkerInstances.Count && diskMarkerInstances[i] != null)
                    diskMarkerInstances[i].gameObject.SetActive(false);
                continue;
            }

            if (i < diskMarkerInstances.Count && diskMarkerInstances[i] != null)
            {
                diskMarkerInstances[i].anchoredPosition = WorldToMinimapPosition(diskSpots[i].position);
            }
        }
    }

    private Vector2 WorldToMinimapPosition(Vector3 worldPos)
    {
        float normX = Mathf.InverseLerp(worldMin.x, worldMax.x, worldPos.x);
        float normY = Mathf.InverseLerp(worldMin.y, worldMax.y, worldPos.y);

        float mapWidth = mapArea.rect.width;
        float mapHeight = mapArea.rect.height;

        float uiX = (normX - 0.5f) * mapWidth;
        float uiY = (normY - 0.5f) * mapHeight;

        return new Vector2(uiX, uiY);
    }

    public void ToggleDiskMarkers()
    {
        showDiskMarkers = !showDiskMarkers;

        for (int i = 0; i < diskMarkerInstances.Count; i++)
        {
            if (diskMarkerInstances[i] != null && diskSpots[i] != null && diskSpots[i].gameObject.activeInHierarchy)
            {
                diskMarkerInstances[i].gameObject.SetActive(showDiskMarkers);
            }
        }
    }
}
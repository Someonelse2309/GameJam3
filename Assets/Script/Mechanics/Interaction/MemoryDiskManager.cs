using UnityEngine;
using TMPro;

public class MemoryDiskManager : MonoBehaviour
{
    public static MemoryDiskManager Instance { get; private set; }

    [Header("Disk Progress")]
    public int collectedDisks = 0;
    public const int REQUIRED_EXPLORATION_DISKS = 4;
    public const int TOTAL_DISKS = 5;

    [Header("Prerequisite")]
    public StreetAmbushTrigger streetAmbushTrigger;

    [Header("UI Indicator (Under Bag Button)")]
    [Tooltip("Panel UI CD di bawah tombol tas")]
    public GameObject diskCounterRoot;

    [Tooltip("Teks angka [X]/5")]
    public TMP_Text diskCounterText;

    [Tooltip("Sembunyikan indikator CD sampai street ambush selesai")]
    public bool hideUntilAmbushDone = true;

    [Header("Navigation & Gate")]
    [Tooltip("Panah penunjuk jalan ke Boss Rooftop")]
    public GameObject arrowToBoss;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (streetAmbushTrigger == null)
            streetAmbushTrigger = FindFirstObjectByType<StreetAmbushTrigger>();

        UpdateDiskUI();
        UpdateQuestTracker();
    }

    private void Update()
    {
        // Pastikan indikator CD otomatis muncul tepat setelah ambush selesai
        if (diskCounterRoot != null && hideUntilAmbushDone)
        {
            bool isAmbushDone = streetAmbushTrigger != null && 
                               streetAmbushTrigger.currentState == StreetAmbushTrigger.AmbushState.Completed;

            if (diskCounterRoot.activeSelf != isAmbushDone)
            {
                diskCounterRoot.SetActive(isAmbushDone);
            }
        }
    }

    public void AddDisk()
    {
        collectedDisks = Mathf.Min(collectedDisks + 1, TOTAL_DISKS);

        if (GameAudioManager.Instance != null)
        {
            GameAudioManager.Instance.PlayQuestTrackerSFX();
        }

        UpdateDiskUI();
        UpdateQuestTracker();

        // Aktifkan panah boss jika 4 disk eksplorasi lengkap
        if (collectedDisks >= REQUIRED_EXPLORATION_DISKS)
        {
            if (streetAmbushTrigger != null && streetAmbushTrigger.currentState == StreetAmbushTrigger.AmbushState.Completed)
            {
                if (arrowToBoss != null)
                    arrowToBoss.SetActive(true);
            }
        }
    }

    // Dipanggil saat Boss Aoyama dikalahkan
    public void CollectBossDisk()
    {
        collectedDisks = TOTAL_DISKS; // Menjadi 5/5

        if (GameAudioManager.Instance != null)
        {
            GameAudioManager.Instance.PlayQuestTrackerSFX();
        }

        UpdateDiskUI();
        UpdateQuestTracker();
    }

    public bool HasCollectedAllExplorationDisks()
    {
        return collectedDisks >= REQUIRED_EXPLORATION_DISKS;
    }

    public void UpdateDiskUI()
    {
        if (diskCounterText != null)
        {
            diskCounterText.text = $"{collectedDisks}/{TOTAL_DISKS}";
        }
    }

    public void UpdateQuestTracker()
    {
        if (QuestTrackerUI.Instance != null)
        {
            QuestTrackerUI.Instance.UpdateQuestInfo();
        }
    }
}
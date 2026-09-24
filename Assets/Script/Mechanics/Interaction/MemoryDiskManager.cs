using UnityEngine;

public class MemoryDiskManager : MonoBehaviour
{
    public static MemoryDiskManager Instance { get; private set; }

    [Header("Disk Progress")]
    public int collectedDisks = 0;
    public const int REQUIRED_EXPLORATION_DISKS = 4;
    public const int TOTAL_DISKS = 5;

    [Header("Prerequisite")]
    public StreetAmbushTrigger streetAmbushTrigger;

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

        UpdateQuestTracker();
    }

    public void AddDisk()
    {
        collectedDisks++;

        if (GameAudioManager.Instance != null)
        {
            GameAudioManager.Instance.PlayQuestTrackerSFX(); 
        }

        UpdateQuestTracker();

        // Jika 4 disk sudah lengkap dan ambush sudah selesai, nyalakan panah ke boss
        if (collectedDisks >= REQUIRED_EXPLORATION_DISKS)
        {
            if (streetAmbushTrigger != null && streetAmbushTrigger.currentState == StreetAmbushTrigger.AmbushState.Completed)
            {
                if (arrowToBoss != null)
                    arrowToBoss.SetActive(true);
            }
        }
    }

    public bool HasCollectedAllExplorationDisks()
    {
        return collectedDisks >= REQUIRED_EXPLORATION_DISKS;
    }

    public void CollectBossDisk()
    {
        collectedDisks = TOTAL_DISKS;
        if (QuestTrackerUI.Instance != null)
            QuestTrackerUI.Instance.UpdateQuestInfo();
    }

    public void UpdateQuestTracker()
    {
        if (QuestTrackerUI.Instance != null)
        {
            QuestTrackerUI.Instance.UpdateQuestInfo();
        }
    }
}
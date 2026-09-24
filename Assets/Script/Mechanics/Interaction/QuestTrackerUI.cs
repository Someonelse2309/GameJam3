using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestTrackerUI : MonoBehaviour
{
    public static QuestTrackerUI Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("Tombol tanda seru (!)")]
    public Button toggleButton;

    [Tooltip("Panel pop-up deskripsi quest di samping tombol")]
    public GameObject questInfoPanel;

    [Tooltip("Teks judul quest")]
    public TMP_Text questTitleText;

    [Tooltip("Teks detail instruksi yang harus dilakukan user")]
    public TMP_Text questDescriptionText;

    [Header("Quest Sources (Otomatis dicari jika kosong)")]
    public BeggarTrigger beggarTrigger;
    public BlacksmithTrigger blacksmithTrigger;
    public StreetAmbushTrigger streetAmbushTrigger;
    public ItemData yakitoriItem;

    [Header("Quest Descriptions (Bisa diedit di Inspector)")]
    public string q1Title = "A Hungry Stranger";
    public string q1Desc = "Look for the beggar on the street (Tanaka Koji).";

    public string q2Title = "Finding Food";
    public string q2Desc = "Find and buy 2 portions of Yakitori from the food vendor (Seller).";

    public string q3Title = "Deliver Yakitori";
    public string q3Desc = "Bring the Yakitori back to Tanaka Koji.";

    public string q4Title = "Street Brawl";
    public string q4Desc = "Beat the bald Yakuza that is harassing Tanaka Koji!";

    public string q5Title = "Deliver Food";
    public string q5Desc = "Speak with Tanaka Koji to deliver the food.";

    public string q6Title = "The Blacksmith";
    public string q6Desc = "Find Ito Shun the blacksmith around the town.";

    public string q7Title = "Prove Your Resolve";
    public string q7Desc = "Defeat the Yakuza associates that are attacking to prove your strength!";

    public string q8Title = "Claim The Blade";
    public string q8Desc = "Speak with Ito Shun to claim your Katana back.";

    public string q9Title = "Syndicate Ambush";
    public string q9Desc = "Survive the street ambush and defeat the Suit Yakuza leader!";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (beggarTrigger == null) beggarTrigger = FindFirstObjectByType<BeggarTrigger>();
        if (blacksmithTrigger == null) blacksmithTrigger = FindFirstObjectByType<BlacksmithTrigger>();
        if (streetAmbushTrigger == null) streetAmbushTrigger = FindFirstObjectByType<StreetAmbushTrigger>();

        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleQuestPanel);
        }

        if (questInfoPanel != null)
        {
            questInfoPanel.SetActive(false);
        }

        UpdateQuestInfo();
    }

    private void Update()
    {
        if (questInfoPanel != null && questInfoPanel.activeSelf)
        {
            UpdateQuestInfo();
        }
    }

    public void ToggleQuestPanel()
    {
        if (questInfoPanel == null) return;

        bool willOpen = !questInfoPanel.activeSelf;
        questInfoPanel.SetActive(willOpen);

        if (willOpen)
        {
            UpdateQuestInfo();
        }
    }

    public void UpdateObjective(string desc, string title = "Memory Disk")
    {
        UpdateQuestInfo();
    }

    public void UpdateQuestInfo()
    {
        string title = "Active Objective";
        string desc = "Jelajahi area sekitar.";

        bool hasYakitori = InventoryManager.Instance != null && yakitoriItem != null && InventoryManager.Instance.HasItem(yakitoriItem);

        // 1. Quest Pengemis (Tanaka Koji)
        if (beggarTrigger != null && beggarTrigger.currentState != BeggarTrigger.QuestState.QuestCompleted)
        {
            switch (beggarTrigger.currentState)
            {
                case BeggarTrigger.QuestState.NotStarted:
                    title = q1Title;
                    desc = q1Desc;
                    break;

                case BeggarTrigger.QuestState.LookingForFood:
                    if (!hasYakitori)
                    {
                        title = q2Title;
                        desc = q2Desc;
                    }
                    else
                    {
                        title = q3Title;
                        desc = q3Desc;
                    }
                    break;

                case BeggarTrigger.QuestState.YakuzaEncounter:
                    title = q4Title;
                    desc = q4Desc;
                    break;

                case BeggarTrigger.QuestState.CombatFinished:
                    title = q5Title;
                    desc = q5Desc;
                    break;
            }
        }
        // 2. Quest Pandai Besi (Ito Shun)
        else if (blacksmithTrigger != null && blacksmithTrigger.currentState != BlacksmithTrigger.BlacksmithState.QuestCompleted)
        {
            switch (blacksmithTrigger.currentState)
            {
                case BlacksmithTrigger.BlacksmithState.Locked:
                case BlacksmithTrigger.BlacksmithState.ReadyForTalk:
                    title = q6Title;
                    desc = q6Desc;
                    break;

                case BlacksmithTrigger.BlacksmithState.InCombat:
                    title = q7Title;
                    desc = q7Desc;
                    break;

                case BlacksmithTrigger.BlacksmithState.CombatFinished:
                    title = q8Title;
                    desc = q8Desc;
                    break;
            }
        }
        // 3. Quest Ambush Jalanan (2 Wave + Duel Suit Yakuza)
        else if (streetAmbushTrigger != null && streetAmbushTrigger.currentState != StreetAmbushTrigger.AmbushState.Completed)
        {
            title = q9Title;
            desc = q9Desc;
        }
        // 4. SETELAH SUIT YAKUZA KALAH: Masuk Misi Memory Disk Ryu
        else if (MemoryDiskManager.Instance != null)
        {
            int disks = MemoryDiskManager.Instance.collectedDisks;
            if (disks < MemoryDiskManager.REQUIRED_EXPLORATION_DISKS)
            {
                title = "Memory Disks";
                desc = $"Find and collect the remaining Ryu Memory Disks ({disks}/4) before facing Aoyama!";
            }
            else
            {
                title = "Showdown at Rooftop";
                desc = "All Memory Disks collected! Enter the gate and face Aoyama on the Rooftop!";
            }
        }

        if (questTitleText != null) questTitleText.text = title;
        if (questDescriptionText != null) questDescriptionText.text = desc;
    }
}
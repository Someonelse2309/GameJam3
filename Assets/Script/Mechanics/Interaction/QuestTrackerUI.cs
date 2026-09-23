using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestTrackerUI : MonoBehaviour
{
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
    public ItemData yakitoriItem;

    [Header("Quest Descriptions (Bisa diedit di Inspector)")]
    public string q1Title = "A Hungry Stanger";
    public string q1Desc = "Bicara dengan pengemis di pinggir jalan (Tanaka Koji).";

    public string q2Title = "Finding Food";
    public string q2Desc = "Cari dan beli 2 porsi Yakitori dari pedagang makanan (Seller).";

    public string q3Title = "Deliver Yakitori";
    public string q3Desc = "Bawa Yakitori kembali ke Tanaka Koji.";

    public string q4Title = "Street Brawl";
    public string q4Desc = "Kalahkan Yakuza botak yang memalak Tanaka Koji!";

    public string q5Title = "Deliver Food";
    public string q5Desc = "Bicara kembali dengan Tanaka Koji untuk memberikan makanannya.";

    public string q6Title = "The Blacksmith";
    public string q6Desc = "Temui Ito Shun si pandai besi (Blacksmith) di seberang jalan.";

    public string q7Title = "Prove Your Resolve";
    public string q7Desc = "Kalahkan kroco Yakuza yang menyerang untuk membuktikan kemampuanmu!";

    public string q8Title = "Claim The Blade";
    public string q8Desc = "Bicara dengan Ito Shun untuk mengambil kembali Katana milikmu.";

    public string q9Title = "Onikoroshi Awakened";
    public string q9Desc = "Katana telah kembali. Bersiaplah menghadapi keluarga Yakuza Aoyama!";

    private void Start()
    {
        // Cari referensi otomatis jika belum di-drag di Inspector
        if (beggarTrigger == null) beggarTrigger = FindFirstObjectByType<BeggarTrigger>();
        if (blacksmithTrigger == null) blacksmithTrigger = FindFirstObjectByType<BlacksmithTrigger>();

        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleQuestPanel);
        }

        // Mulai dengan panel info tertutup
        if (questInfoPanel != null)
        {
            questInfoPanel.SetActive(false);
        }

        UpdateQuestInfo();
    }

    private void Update()
    {
        // Perbarui teks jika panel sedang terbuka agar selalu real-time
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

    public void UpdateQuestInfo()
    {
        string title = "Active Objective";
        string desc = "Jelajahi area sekitar.";

        bool hasYakitori = InventoryManager.Instance != null && yakitoriItem != null && InventoryManager.Instance.HasItem(yakitoriItem);

        // 1. Cek State Beggar Quest
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
        // 2. Beggar Selesai -> Cek State Blacksmith Quest
        else if (blacksmithTrigger != null)
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

                case BlacksmithTrigger.BlacksmithState.QuestCompleted:
                    title = q9Title;
                    desc = q9Desc;
                    break;
            }
        }

        if (questTitleText != null) questTitleText.text = title;
        if (questDescriptionText != null) questDescriptionText.text = desc;
    }
}
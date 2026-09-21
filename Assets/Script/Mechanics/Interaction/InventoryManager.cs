using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Carried Item UI")]
    public GameObject itemIconImage;

    [Header("Inventory Items")]
    public List<ItemData> items = new List<ItemData>();

    [Header("UI Pop-up Obtained")]
    public GameObject obtainedPopupPanel;
    public Image popupItemIcon;
    public TextMeshProUGUI popupItemNameText;
    public float popupDuration = 2.5f; // Pop-up otomatis hilang setelah 2.5 detik

    [Header("UI Inventory Bag")]
    public GameObject inventoryPanel;
    public Transform itemSlotContainer;
    public GameObject itemSlotPrefab;

    private Coroutine popupCoroutine;

    void Awake()
    {
        Instance = this;

        // Auto-hide semua UI saat game dimulai
        if (itemIconImage != null) itemIconImage.SetActive(false);
        if (obtainedPopupPanel != null) obtainedPopupPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    public void AddItem(ItemData item)
    {
        items.Add(item);
        ShowItemObtainedPopup(item);

        // Jika tas sedang terbuka, langsung perbarui isinya
        if (inventoryPanel != null && inventoryPanel.activeSelf)
        {
            RefreshInventoryUI();
        }
    }

    public void RemoveItem(ItemData item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            if (inventoryPanel != null && inventoryPanel.activeSelf)
            {
                RefreshInventoryUI();
            }
        }
    }

    public bool HasItem(ItemData item)
    {
        return item != null && items.Contains(item);
    }

    private void ShowItemObtainedPopup(ItemData item)
    {
        if (obtainedPopupPanel == null) return;

        if (popupItemIcon != null) popupItemIcon.sprite = item.itemIcon;
        if (popupItemNameText != null) popupItemNameText.text = "Obtained " + item.itemName;

        obtainedPopupPanel.SetActive(true);

        // Reset timer jika mendapat item berturut-turut
        if (popupCoroutine != null)
        {
            StopCoroutine(popupCoroutine);
        }
        popupCoroutine = StartCoroutine(HidePopupRoutine());
    }

    private IEnumerator HidePopupRoutine()
    {
        yield return new WaitForSeconds(popupDuration);
        if (obtainedPopupPanel != null)
        {
            obtainedPopupPanel.SetActive(false);
        }
    }

    public void CloseObtainedPopup()
    {
        if (obtainedPopupPanel != null)
            obtainedPopupPanel.SetActive(false);
    }

    public void ToggleInventoryPanel()
    {
        if (inventoryPanel == null) return;
        
        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);

        if (isActive)
        {
            RefreshInventoryUI();
        }
    }

    public void RefreshInventoryUI()
    {
        if (itemSlotContainer == null) return;

        // Bersihkan slot lama di grid
        foreach (Transform child in itemSlotContainer)
        {
            Destroy(child.gameObject);
        }

        // Buat slot baru untuk setiap item di dalam list
        foreach (ItemData item in items)
        {
            if (itemSlotPrefab != null)
            {
                GameObject slot = Instantiate(itemSlotPrefab, itemSlotContainer);
                Image slotIcon = slot.GetComponentInChildren<Image>();
                if (slotIcon != null)
                {
                    slotIcon.sprite = item.itemIcon;
                    slotIcon.enabled = true;
                }
            }
        }
    }
}
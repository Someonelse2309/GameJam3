using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory Data")]
    public List<ItemData> items = new List<ItemData>();
    public int maxCapacity = 9; // Grid 3x3

    [Header("UI References")]
    public GameObject inventoryPanel;
    public Transform itemSlotContainer; // Tempat slot item (Grid Layout Group 3x3)
    public GameObject itemSlotPrefab;

    [Header("Obtained & Equip Popup (Toast)")]
    public GameObject popupPanel;
    public Image popupItemIcon;
    public TMP_Text popupItemNameText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (popupPanel != null) popupPanel.SetActive(false);
    }

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool isActive = inventoryPanel.activeSelf;
            inventoryPanel.SetActive(!isActive);
            if (!isActive) RenderInventory();
        }
    }

    public void AddItem(ItemData item)
    {
        if (items.Count >= maxCapacity) return;

        items.Add(item);
        ShowObtainedPopup(item);

        // Jika tas sedang terbuka, langsung perbarui tampilannya
        if (inventoryPanel != null && inventoryPanel.activeSelf)
        {
            RenderInventory();
        }
    }

    public void RemoveItem(ItemData item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            RenderInventory();
        }
    }

    public bool HasItem(ItemData item)
    {
        return items.Contains(item);
    }

    public void RenderInventory()
    {
        if (itemSlotContainer == null || itemSlotPrefab == null) return;

        // 1. Bersihkan slot lama
        foreach (Transform child in itemSlotContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Buat slot berdasarkan item yang ada di list
        foreach (ItemData item in items)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, itemSlotContainer);

            // Pasang ikon sprite item
            Image iconImage = slotObj.GetComponent<Image>() ?? slotObj.GetComponentInChildren<Image>();
            if (iconImage != null && item != null && item.itemIcon != null)
            {
                iconImage.sprite = item.itemIcon;
                iconImage.color = Color.white;
                iconImage.gameObject.SetActive(true);
            }

            // 3. Pasang Button Click untuk memicu fungsi Use/Equip
            Button slotButton = slotObj.GetComponent<Button>();
            if (slotButton == null) slotButton = slotObj.AddComponent<Button>();

            ItemData currentItem = item;
            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(() => OnItemClicked(currentItem));
        }
    }

    // Fungsi saat slot item di-klik
    public void OnItemClicked(ItemData item)
    {
        if (item == null) return;

        // Jika item adalah Katana / Senjata
        if (item.isWeapon && PlayerMovement.Instance != null)
        {
            // Toggle pasang / lepas pedang
            bool toggleState = !PlayerMovement.Instance.isSwordEquipped;
            PlayerMovement.Instance.EquipSword(toggleState);

            // Tampilkan notifikasi toast di layar
            string statusMsg = toggleState ? "Equipped: " + item.itemName : "Unequipped: " + item.itemName;
            ShowStatusPopup(item, statusMsg);
        }
    }

    private void ShowObtainedPopup(ItemData item)
    {
        if (item == null) return;
        ShowStatusPopup(item, "Obtained: " + item.itemName);
    }

    private void ShowStatusPopup(ItemData item, string message)
    {
        if (popupPanel == null) return;

        StopAllCoroutines();
        StartCoroutine(PopupRoutine(item, message));
    }

    private IEnumerator PopupRoutine(ItemData item, string message)
    {
        if (popupItemIcon != null)
        {
            popupItemIcon.sprite = item != null ? item.itemIcon : null;
            popupItemIcon.gameObject.SetActive(item != null && item.itemIcon != null);
        }
        if (popupItemNameText != null)
        {
            popupItemNameText.text = message;
        }

        popupPanel.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        popupPanel.SetActive(false);
    }
}
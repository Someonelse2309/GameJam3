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

    [Header("Obtained Popup (Toast)")]
    public GameObject popupPanel;
    public Image popupItemIcon;
    public TMP_Text popupItemNameText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

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

        // Bersihkan slot lama
        foreach (Transform child in itemSlotContainer)
        {
            Destroy(child.gameObject);
        }

        // Buat slot berdasarkan item yang ada di list
        foreach (ItemData item in items)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, itemSlotContainer);

            // Mendeteksi komponen Image baik di root objek slot maupun di child-nya
            Image iconImage = slotObj.GetComponent<Image>() ?? slotObj.GetComponentInChildren<Image>();

            if (iconImage != null && item != null && item.itemIcon != null)
            {
                iconImage.sprite = item.itemIcon;
                iconImage.color = Color.white;
                iconImage.gameObject.SetActive(true);
            }
        }
    }

    private void ShowObtainedPopup(ItemData item)
    {
        if (popupPanel == null) return;

        StopAllCoroutines();
        StartCoroutine(PopupRoutine(item));
    }

    private System.Collections.IEnumerator PopupRoutine(ItemData item)
    {
        if (popupItemIcon != null)
        {
            popupItemIcon.sprite = item.itemIcon;
            popupItemIcon.gameObject.SetActive(item.itemIcon != null);
        }
        if (popupItemNameText != null)
        {
            popupItemNameText.text = "Obtained: " + item.itemName;
        }

        popupPanel.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        popupPanel.SetActive(false);
    }
}
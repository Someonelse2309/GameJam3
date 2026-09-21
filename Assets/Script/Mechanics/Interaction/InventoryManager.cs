using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Inventory Data")]
    public List<ItemData> items = new List<ItemData>();

    [Header("Bag UI")]
    public GameObject inventoryPanel;
    public Transform itemSlotContainer;
    public GameObject itemSlotPrefab;

    [Header("Obtained Popup (Toast)")]
    public GameObject popupPanel;
    public Image popupItemIcon;
    public TextMeshProUGUI popupItemNameText;
    private Coroutine popupCoroutine;

    private void Awake()
{
    if (Instance == null) Instance = this;
    else Destroy(this); // Hapus script-nya saja, jangan Destroy(gameObject)

    if (inventoryPanel != null) inventoryPanel.SetActive(false);
    if (popupPanel != null) popupPanel.SetActive(false);
}

    public void AddItem(ItemData item)
    {
        if (item == null) return;
        items.Add(item);
        ShowObtainedPopup(item);
        RefreshInventoryUI();
    }

    public void RemoveItem(ItemData item)
    {
        if (item == null) return;
        if (items.Contains(item))
        {
            items.Remove(item);
            RefreshInventoryUI();
        }
    }

    public bool HasItem(ItemData item)
    {
        if (item == null) return false;
        return items.Contains(item);
    }

    public void ToggleInventory()
    {
        if (inventoryPanel == null) return;
        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);
        if (isActive) RefreshInventoryUI();
    }

    public void RefreshInventoryUI()
    {
        if (itemSlotContainer == null || itemSlotPrefab == null) return;

        // Bersihkan slot lama
        foreach (Transform child in itemSlotContainer)
        {
            Destroy(child.gameObject);
        }

        // Spawn slot item baru
        foreach (ItemData item in items)
        {
            GameObject slot = Instantiate(itemSlotPrefab, itemSlotContainer);
            Image icon = slot.GetComponentInChildren<Image>();
            if (icon != null)
            {
                icon.sprite = item.itemIcon;
                icon.enabled = true;
            }
        }
    }

    public void ShowObtainedPopup(ItemData item)
    {
        if (popupPanel == null) return;

        if (popupItemIcon != null) popupItemIcon.sprite = item.itemIcon;
        if (popupItemNameText != null) popupItemNameText.text = "Obtained: " + item.itemName;

        popupPanel.SetActive(true);

        if (popupCoroutine != null) StopCoroutine(popupCoroutine);
        popupCoroutine = StartCoroutine(HidePopupRoutine(1.8f));
    }

    private IEnumerator HidePopupRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (popupPanel != null) popupPanel.SetActive(false);
    }
}
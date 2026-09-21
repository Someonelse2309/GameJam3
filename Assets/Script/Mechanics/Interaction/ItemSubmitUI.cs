using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSubmitUI : MonoBehaviour
{
    private static ItemSubmitUI instance;
    public static ItemSubmitUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<ItemSubmitUI>(FindObjectsInactive.Include);
            }
            return instance;
        }
    }

    [Header("UI Elements")]
    public GameObject submitPanel;
    public Image requiredItemIcon;
    public TextMeshProUGUI requiredItemNameText;
    public Button deliverButton;
    public Button cancelButton;

    private ItemData currentRequiredItem;
    private Action onDeliverSuccess;

    void Awake()
    {
        if (instance == null) instance = this;
        if (submitPanel != null) submitPanel.SetActive(false);
    }

    public void OpenSubmitScreen(ItemData requiredItem, Action onSuccess)
    {
        currentRequiredItem = requiredItem;
        onDeliverSuccess = onSuccess;

        if (submitPanel != null) submitPanel.SetActive(true);

        if (requiredItemIcon != null && requiredItem != null)
            requiredItemIcon.sprite = requiredItem.itemIcon;

        if (requiredItemNameText != null && requiredItem != null)
            requiredItemNameText.text = requiredItem.itemName;

        bool hasItem = InventoryManager.Instance != null && InventoryManager.Instance.HasItem(requiredItem);

        if (deliverButton != null)
        {
            deliverButton.interactable = hasItem;
            deliverButton.onClick.RemoveAllListeners();
            deliverButton.onClick.AddListener(OnDeliverClicked);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(CloseScreen);
        }
    }

    private void OnDeliverClicked()
    {
        if (InventoryManager.Instance != null && InventoryManager.Instance.HasItem(currentRequiredItem))
        {
            InventoryManager.Instance.RemoveItem(currentRequiredItem);
            CloseScreen();
            onDeliverSuccess?.Invoke();
        }
    }

    public void CloseScreen()
    {
        if (submitPanel != null) submitPanel.SetActive(false);
        onDeliverSuccess = null;
    }
}
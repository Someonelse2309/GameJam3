using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSubmitUI : MonoBehaviour
{
    public static ItemSubmitUI Instance { get; private set; }

    [Header("Submit Modal")]
    public GameObject submitPanel;
    public Image requiredItemIcon;
    public TextMeshProUGUI requiredItemNameText;
    public Button deliverButton;
    public Button cancelButton;

    private ItemData currentItem;
    private Action onSubmitSuccess;
    private Action onCancel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (submitPanel != null) submitPanel.SetActive(false);
    }

    public void OpenSubmitScreen(ItemData requiredItem, Action onSuccess, Action onCancelled = null)
    {
        currentItem = requiredItem;
        onSubmitSuccess = onSuccess;
        onCancel = onCancelled;

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
            deliverButton.onClick.AddListener(ConfirmDeliver);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(CloseScreen);
        }
    }

    private void ConfirmDeliver()
    {
        if (InventoryManager.Instance != null && InventoryManager.Instance.HasItem(currentItem))
        {
            InventoryManager.Instance.RemoveItem(currentItem);
            CloseScreen();
            onSubmitSuccess?.Invoke();
        }
    }

    public void CloseScreen()
    {
        if (submitPanel != null) submitPanel.SetActive(false);
        onCancel?.Invoke();
    }
}
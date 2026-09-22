using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemId;
    public string itemName;
    [TextArea(2, 4)]
    public string description;
    public Sprite itemIcon;

    [Header("Item Type")]
    public bool isWeapon = false; // Centang untuk Katana
}
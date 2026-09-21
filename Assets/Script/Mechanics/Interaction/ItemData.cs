using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName = "Yakitori";
    public Sprite itemIcon;
    [TextArea] public string description = "Dua porsi Yakitori lezat.";
}
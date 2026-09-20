using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("UI Reference")]
    public Image itemIconImage; // Drag CarriedItemIcon dari Canvas ke sini

    [Header("Inventory Status")]
    public bool hasYakitori = false;

    void Awake()
    {
        Instance = this;
        if (itemIconImage != null)
        {
            itemIconImage.gameObject.SetActive(false); // Sembunyikan icon saat awal game
        }
    }

    // Dipanggil saat mendapat Yakitori dari Pedagang
    public void GiveYakitori(Sprite yakitoriSprite)
    {
        hasYakitori = true;
        if (itemIconImage != null)
        {
            if (yakitoriSprite != null) itemIconImage.sprite = yakitoriSprite;
            itemIconImage.gameObject.SetActive(true); // Tampilkan icon makanan di UI
        }
    }

    // Dipanggil saat makanan diberikan ke Beggar
    public void RemoveYakitori()
    {
        hasYakitori = false;
        if (itemIconImage != null)
        {
            itemIconImage.gameObject.SetActive(false); // Sembunyikan icon dari UI
        }
    }
}
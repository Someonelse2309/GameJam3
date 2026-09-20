using UnityEngine;

public class PedagangTrigger : MonoBehaviour
{
    public Sprite yakitoriSprite; // Drag sprite ikon Yakitori ke sini

    [Header("Dialog Pedagang")]
    public DialogueLine[] pedagangDialogue = new DialogueLine[]
    {
        new DialogueLine { characterName = "Michelle Sato", sentence = "Give me two portions of Yakitori." },
        new DialogueLine { characterName = "Seller", sentence = "Here is the Yakitori. Be careful out there!" }
    };

    private bool isPlayerNearby = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            BuyYakitori();
        }
    }

    public void BuyYakitori()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(pedagangDialogue);
        }

        // Tambahkan Yakitori ke Inventory & tampilkan di UI
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.GiveYakitori(yakitoriSprite);
        }
    }
}
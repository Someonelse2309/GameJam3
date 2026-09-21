using UnityEngine;

public class PedagangTrigger : MonoBehaviour
{
    public ItemData yakitoriItem;

    [Header("Dialog Pedagang")]
    public DialogueLine[] pedagangDialogue = new DialogueLine[]
    {
        new DialogueLine { characterName = "Michelle Sato", sentence = "Give me two portions of Yakitori." },
        new DialogueLine { characterName = "Seller", sentence = "Here is the Yakitori. Be careful out there!" }
    };

    private bool isPlayerNearby = false;
    private bool hasGivenItem = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasGivenItem)
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

        if (InventoryManager.Instance != null && yakitoriItem != null)
        {
            InventoryManager.Instance.AddItem(yakitoriItem); // Muncul Pop-up Obtained & masuk tas
            hasGivenItem = true;
        }
    }
}
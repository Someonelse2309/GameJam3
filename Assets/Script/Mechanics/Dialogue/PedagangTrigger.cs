using UnityEngine;

public class PedagangTrigger : MonoBehaviour
{
    public ItemData yakitoriItem;
    private bool alreadyGiven = false;

    public DialogueSentence[] sellerDialogue = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Give me two portions of yakitori." },
        new DialogueSentence { speakerName = "Pedagang", sentence = "Here is the yakitori." }
    };

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !alreadyGiven)
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(sellerDialogue, () =>
                {
                    if (InventoryManager.Instance != null && yakitoriItem != null)
                    {
                        InventoryManager.Instance.AddItem(yakitoriItem);
                        alreadyGiven = true;
                    }
                });
            }
        }
    }
}
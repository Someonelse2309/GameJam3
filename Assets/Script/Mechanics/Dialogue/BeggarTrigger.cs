using UnityEngine;

public class BeggarTrigger : MonoBehaviour
{
    public ItemData yakitoriItem;

    [Header("1. Dialog Minta Makanan")]
    public DialogueLine[] initialDialogue = new DialogueLine[]
    {
        new DialogueLine { characterName = "Tanaka Koji", sentence = "Well well well, look what we have here..." },
        new DialogueLine { characterName = "Tanaka Koji", sentence = "I'm hungry... Get me some Yakitori, then we'll talk." }
    };

    [Header("2. Dialog Menolak (Belum Bawa / Batal Kasih)")]
    public DialogueLine[] noFoodDialogue = new DialogueLine[]
    {
        new DialogueLine { characterName = "Tanaka Koji", sentence = "Where's my Yakitori? I won't give info without food!" }
    };

    [Header("3. Dialog Sukses (Setelah Beri Makanan)")]
    public DialogueLine[] giveFoodDialogue = new DialogueLine[]
    {
        new DialogueLine { characterName = "Michelle Sato", sentence = "Here's your Yakitori." },
        new DialogueLine { characterName = "Tanaka Koji", sentence = "What a feast! Now start listening..." },
        new DialogueLine { characterName = "Tanaka Koji", sentence = "Aoyama is here in town. Go see Ito Shun the blacksmith for a weapon." }
    };

    private bool hasAskedForFood = false;
    private bool questCompleted = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartBeggarInteraction();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            DialogueManager.Instance?.EndDialogue();
        }
    }

    public void StartBeggarInteraction()
    {
        if (questCompleted) return;

        if (!hasAskedForFood)
        {
            if (DialogueManager.Instance == null) { Debug.LogError("BeggarTrigger: DialogueManager not found in scene"); return; }
            DialogueManager.Instance.StartDialogue(initialDialogue, () =>
            {
                hasAskedForFood = true;
                if (yakitoriItem == null) { Debug.LogError("BeggarTrigger: yakitoriItem is not assigned in the Inspector"); return; }
                if (ItemSubmitUI.Instance == null) { Debug.LogWarning("BeggarTrigger: ItemSubmitUI not found in scene — cannot open submit screen"); return; }
                ItemSubmitUI.Instance.OpenSubmitScreen(yakitoriItem, OnFoodDelivered);
            });
        }
        else
        {
            if (yakitoriItem == null) { Debug.LogError("BeggarTrigger: yakitoriItem is not assigned in the Inspector"); return; }
            if (ItemSubmitUI.Instance == null) { Debug.LogWarning("BeggarTrigger: ItemSubmitUI not found in scene — cannot open submit screen"); return; }
            ItemSubmitUI.Instance.OpenSubmitScreen(yakitoriItem, OnFoodDelivered);
        }
    }

    private void OnFoodDelivered()
    {
        questCompleted = true;
        ItemSubmitUI.Instance?.CloseScreen();
        if (DialogueManager.Instance == null) { Debug.LogError("BeggarTrigger: DialogueManager not found in scene"); return; }
        DialogueManager.Instance.StartDialogue(giveFoodDialogue, () =>
        {
            ItemSubmitUI.Instance?.CloseScreen();
        });
    }
}
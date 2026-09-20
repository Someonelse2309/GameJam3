using UnityEngine;

public class BeggarTrigger : MonoBehaviour
{
    public bool autoTriggerOnApproach = true;

    [Header("1. Dialog Awal (Minta Makanan)")]
    public DialogueLine[] initialDialogue = new DialogueLine[]
    {
        new DialogueLine { characterName = "Tanaka Koji", sentence = "Well well well, look what we have here..." },
        new DialogueLine { characterName = "Tanaka Koji", sentence = "鬼殺し (Onikoroshi)…. The ghost slayer…." },
        new DialogueLine { characterName = "Michelle Sato", sentence = "I need your help…." },
        new DialogueLine { characterName = "Tanaka Koji", sentence = "I'm hungry… Get me some Yakitori, then we'll talk." }
    };

    [Header("2. Dialog Jika BELUM Bawa Makanan")]
    public DialogueLine[] noFoodDialogue = new DialogueLine[]
    {
        new DialogueLine { characterName = "Tanaka Koji", sentence = "Where's my Yakitori? I won't tell you anything until I get my food!" }
    };

    [Header("3. Dialog Jika SUDAH Bawa Makanan")]
    public DialogueLine[] giveFoodDialogue = new DialogueLine[]
    {
        new DialogueLine { characterName = "Michelle Sato", sentence = "Here's your Yakitori." },
        new DialogueLine { characterName = "Tanaka Koji", sentence = "What a feast! Now start listening..." },
        new DialogueLine { characterName = "Tanaka Koji", sentence = "Aoyama, the one who took your child... He's here in town." },
        new DialogueLine { characterName = "Tanaka Koji", sentence = "Go to Ito Shun, the blacksmith, to get a weapon." }
    };

    private bool hasAskedForFood = false;
    private bool questCompleted = false;
    private bool isPlayerNearby = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && autoTriggerOnApproach)
        {
            isPlayerNearby = true;
            StartBeggarDialogue();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.EndDialogue();
            }
        }
    }

    public void StartBeggarDialogue()
    {
        if (DialogueManager.Instance == null) return;

        // Kondisi 3: Punya Yakitori -> Berikan makanan & lanjut cerita
        if (hasAskedForFood && InventoryManager.Instance != null && InventoryManager.Instance.hasYakitori)
        {
            InventoryManager.Instance.RemoveYakitori(); // Hapus item dari UI
            DialogueManager.Instance.StartDialogue(giveFoodDialogue);
            questCompleted = true;
        }
        // Kondisi 2: Belum bawa makanan -> Menolak
        else if (hasAskedForFood && !questCompleted)
        {
            DialogueManager.Instance.StartDialogue(noFoodDialogue);
        }
        // Kondisi 1: Pertama kali bicara -> Minta makanan
        else if (!hasAskedForFood)
        {
            DialogueManager.Instance.StartDialogue(initialDialogue);
            hasAskedForFood = true;
        }
    }
}
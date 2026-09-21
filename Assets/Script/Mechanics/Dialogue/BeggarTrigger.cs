using UnityEngine;

public class BeggarTrigger : MonoBehaviour
{
    public ItemData yakitoriItem;

    public enum QuestState { NotStarted, LookingForFood, QuestCompleted }
    public QuestState currentState = QuestState.NotStarted;

    private bool isPlayerNearby = false;

    public DialogueSentence[] dialoguePT1 = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "Well well well, look what we have here" },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "Onikoroshi.... The ghost slayer...." },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "I thought you left this life behind...." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "I need your help...." },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "Why should I? You should just go home and...." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "You owe me one, Remember Nagoya?" },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "I remember...." },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "But I'm hungry...." },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "Get me something to eat, and we'll talk...." }
    };

    public DialogueSentence[] dialoguePT3 = new DialogueSentence[]
    {
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "That was close... And I'm still hungry..." },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Here's your yakitori." },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "What a feast!" },
        new DialogueSentence { speakerName = "Michelle Sato", sentence = "Now start talking." },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "Aoyama, the one who took away your child. He's here." },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "But you need a weapon." },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "Go to Ito Shun, he's the last blacksmith in town." },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "Look for him in his house on the further side of town." },
        new DialogueSentence { speakerName = "Tanaka Koji", sentence = "And beware, the Yakuza knows you're here for revenge." }
    };

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            TriggerInteraction();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            TriggerInteraction();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }

    public void TriggerInteraction()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive) return;

        bool hasYakitori = InventoryManager.Instance != null && InventoryManager.Instance.HasItem(yakitoriItem);

        if (currentState == QuestState.NotStarted)
        {
            DialogueManager.Instance.StartDialogue(dialoguePT1, () =>
            {
                currentState = QuestState.LookingForFood;
            });
        }
        else if (currentState == QuestState.LookingForFood && hasYakitori)
        {
            // Buka panel submit/serahkan makanan ke beggar
            ItemSubmitUI.Instance.OpenSubmitScreen(yakitoriItem, () =>
            {
                currentState = QuestState.QuestCompleted;
                DialogueManager.Instance.StartDialogue(dialoguePT3);
            });
        }
        else if (currentState == QuestState.LookingForFood && !hasYakitori)
        {
            DialogueSentence[] remindDialog = new DialogueSentence[]
            {
                new DialogueSentence { speakerName = "Tanaka Koji", sentence = "I'm starving... Go get me that Yakitori from the vendor!" }
            };
            DialogueManager.Instance.StartDialogue(remindDialog);
        }
        else if (currentState == QuestState.QuestCompleted)
        {
            DialogueSentence[] doneDialog = new DialogueSentence[]
            {
                new DialogueSentence { speakerName = "Tanaka Koji", sentence = "Go see Ito Shun. He has what you need." }
            };
            DialogueManager.Instance.StartDialogue(doneDialog);
        }
    }
}
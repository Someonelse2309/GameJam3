using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Game/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public string text;
        public AudioClip voiceLine; // Optional: voice clip for this line
        public float delayAfter = 0.5f; // Delay after this line finishes
    }

    public DialogueLine[] lines;
}

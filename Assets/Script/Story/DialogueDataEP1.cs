using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Game/Dialogue Data")]
public class DialogueDataEP1 : ScriptableObject
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public string text;
        public AudioClip voiceLine; // Optional: voice clip for this line
        public float delayAfter = 0.5f; // Delay after this line finishes
        public Sprite characterImage; // Avatar for this line

        [Header("Effects")]
        public AudioClip soundEffect; // SFX untuk line ini (misal: gunshot)
        public bool triggerFadeToBlack = false; // Fade to black setelah line ini
        public bool triggerScreenShake = false; // Screen shake saat line ini
    }

    public DialogueLine[] lines;
}

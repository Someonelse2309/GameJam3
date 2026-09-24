using UnityEngine;

[System.Serializable]
public class DialogueSentence
{
    public string speakerName;
    [TextArea(2, 5)]
    public string sentence;
}
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string characterName;     // Nama pembicara
    public Sprite characterPortrait; // Foto profil pembicara
    [TextArea(2, 5)]
    public string sentence;          // Teks dialog
}
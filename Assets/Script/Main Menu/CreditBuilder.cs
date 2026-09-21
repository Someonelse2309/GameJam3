using System.Text;
using UnityEngine;
using TMPro;
using Newtonsoft.Json; // Required for dictionary parsing
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CreditsBuilder : MonoBehaviour
{

    [Tooltip("Drag your saved .json file here from the Project window")]
    public TextAsset creditsJsonFile;
    
    [Tooltip("The TextMeshPro component that will display the text")]
    public TextMeshProUGUI creditsText;

    [Tooltip("The back button to Main Menu")]
    public Button backButton;

    private void Start()
    {
        if (creditsJsonFile == null || creditsText == null)
        {
            Debug.LogError("CreditsBuilder: Missing references. Please assign the JSON and Text components.");
            return;
        }

        BuildCreditsUI();
        AudioManager.instance.PlayMusic(0);
    }

    private void BuildCreditsUI()
    {
        // 1. Deserialize the JSON into our C# dictionaries
        CreditsDatabase database = JsonConvert.DeserializeObject<CreditsDatabase>(creditsJsonFile.text);

        // 2. Use StringBuilder for memory-efficient text generation
        StringBuilder sb = new StringBuilder();

        // 3. Format each section
        FormatSection(sb, "AUDIO & MUSIC", database.Music);
        FormatSection(sb, "SOUND EFFECTS", database.SFX);
        FormatSection(sb, "VISUAL ASSETS", database.Visual);

        // 4. Inject the final generated text into the UI
        creditsText.text = sb.ToString();
    }

    private void FormatSection(StringBuilder sb, string header, Dictionary<string, CreditEntry> category)
    {
        if (category == null || category.Count == 0) return;

        // Add spacing and a bold header
        sb.AppendLine($"<size=150%><b>{header}</b></size>");
        sb.AppendLine();

        // Loop through each entry in the dictionary
        foreach (var kvp in category)
        {
            CreditEntry entry = kvp.Value;
            
            // Format: "Title" by Creator
            sb.AppendLine($"<b>\"{entry.Title}\"</b>");
            sb.AppendLine($"by {entry.Creator}");
            
            // Optional: You can make the link smaller and slightly transparent
            sb.AppendLine($"<size=70%><alpha=#AA>{entry.Link}</size>");
            sb.AppendLine();
        }
        
        sb.AppendLine(); // Extra space between major sections
    }
    
    public void BackToMainMenu()
    {
        // The '?' safely aborts the call if AudioManager is missing
        AudioManager.instance?.StopMusic();

        if (GameState.getIsCompleteEP1()) 
        {
            SceneController.instance?.LoadSceneByName("MainMenuEP1", true);
        } 
        else 
        {
            SceneController.instance?.LoadSceneByName("MainMenuEP2", true);
        }
    }
}
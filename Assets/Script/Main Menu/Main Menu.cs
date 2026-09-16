using UnityEngine;

public class MainMenu : MonoBehaviour
{

    // Button
    // public Button playButton;
    // public Button creditButton;
    // public Button quitGameButton;


    
    void Awake()
    {
        Resolution[] resolutions = Screen.resolutions;
        if (resolutions.Length > 0)
        {
            // The last resolution in the array is typically the native/highest resolution
            Resolution maxRes = resolutions[resolutions.Length - 1];
            Screen.SetResolution(maxRes.width, maxRes.height, FullScreenMode.FullScreenWindow);
        }
        Screen.fullScreen = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() 
    {
        AudioManager.instance.PlayMusic(0);  
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

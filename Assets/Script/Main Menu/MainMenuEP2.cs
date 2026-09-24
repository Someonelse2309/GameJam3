using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuEP2 : MonoBehaviour
{
    // Button
    public Button playButton;
    public Button creditButton;

    void Awake()
    {
        if (GameState.getIsCompleteEP1()) 
        {
            SceneController.instance.LoadSceneByName("MainMenuEP1", true);
            return;
        }

        if (playButton != null)
        {
            playButton.transform.SetSiblingIndex(10);
        }

        if (creditButton != null)
        {
            creditButton.transform.SetSiblingIndex(10);
        }
    }   

    void Start() 
    {
        AudioManager.instance.PlayMusic(0);  
    }

    public void PlayGame()
    {
        AudioManager.instance.StopMusic(); 
        SceneController.instance.LoadSceneByName("StoryOnboardingEP2", true);
    }

    public void Credit()
    {
        SceneController.instance.LoadSceneByName("Credit", true);
    }

}

using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Collections;

public class SceneController : MonoBehaviour
{
    public static SceneController instance;

    private void Awake()
    {
        if (instance == null) {
            instance = this;
            Screen.fullScreen = true;
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene("Main Menu Scene"); 
    }


    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }
    
    public void LoadSceneByName(string sceneName, bool isFullscreen = true)
    {
        Screen.fullScreen = isFullscreen;
        SceneManager.LoadScene(sceneName);
    }
}

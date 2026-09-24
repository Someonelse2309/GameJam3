using UnityEngine;
using TMPro;
using System.Collections;

public class StoryPostEP1 : MonoBehaviour
{
    public TextMeshProUGUI subtitleText;

    public int index = 0;

    private void Start()
    {
        StartCoroutine(PlayStatement());
    }

    private IEnumerator PlayStatement()
    {
        switch (index)
        {
            case 0:
                subtitleText.text = "";
                AudioManager.instance.PlayMusic(0, 1f);
                AudioManager.instance.PlayLoopingSFX(0, 0.5f);
                yield return new WaitForSeconds(2f);

                break;

            case 1:
                subtitleText.text = "<color=#FFFFFF>[COMPACT DISC #2 — </color><color=#bd3131>STOLEN</color>]";

                yield return new WaitForSeconds(2f);
                break;

            case 2:
                subtitleText.text = "<color=#FFFFFF>[MICHELLE SATO — </color><color=#bd3131>ASSASSIN RETIRED</color>]";

                yield return new WaitForSeconds(2f);
                break;

            case 3:
                subtitleText.text = "<color=#FFFFFF>[NOW... SHE HAS NOTHING LEFT TO LIVE FOR]</color>";

                yield return new WaitForSeconds(2f);
                break;

            case 4:
                subtitleText.text = "<color=#FFFFFF>[EXCEPT ONE THING]</color>";

                yield return new WaitForSeconds(2f);
                break;
        

            case 5:
                subtitleText.text = "<color=#bd3131>[REVENGE]</color>";

                yield return new WaitForSeconds(2f);
                break;

            case 6:
                subtitleText.text = 
                "<color=#FFFFFF>[ONIKOROSHI]</color>" +
                "\n<color=#bd3131>[THE MOTHER'S WRATH]</color>";

                // yield return new WaitForSeconds(2f);
                AudioManager.instance.StopLoopingSFX();
                AudioManager.instance.PlaySFX(1);

                yield return new WaitUntil(
                    () => !AudioManager.instance.isSFXPlaying()
                );
                break;

            
            default:
                // AudioManager.instance.StopMusic();
                SceneController.instance.LoadSceneByName("MainMenuEP2", true);
                yield break;
        }

        index++;

        StartCoroutine(PlayStatement());
    }
}
using UnityEngine;
using TMPro;
using System.Collections;

public class StoryEP1 : MonoBehaviour
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

                yield return new WaitForSeconds(2f);
                break;

            case 1:
                subtitleText.text =
                    "<color=#4DA6FF>Michelle Sato:</color> " +
                    "<color=#FFFFFF>I left that life behind for him.</color>";

                AudioManager.instance.PlayVoice(0);

                yield return new WaitUntil(
                    () => !AudioManager.instance.isVoicePlaying()
                );

                yield return new WaitForSeconds(1f);
                break;

            case 2:

                yield return new WaitForSeconds(1f);

                subtitleText.text =
                    "<color=#4DA6FF>Michelle Sato:</color> " +
                    "<color=#FFFFFF>He's everything for me.</color>";

                AudioManager.instance.PlayVoice(1);

                yield return new WaitUntil(
                    () => !AudioManager.instance.isVoicePlaying()
                );

                yield return new WaitForSeconds(1f);
                break;

            case 3:
                subtitleText.text =
                    "<color=#4DA6FF>Michelle Sato:</color> " +
                    "<color=#FFFFFF>I kept all of the memories about him in the CDs.</color>";

                AudioManager.instance.PlayVoice(2);

                yield return new WaitUntil(
                    () => !AudioManager.instance.isVoicePlaying()
                );

                yield return new WaitForSeconds(1f);
                break;

            case 4:
                subtitleText.text =
                    "<color=#4DA6FF>Michelle Sato:</color> " +
                    "<color=#FFFFFF>I'm looking forward to spend time with him today.</color>";

                AudioManager.instance.PlayVoice(3);

                yield return new WaitUntil(
                    () => !AudioManager.instance.isVoicePlaying()
                );

                yield return new WaitForSeconds(1f);
                break;

            case 5:
                subtitleText.text =
                    "<color=#4DA6FF>Michelle Sato:</color> " +
                    "<color=#FFFFFF>My one and only son Ryu.</color>";

                AudioManager.instance.PlayVoice(4);

                yield return new WaitUntil(
                    () => !AudioManager.instance.isVoicePlaying()
                );

                yield return new WaitForSeconds(1f);
                break;

            case 6:
                subtitleText.text =
                    "<color=#4DA6FF>Michelle Sato:</color> " +
                    "<color=#FFFFFF>But I never expect the day to end this way.</color>";

                AudioManager.instance.PlayVoice(5);

                yield return new WaitUntil(
                    () => !AudioManager.instance.isVoicePlaying()
                );

                yield return new WaitForSeconds(1f);
                break;

            default:
                yield return new WaitForSeconds(1f);
                SceneController.instance.LoadSceneByName("InGameEP1", true);
                break;
        }

        index++;

        StartCoroutine(PlayStatement());
    }
}
using UnityEngine;
using TMPro;
using System.Collections;

public class StoryEP2 : MonoBehaviour
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
                subtitleText.text =
                    "<color=#4DA6FF>Michelle Sato:</color> " +
                    "<color=#FFFFFF>All the contract and all of the jobs</color>";

                AudioManager.instance.PlayVoice(1);

                yield return new WaitUntil(
                    () => !AudioManager.instance.isVoicePlaying()
                );

                yield return new WaitForSeconds(1f);
                break;

            case 3:
                AudioManager.instance.PlayMusic(0);

                yield return new WaitForSeconds(1f);

                subtitleText.text =
                    "<color=#FFFFFF>[Compact disc spinning]</color>";

                yield return new WaitForSeconds(6f);
                break;

            case 4:
                subtitleText.text =
                    "<color=#4DA6FF>Ryu Sato:</color> " +
                    "<color=#FFFFFF>Look, Mom! I brought you these seashells!</color>";

                AudioManager.instance.PlayVoice(2);

                yield return new WaitUntil(
                    () => !AudioManager.instance.isVoicePlaying()
                );

                yield return new WaitForSeconds(1f);
                break;

            case 5:
                subtitleText.text =
                    "<color=#4DA6FF>Ryu Sato:</color> " +
                    "<color=#FFFFFF>I found them near the shore. It's for you</color>";

                AudioManager.instance.PlayVoice(3);

                yield return new WaitUntil(
                    () => !AudioManager.instance.isVoicePlaying()
                );

                yield return new WaitForSeconds(1f);

                AudioManager.instance.StopMusic();
                break;

            case 6:
                subtitleText.text =
                    "<color=#4DA6FF>Michelle Sato:</color> " +
                    "<color=#FFFFFF>The CD is the only memory I have left of him.</color>";

                AudioManager.instance.PlayVoice(4);

                yield return new WaitUntil(
                    () => !AudioManager.instance.isVoicePlaying()
                );

                yield return new WaitForSeconds(1f);
                break;

            case 7:
                subtitleText.text =
                    "<color=#4DA6FF>Michelle Sato:</color> " +
                    "<color=#FFFFFF>After he left this world behind...</color>";

                AudioManager.instance.PlayVoice(5);

                yield return new WaitUntil(
                    () => !AudioManager.instance.isVoicePlaying()
                );

                yield return new WaitForSeconds(1f);
                break;

            case 8:
                subtitleText.text =
                    "<color=#FF7043>Aoyama:</color> " +
                    "<color=#FFFFFF>Say goodbye to your mom.</color>";

                AudioManager.instance.PlayVoice(6);

                yield return new WaitUntil(
                    () => !AudioManager.instance.isVoicePlaying()
                );

                // yield return new WaitForSeconds(1f);
                break;

            case 9:
                subtitleText.text =
                    "<color=#FFFFFF>[Gunshot]</color>";

                AudioManager.instance.PlaySFX(0);

                yield return new WaitForSeconds(0.5f);
                break;

            case 10:
                subtitleText.text =
                    "<color=#4DA6FF>Michelle Sato:</color> " +
                    "<color=#FFFFFF>Ryu... Ryu... stay with me... please...</color>";

                AudioManager.instance.PlayVoice(7);

                yield return new WaitUntil(
                    () => !AudioManager.instance.isVoicePlaying()
                );

                // yield return new WaitForSeconds(1f);
                break;

            case 11:
                subtitleText.text =
                    "<color=#4DA6FF>Michelle Sato:</color> " +
                    "<color=#FFFFFF>Now the memory is gone.</color>";

                AudioManager.instance.PlayVoice(8);

                yield return new WaitUntil(
                    () => !AudioManager.instance.isVoicePlaying()
                );

                yield return new WaitForSeconds(1f);
                break;

            case 12:
                subtitleText.text =
                    "<color=#4DA6FF>Michelle Sato:</color> " +
                    "<color=#FFFFFF>Without the CD...</color>";

                AudioManager.instance.PlayVoice(9);

                yield return new WaitUntil(
                    () => !AudioManager.instance.isVoicePlaying()
                );

                // yield return new WaitForSeconds(0.5f);
                break;

            case 13:
                subtitleText.text =
                    "<color=#4DA6FF>Michelle Sato:</color> " +
                    "<color=#FFFFFF>I can't remember his face.</color>";

                AudioManager.instance.PlayVoice(10);

                yield return new WaitUntil(
                    () => !AudioManager.instance.isVoicePlaying()
                );

                yield return new WaitForSeconds(1f);
                break;

            case 14:
                subtitleText.text =
                    "<color=#4DA6FF>Michelle Sato:</color> " +
                    "<color=#FFFFFF>There is nothing left of him.</color>";

                AudioManager.instance.PlayVoice(11);

                yield return new WaitUntil(
                    () => !AudioManager.instance.isVoicePlaying()
                );

                yield return new WaitForSeconds(1f);
                break;

            case 15:
                subtitleText.text =
                    "<color=#4DA6FF>Michelle Sato:</color> " +
                    "<color=#FFFFFF>It's time for revenge.</color>";

                AudioManager.instance.PlayVoice(12);

                yield return new WaitUntil(
                    () => !AudioManager.instance.isVoicePlaying()
                );

                yield return new WaitForSeconds(1f);
                break;

            default:
                yield return new WaitForSeconds(1f);
                SceneController.instance.LoadSceneByName("In game", true);
                yield break;
        }

        index++;

        StartCoroutine(PlayStatement());
    }
}
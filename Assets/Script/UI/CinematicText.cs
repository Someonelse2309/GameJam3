using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CinematicText : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public TextMeshProUGUI[] textLines;

    [Header("Settings")]
    public float typingSpeed = 0.05f;
    public float lineDelay = 1.5f;

    [Header("After Text")]
    public bool fadeToBlackAfter = true;

    [System.NonSerialized]
    public System.Action onComplete;

    private bool isPlaying = false;
    private Image panelImage;
    private CanvasGroup canvasGroup;
    private string[] savedTexts;

    void Start()
    {
        LoadTextFromTMP();

        if (panel != null)
        {
            panelImage = panel.GetComponent<Image>();
            if (panelImage != null)
            {
                Color c = panelImage.color;
                c.a = 0f;
                panelImage.color = c;
            }

            canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = panel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            panel.SetActive(false);
        }
    }

    private void LoadTextFromTMP()
    {
        if (textLines == null || textLines.Length == 0) return;

        savedTexts = new string[textLines.Length];
        for (int i = 0; i < textLines.Length; i++)
        {
            if (textLines[i] != null)
            {
                savedTexts[i] = textLines[i].text;
                textLines[i].text = "";
            }
        }
    }

    public void Play()
    {
        if (isPlaying)
        {
            isPlaying = false;
        }

        if (savedTexts == null || savedTexts.Length == 0)
        {
            LoadTextFromTMP();
        }

        if (panel != null && !panel.activeInHierarchy)
        {
            panel.SetActive(true);
        }

        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        isPlaying = true;

        if (textLines != null)
        {
            for (int i = 0; i < textLines.Length; i++)
            {
                if (textLines[i] != null)
                    textLines[i].text = "";
            }
        }

        if (panel != null && panelImage != null)
        {
            Color c = panelImage.color;
            c.a = 1f;
            c.r = 0f;
            c.g = 0f;
            c.b = 0f;
            panelImage.color = c;

            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
        }

        yield return new WaitForSeconds(0.5f);

        if (savedTexts != null)
        {
            for (int i = 0; i < savedTexts.Length; i++)
            {
                string fullText = savedTexts[i];

                if (textLines[i] != null)
                    textLines[i].text = "";

                foreach (char c in fullText)
                {
                    if (textLines[i] != null)
                        textLines[i].text += c;
                    yield return new WaitForSeconds(typingSpeed);
                }

                if (i < savedTexts.Length - 1)
                    yield return new WaitForSeconds(lineDelay);
            }
        }

        yield return new WaitForSeconds(2f);

        if (panel != null)
            panel.SetActive(false);

        isPlaying = false;
        onComplete?.Invoke();
        
        GameState.toggleIsCompleteEP1();
        SceneController.instance.LoadSceneByName("MainMenuEP2", true);
    }
}

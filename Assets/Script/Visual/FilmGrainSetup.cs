using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class FilmGrainSetup : MonoBehaviour
{
    [Header("References - Assign di Inspector")]
    public VideoPlayer videoPlayer;
    public RawImage rawImage;
    public VideoClip videoClip;

    [Header("Settings")]
    public float overlayAlpha = 0.3f;

    private RenderTexture renderTexture;

    void Start()
    {
        // Auto-detect screen size
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

        // Create RenderTexture sesuai ukuran layar
        renderTexture = new RenderTexture(screenWidth, screenHeight, 24);
        renderTexture.name = "FilmGrainRT_" + screenWidth + "x" + screenHeight;

        Debug.Log("FilmGrainRT created: " + screenWidth + "x" + screenHeight);

        // Setup VideoPlayer
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null)
        {
            videoPlayer.clip = videoClip;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = renderTexture;
            videoPlayer.Play();
        }

        // Setup RawImage
        if (rawImage != null)
        {
            rawImage.texture = renderTexture;
            rawImage.color = new Color(1, 1, 1, overlayAlpha);
        }
    }

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
        }
    }
}

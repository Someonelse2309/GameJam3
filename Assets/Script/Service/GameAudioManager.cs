using System.Collections;
using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }

    [Header("Volume Controls (0.0 to 1.0)")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("BGM Tracks")]
    public AudioClip explorationBGM;
    public AudioClip combatBGM;

    [Header("Combat SFX")]
    public AudioClip punchSFX;
    public AudioClip swordSFX;
    public AudioClip hitImpactSFX;

    [Header("Movement SFX")]
    public AudioClip footstepSFX;
    public float footstepInterval = 0.32f;

    [Header("UI SFX (Dibutuhkan UISoundTrigger)")]
    public AudioClip uiClickDefault;
    public AudioClip dialogueAdvanceSFX;
    public AudioClip questTrackerSFX;
    public AudioClip inventoryToggleSFX;

    private AudioSource bgmSource;
    private AudioSource sfxSource;
    private AudioSource footstepSource;

    private Coroutine bgmFadeCoroutine;
    private float footstepTimer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Kunci 60 FPS dan matikan vSync
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            InitializeAudioChannels();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioChannels()
    {
        foreach (var src in GetComponents<AudioSource>())
        {
            src.playOnAwake = false;
            src.Stop();
            src.enabled = false;
        }

        GameObject bgmObj = new GameObject("Channel_BGM");
        bgmObj.transform.SetParent(transform);
        bgmSource = bgmObj.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.spatialBlend = 0f;
        bgmSource.enabled = true;
        Debug.Log("[GameAudio] BGM channel ready, clip: " + (explorationBGM != null ? explorationBGM.name : "null"));

        GameObject sfxObj = new GameObject("Channel_SFX");
        sfxObj.transform.SetParent(transform);
        sfxSource = sfxObj.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.enabled = true;
        Debug.Log("[GameAudio] SFX channel ready");

        GameObject footstepObj = new GameObject("Channel_Footstep");
        footstepObj.transform.SetParent(transform);
        footstepSource = footstepObj.AddComponent<AudioSource>();
        footstepSource.loop = false;
        footstepSource.playOnAwake = false;
        footstepSource.spatialBlend = 0f;
        footstepSource.enabled = true;
        Debug.Log("[GameAudio] Footstep channel ready");

        ApplyVolumes();
    }

    private void Start()
    {
        if (explorationBGM != null)
        {
            PlayExplorationBGM();
        }
    }

    private void OnValidate()
    {
        ApplyVolumes();
    }

    private void Update()
    {
        ApplyVolumes();
    }

    public void ApplyVolumes()
    {
        if (bgmSource != null) bgmSource.volume = bgmVolume * masterVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume * masterVolume;
        if (footstepSource != null) footstepSource.volume = sfxVolume * masterVolume * 0.4f;
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }

    // ==================== BGM CONTROLS ====================

    public void PlayExplorationBGM(float fadeDuration = 1f) => SwitchBGM(explorationBGM, fadeDuration);
    public void PlayExplorationBGM(AudioClip clip, float fadeDuration = 1f)
    {
        if (clip != null) explorationBGM = clip;
        SwitchBGM(explorationBGM, fadeDuration);
    }

    public void PlayCombatBGM(float fadeDuration = 0.5f) => SwitchBGM(combatBGM, fadeDuration);
    public void PlayCombatBGM(AudioClip clip, float fadeDuration = 0.5f)
    {
        if (clip != null) combatBGM = clip;
        SwitchBGM(combatBGM, fadeDuration);
    }

    public void SwitchBGM(AudioClip targetClip, float duration = 1f)
    {
        if (targetClip == null || bgmSource == null) return;
        if (bgmSource.clip == targetClip && bgmSource.isPlaying) return;

        if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
        bgmFadeCoroutine = StartCoroutine(CrossfadeRoutine(targetClip, duration));
    }

    private IEnumerator CrossfadeRoutine(AudioClip targetClip, float duration)
    {
        float targetVol = bgmVolume * masterVolume;

        if (bgmSource.isPlaying)
        {
            float startVol = bgmSource.volume;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                if (bgmSource == null) yield break;
                bgmSource.volume = Mathf.Lerp(startVol, 0f, t / duration);
                yield return null;
            }
        }

        bgmSource.clip = targetClip;
        bgmSource.Play();

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            if (bgmSource == null) yield break;
            bgmSource.volume = Mathf.Lerp(0f, targetVol, t / duration);
            yield return null;
        }

        if (bgmSource != null) bgmSource.volume = targetVol;
    }

    // ==================== SFX CONTROLS ====================

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
        }
    }

    public void PlayPunchSFX() => PlaySFX(punchSFX);
    public void PlaySwordSFX() => PlaySFX(swordSFX);
    public void PlayHitImpactSFX() => PlaySFX(hitImpactSFX);

    public void PlayClickSFX() => PlaySFX(uiClickDefault);
    public void PlayQuestTrackerSFX() => PlaySFX(questTrackerSFX);
    public void PlayInventoryToggleSFX() => PlaySFX(inventoryToggleSFX);
    public void PlayDialogueNextSFX() => PlaySFX(dialogueAdvanceSFX);

    // ==================== FOOTSTEP CONTROLS ====================

    public void ProcessFootstep(bool isMoving)
    {
        if (!isMoving || footstepSFX == null || footstepSource == null)
        {
            footstepTimer = 0f;
            return;
        }

        footstepTimer += Time.deltaTime;
        if (footstepTimer >= footstepInterval)
        {
            footstepTimer = 0f;
            footstepSource.pitch = UnityEngine.Random.Range(0.85f, 1.15f);
            footstepSource.PlayOneShot(footstepSFX, sfxVolume * masterVolume * 0.4f);
        }
    }
    public void PlayFootstep()
{
    if (footstepSFX != null && footstepSource != null)
    {
        footstepSource.PlayOneShot(footstepSFX, sfxVolume * masterVolume);
    }
}
}
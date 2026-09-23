using System.Collections;
using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }

    [Header("Audio Sources (Assign dari GameObject ini)")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource footstepSource;

    [Header("Volume Controls (0.0 to 1.0)")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 0.8f;
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
    public float footstepInterval = 0.35f;

    [Header("UI SFX")]
    public AudioClip uiClickDefault;
    public AudioClip dialogueAdvanceSFX;
    public AudioClip questTrackerSFX;
    public AudioClip inventoryToggleSFX;

    private Coroutine bgmFadeCoroutine;
    private float footstepTimer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ValidateAudioSources();
            LoadVolumeSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayExplorationBGM();
    }

    private void Update()
    {
        // Update volume secara real-time saat slider digeser di Inspector/UI
        ApplyVolumes();
    }

    private void ValidateAudioSources()
    {
        // Otomatis pasang AudioSource jika slot kosong di Inspector
        AudioSource[] sources = GetComponents<AudioSource>();
        
        if (bgmSource == null)
        {
            bgmSource = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        if (footstepSource == null)
        {
            footstepSource = sources.Length > 2 ? sources[2] : gameObject.AddComponent<AudioSource>();
            footstepSource.loop = false;
            footstepSource.playOnAwake = false;
        }
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("MasterVol", masterVolume);
        ApplyVolumes();
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("BGMVol", bgmVolume);
        ApplyVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SFXVol", sfxVolume);
        ApplyVolumes();
    }

    private void ApplyVolumes()
    {
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume * masterVolume;
        }
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume * masterVolume;
        }
        if (footstepSource != null)
        {
            footstepSource.volume = 0.4f * sfxVolume * masterVolume;
        }
    }

    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVol", 1f);
        bgmVolume = PlayerPrefs.GetFloat("BGMVol", 0.8f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVol", 1f);
        ApplyVolumes();
    }

    public void PlayExplorationBGM(float fadeDuration = 1f)
    {
        SwitchBGM(explorationBGM, fadeDuration);
    }

    public void PlayCombatBGM(float fadeDuration = 0.5f)
    {
        SwitchBGM(combatBGM, fadeDuration);
    }

    public void SwitchBGM(AudioClip newClip, float fadeDuration = 1f)
    {
        if (newClip == null || bgmSource.clip == newClip) return;

        if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
        bgmFadeCoroutine = StartCoroutine(CrossfadeBGM(newClip, fadeDuration));
    }

    private IEnumerator CrossfadeBGM(AudioClip targetClip, float duration)
    {
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

        float finalTargetVol = bgmVolume * masterVolume;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            if (bgmSource == null) yield break;
            bgmSource.volume = Mathf.Lerp(0f, finalTargetVol, t / duration);
            yield return null;
        }

        bgmSource.volume = finalTargetVol;
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volumeScale * sfxVolume * masterVolume);
        }
    }

    public void PlayPunchSFX() => PlaySFX(punchSFX);
    public void PlaySwordSFX() => PlaySFX(swordSFX);
    public void PlayHitImpactSFX() => PlaySFX(hitImpactSFX);
    public void PlayDialogueNextSFX() => PlaySFX(dialogueAdvanceSFX);
    public void PlayQuestTrackerSFX() => PlaySFX(questTrackerSFX);
    public void PlayInventoryToggleSFX() => PlaySFX(inventoryToggleSFX);

    public void ProcessFootstep(bool isMoving)
    {
        if (!isMoving || footstepSFX == null) return;

        footstepTimer += Time.deltaTime;
        if (footstepTimer >= footstepInterval)
        {
            footstepTimer = 0f;
            footstepSource.pitch = Random.Range(0.9f, 1.1f);
            footstepSource.PlayOneShot(footstepSFX, 0.4f * sfxVolume * masterVolume);
        }
    }
}
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource loopingSfxSource;
    public AudioSource voiceSource;

    public AudioClip[] musicLibrary;
    public AudioClip[] sfxLibrary;
    public AudioClip[] voiceLibrary;

    public static AudioManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject);
            EnsureAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void EnsureAudioSources()
    {
        AudioSource[] sources = GetComponents<AudioSource>();

        while (sources.Length < 4)
        {
            gameObject.AddComponent<AudioSource>();
            sources = GetComponents<AudioSource>();
        }

        musicSource = sources[0];
        sfxSource = sources[1];
        loopingSfxSource = sources[2];
        voiceSource = sources[3];

        musicSource.playOnAwake = false;
        musicSource.loop = true;

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;

        loopingSfxSource.playOnAwake = false;
        loopingSfxSource.loop = true;

        voiceSource.playOnAwake = false;
        voiceSource.loop = false;
    }

    public void PlayMusic(int index, float volume = 1f)
    {
        if (index >= 0 && index < musicLibrary.Length && musicLibrary[index] != null)
        {
            if (musicSource.clip == musicLibrary[index] && musicSource.isPlaying)
            {
                musicSource.volume = volume;
                return;
            }

            musicSource.clip = musicLibrary[index];
            musicSource.loop = true;
            musicSource.volume = volume;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
        musicSource.clip = null;
    }

    public void PlaySFX(int index, float volume = 1f)
    {
        if (index >= 0 && index < sfxLibrary.Length && sfxLibrary[index] != null)
        {
            sfxSource.PlayOneShot(sfxLibrary[index], volume);
        }
        else
        {
            Debug.LogWarning("Audio Manager: SFX index " + index + " is out of bounds!");
        }
    }

    public bool isSFXPlaying()
    {
        return sfxSource.isPlaying;
    }

    public void PlayVoice(int index, float volume = 1f)
    {
        if (index >= 0 && index < voiceLibrary.Length && voiceLibrary[index] != null)
        {
            voiceSource.PlayOneShot(voiceLibrary[index], volume);
        }
        else
        {
            Debug.LogWarning("Audio Manager: Voice index " + index + " is out of bounds!");
        }
    }

    public bool isVoicePlaying()
    {
        return voiceSource.isPlaying;
    }

    public void PlayLoopingSFX(int index, float volume = 1f)
    {
        if (index >= 0 && index < sfxLibrary.Length && sfxLibrary[index] != null)
        {
            loopingSfxSource.clip = sfxLibrary[index];
            loopingSfxSource.loop = true; 
            
            loopingSfxSource.volume = volume;
            
            loopingSfxSource.Play();
        }
    }

    public void StopLoopingSFX()
    {
        loopingSfxSource.Stop();
    }

    public float GetSFXLength(int index)
    {
        if (index >= 0 && index < sfxLibrary.Length && sfxLibrary[index] != null)
        {
            return sfxLibrary[index].length; 
        }
        return 0f; 
    }

    
    public void PlaySFXPanned(int index, float volume = 1f, float pan = 0f)
    {
        if (index >= 0 && index < sfxLibrary.Length && sfxLibrary[index] != null)
        {
            AudioSource tempSource = gameObject.AddComponent<AudioSource>();
            tempSource.clip = sfxLibrary[index];
            tempSource.volume = volume;
            tempSource.panStereo = Mathf.Clamp(pan, -1f, 1f);
            tempSource.spatialBlend = 0f;
            tempSource.playOnAwake = false;
            tempSource.Play();

            Destroy(tempSource, sfxLibrary[index].length + 0.1f);
        }
    }
}

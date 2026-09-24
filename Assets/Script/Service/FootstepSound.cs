using UnityEngine;

public class FootstepSound : MonoBehaviour
{
    public float footstepInterval = 0.5f;
    private Vector3 lastPosition;
    private float footstepTimer;
    private AudioSource footstepSource;

    void Start()
    {
        lastPosition = transform.position;
        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.playOnAwake = false;
        footstepSource.loop = false;
        footstepSource.spatialBlend = 0f;

        if (AudioManager.instance != null && AudioManager.instance.sfxLibrary.Length > 0)
            footstepSource.clip = AudioManager.instance.sfxLibrary[0];
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, lastPosition);

        if (distance > 0.05f)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f && !footstepSource.isPlaying)
            {
                footstepSource.Play();
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            if (footstepSource.isPlaying)
                footstepSource.Stop();
            footstepTimer = 0f;
        }

        lastPosition = transform.position;
    }
}

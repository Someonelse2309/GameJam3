using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    [Header("Settings")]
    public float shakeDuration = 0.5f;
    public float shakeIntensity = 1f;
    public Camera targetCamera;

    private float currentShakeTime = 0f;
    private Vector3 originalPosition;
    private bool isShaking = false;

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera != null)
            originalPosition = targetCamera.transform.position;

        // Matiin shake di awal
        enabled = false;
    }

    void Update()
    {
        if (!isShaking) return;

        if (currentShakeTime > 0)
        {
            currentShakeTime -= Time.deltaTime;

            if (targetCamera != null)
            {
                float x = Random.Range(-1f, 1f) * shakeIntensity;
                float y = Random.Range(-1f, 1f) * shakeIntensity;

                targetCamera.transform.position = originalPosition + new Vector3(x, y, 0);
            }
        }
        else
        {
            // Stop shake
            if (targetCamera != null)
                targetCamera.transform.position = originalPosition;

            isShaking = false;
            enabled = false;
        }
    }

    public void TriggerShake(float duration = 0.5f, float intensity = 1f)
    {
        shakeDuration = duration;
        shakeIntensity = intensity;
        currentShakeTime = duration;

        if (targetCamera != null)
            originalPosition = targetCamera.transform.position;

        isShaking = true;
        enabled = true;
    }

    public void OnGunshot()
    {
        TriggerShake(0.8f, 1.5f);
    }

    public void StopShake()
    {
        isShaking = false;
        enabled = false;

        if (targetCamera != null)
            targetCamera.transform.position = originalPosition;
    }
}

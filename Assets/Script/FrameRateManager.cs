using UnityEngine;

public static class FrameRateBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeFrameRate()
    {
        // 0 = Matikan VSync agar targetFrameRate dihormati
        QualitySettings.vSyncCount = 0;

        // Buka lock FPS ke 60 (atau sesuaikan dengan layar HP)
        Application.targetFrameRate = 60;

        // Cegah layar redup otomatis
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}
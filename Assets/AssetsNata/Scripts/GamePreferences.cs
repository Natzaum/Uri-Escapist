using UnityEngine;

public static class GamePreferences
{
    public static float Volume => Mathf.Clamp01(PlayerPrefs.GetFloat("URI.Volume", 0.8f));
    public static float Sensitivity => Mathf.Clamp(PlayerPrefs.GetFloat("URI.Sensitivity", 1f), 0.2f, 3f);

    public static void SetVolume(float value)
    {
        PlayerPrefs.SetFloat("URI.Volume", Mathf.Clamp01(value));
        AudioListener.volume = Volume;
    }

    public static void SetSensitivity(float value)
    {
        PlayerPrefs.SetFloat("URI.Sensitivity", Mathf.Clamp(value, 0.2f, 3f));
    }

    public static void Save() => PlayerPrefs.Save();
}

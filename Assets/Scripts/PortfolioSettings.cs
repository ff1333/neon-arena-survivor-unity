using System;
using UnityEngine;

public static class PortfolioSettings
{
    private const string Prefix = "Portfolio.Settings.";
    public static event Action Changed;
    public static bool IsOpen { get; internal set; }
    public static bool Chinese => PlayerPrefs.GetInt(Prefix + "English", 0) == 0;
    public static bool SoundEnabled => PlayerPrefs.GetInt(Prefix + "Sound", 1) != 0;
    public static float Master => ReadVolume("Master", .8f);
    public static float Music => ReadVolume("Music", .25f);
    public static float Effects => ReadVolume("Effects", .8f);
    private static float ReadVolume(string name, float fallback) => Mathf.Clamp01(PlayerPrefs.GetFloat(Prefix + name, fallback));
    public static void SetLanguage(bool chinese) { PlayerPrefs.SetInt(Prefix + "English", chinese ? 0 : 1); Notify(); }
    public static void SetSound(bool enabled) { PlayerPrefs.SetInt(Prefix + "Sound", enabled ? 1 : 0); Notify(); }
    public static void SetVolume(string name, float value)
    {
        if (name is not ("Master" or "Music" or "Effects")) throw new ArgumentException(nameof(name));
        PlayerPrefs.SetFloat(Prefix + name, Mathf.Clamp01(value)); Notify();
    }
    public static void Apply() => AudioListener.volume = SoundEnabled ? Master : 0f;
    private static void Notify() { Apply(); Changed?.Invoke(); }
    public static void Save() => PlayerPrefs.Save();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() { Changed = null; IsOpen = false; }
}

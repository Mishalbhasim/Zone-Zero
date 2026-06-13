using UnityEngine;

public class SettingsManager : Singleton<SettingsManager>
{
    public float CameraSensitivity { get; set; } = 1f;
    public float MusicVolume { get; set; } = 0.7f;
    public float SFXVolume { get; set; } = 1f;
    public int GraphicsQuality { get; set; } = 1;

    protected override void Awake()
    {
        base.Awake();
        Load();
    }

    public void Save()
    {
        PlayerPrefs.SetFloat("Sensitivity", CameraSensitivity);
        PlayerPrefs.SetFloat("Music", MusicVolume);
        PlayerPrefs.SetFloat("SFX", SFXVolume);
        PlayerPrefs.SetInt("Quality", GraphicsQuality);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        CameraSensitivity = PlayerPrefs.GetFloat("Sensitivity", 1f);
        MusicVolume = PlayerPrefs.GetFloat("Music", 0.7f);
        SFXVolume = PlayerPrefs.GetFloat("SFX", 1f);
        GraphicsQuality = PlayerPrefs.GetInt("Quality", 1);
    }
}
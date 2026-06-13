using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;

    protected override void Awake()
    {
        base.Awake();
    }

    //Music
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || _musicSource.clip == clip) return;
        _musicSource.clip = clip;
        _musicSource.loop = true;
        _musicSource.Play();
    }

    public void StopMusic() => _musicSource.Stop();

    public void SetMusicVolume(float volume)
    {
        _musicSource.volume = volume;
    }

    //SFX
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        _sfxSource.PlayOneShot(clip);
    }

    public void SetSFXVolume(float volume)
    {
        _sfxSource.volume = volume;
    }
}
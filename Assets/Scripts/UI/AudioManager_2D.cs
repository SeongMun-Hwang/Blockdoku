using UnityEngine;

public class AudioManager_2D : MonoBehaviour
{
    public static AudioManager_2D Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip blockEraseClip;
    [SerializeField] private AudioClip blockThudClip;
    [SerializeField] private AudioClip errorClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Auto-assign AudioSources if not set in inspector
        if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        sfxSource.playOnAwake = false;
    }

    private void Start()
    {
        ApplySettings();
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnSettingsChanged += ApplySettings;
        }
    }

    private void OnDestroy()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnSettingsChanged -= ApplySettings;
        }
    }

    public void PlayBgm()
    {
        if (!SettingsManager.Instance.BgmMute && !bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }

    public void StopBgm()
    {
        bgmSource.Stop();
    }

    public void PlayBlockThudAudio()
    {
        if (SettingsManager.Instance.SfxMute) return;
        sfxSource.PlayOneShot(blockThudClip);
    }

    public void PlayErrorAudio()
    {
        if (SettingsManager.Instance.SfxMute) return;
        sfxSource.PlayOneShot(errorClip);
    }

    public void PlayBlockDestroyAudio(int combo)
    {
        if (SettingsManager.Instance.SfxMute) return;

        sfxSource.pitch = 1f + ((combo-1) * 0.1f);
        sfxSource.clip = blockEraseClip;
        sfxSource.Play();
    }

    public void ApplySettings()
    {
        if (SettingsManager.Instance == null) return;

        if (bgmSource != null)
        {
            bgmSource.mute = SettingsManager.Instance.BgmMute;
            bgmSource.volume = SettingsManager.Instance.BgmVolume;
            if (SettingsManager.Instance.BgmMute) bgmSource.Stop();
            else if (!bgmSource.isPlaying) bgmSource.Play();
        }

        if (sfxSource != null)
        {
            sfxSource.mute = SettingsManager.Instance.SfxMute;
            sfxSource.volume = SettingsManager.Instance.SfxVolume;
        }
    }
}

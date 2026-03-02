using System.IO;
using UnityEngine;

using static SavePaths;

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

    public bool bgmMute { get; set; }
    public bool sfxMute { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadAudioData_2D();
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
        ApplyMuteSettings();
    }

    public void PlayBgm()
    {
        if (!bgmMute && !bgmSource.isPlaying)
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
        if (sfxMute) return;
        sfxSource.PlayOneShot(blockThudClip);
    }

    public void PlayErrorAudio()
    {
        if (sfxMute) return;
        sfxSource.PlayOneShot(errorClip);
    }

    public void PlayBlockDestroyAudio(int combo)
    {
        if (sfxMute) return;
        
        // Match the 3D version's combo pitch logic
        sfxSource.pitch = 1f + (combo * 0.1f);
        sfxSource.PlayOneShot(blockEraseClip);
        sfxSource.pitch = 1f; // Reset pitch for next sound
    }

    public void ToggleSfxMute()
    {
        sfxMute = !sfxMute;
        ApplyMuteSettings();
        SaveAudioData_2D();
    }

    public void ToggleBgmMute()
    {
        bgmMute = !bgmMute;
        ApplyMuteSettings();
        SaveAudioData_2D();
    }

    public void ApplyMuteSettings()
    {
        if (bgmSource != null)
        {
            bgmSource.mute = bgmMute;
            if (bgmMute) bgmSource.Stop();
            else if (!bgmSource.isPlaying) bgmSource.Play();
        }
    }

    public void SaveAudioData_2D()
    {
        AudioData audioData = new AudioData
        {
            bgmMute = bgmMute,
            sfxMute = sfxMute
        };

        string json = JsonUtility.ToJson(audioData);
        string path = SettingDataPath;
        File.WriteAllText(path, json);
        Debug.Log("2D Audio data saved to " + path);
    }

    public void LoadAudioData_2D()
    {
        if (File.Exists(SettingDataPath))
        {
            string json = File.ReadAllText(SettingDataPath);
            AudioData audioData = JsonUtility.FromJson<AudioData>(json);
            bgmMute = audioData.bgmMute;
            sfxMute = audioData.sfxMute;
            Debug.Log("2D Audio data loaded from " + SettingDataPath);
        }
        else
        {
            Debug.Log("2D Audio data file not found at " + SettingDataPath);
            bgmMute = false;
            sfxMute = false;
        }
    }
}

using UnityEngine;
using System.IO;
using System;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private AudioData settings;

    public event Action OnSettingsChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool BgmMute
    {
        get => settings.bgmMute;
        set { settings.bgmMute = value; SaveSettings(); OnSettingsChanged?.Invoke(); }
    }

    public bool SfxMute
    {
        get => settings.sfxMute;
        set { settings.sfxMute = value; SaveSettings(); OnSettingsChanged?.Invoke(); }
    }

    public bool VibrationMute
    {
        get => settings.vibrationMute;
        set { settings.vibrationMute = value; SaveSettings(); OnSettingsChanged?.Invoke(); }
    }

    public float BgmVolume
    {
        get => settings.bgmVolume;
        set { settings.bgmVolume = Mathf.Clamp01(value); SaveSettings(); OnSettingsChanged?.Invoke(); }
    }

    public float SfxVolume
    {
        get => settings.sfxVolume;
        set { settings.sfxVolume = Mathf.Clamp01(value); SaveSettings(); OnSettingsChanged?.Invoke(); }
    }

    public float VibrationValue
    {
        get => settings.vibrationValue;
        set { settings.vibrationValue = Mathf.Clamp01(value); SaveSettings(); OnSettingsChanged?.Invoke(); }
    }

    public void SaveSettings()
    {
        string json = JsonUtility.ToJson(settings);
        File.WriteAllText(SavePaths.SettingDataPath, json);
    }

    private void LoadSettings()
    {
        if (File.Exists(SavePaths.SettingDataPath))
        {
            string json = File.ReadAllText(SavePaths.SettingDataPath);
            settings = JsonUtility.FromJson<AudioData>(json);
        }
        else
        {
            settings = new AudioData
            {
                bgmMute = false,
                sfxMute = false,
                vibrationMute = false,
                bgmVolume = 1.0f,
                sfxVolume = 1.0f,
                vibrationValue = 1.0f
            };
        }
    }

    public void Vibrate()
    {
        // Check both mute toggle and slider value
        if (!settings.vibrationMute && settings.vibrationValue > 0)
        {
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }
}

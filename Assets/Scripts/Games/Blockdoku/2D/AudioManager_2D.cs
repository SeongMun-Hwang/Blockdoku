using System.IO;
using UnityEngine;

using static SavePaths;

public class AudioManager_2D : MonoBehaviour
{
    public static AudioManager_2D Instance { get; private set; }

    public bool bgmMute;
    public bool sfxMute;

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

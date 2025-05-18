using System.IO;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioClip blockEraseClip;
    [SerializeField] AudioClip blockThudClip;
    private AudioSource audioSource;
    private int multiplier = 1;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void PlayerBlockThudAudio()
    {
        audioSource.volume = 1f * multiplier;
        audioSource.PlayOneShot(blockThudClip);
    }
    public void PlayerBlockDestoryAudio()
    {
        int combo = GameManager.Instance.scoreManager.GetCombo();
        audioSource.volume = 0.1f * multiplier;
        audioSource.pitch = 1 + (combo * 0.1f);
        audioSource.PlayOneShot(blockEraseClip);
    }
    public void SetMultipier()
    {
        if (multiplier == 1) multiplier = 0;
        else multiplier = 1;
    }
    public int GetMultiplier()
    {
        return multiplier;
    }
    public void SaveAudioData()
    {
        AudioData audioData = new AudioData();
        audioData.multiplier = multiplier;

        string json = JsonUtility.ToJson(audioData);
        string path = SavePaths.SettingDataPath;

        File.WriteAllText(path, json);
        Debug.Log("Audio data saved to " + path);
    }
    public void LoadAudioData()
    {
        if (File.Exists(SavePaths.SettingDataPath))
        {
            string json = File.ReadAllText(SavePaths.SettingDataPath);
            AudioData audioData = JsonUtility.FromJson<AudioData>(json);
            multiplier = audioData.multiplier;
            Debug.Log("Audio data loaded from " + SavePaths.SettingDataPath);
        }
        else
        {
            Debug.Log("Audio data file not found at " + SavePaths.SettingDataPath);
        }
    }
}
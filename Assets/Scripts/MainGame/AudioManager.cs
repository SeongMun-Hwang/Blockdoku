using System.IO;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioClip blockEraseClip;
    [SerializeField] AudioClip blockThudClip;
    [SerializeField] AudioClip bgmClip;
    private AudioSource audioSource;
    private bool sfxMute;
    private bool bgmMute;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void PlayerBlockThudAudio()
    {
        if(sfxMute) return;
        audioSource.PlayOneShot(blockThudClip);
    }
    public void PlayerBlockDestoryAudio()
    {
        if (sfxMute) return;
        int combo = GameManager.Instance.scoreManager.GetCombo();
        audioSource.pitch = 1 + (combo * 0.1f);
        audioSource.PlayOneShot(blockEraseClip);
        audioSource.pitch = 1f;
    }
    public void SetSfxMute()
    {
        if (sfxMute == true) sfxMute = false;
        else sfxMute = true;
    }
    public bool GetSfxMute()
    {
        return sfxMute;
    }
    public void SetBgmMute()
    {
        if (bgmMute == true)
        {
            bgmMute = false;
            audioSource.Play();
        }
        else
        {
            bgmMute = true;
            audioSource.Stop();
        }
    }
    public bool GetBgmMute()
    {
        return bgmMute;
    }
    public void SaveAudioData()
    {
        AudioData audioData = new AudioData();
        audioData.sfxMute = sfxMute;
        audioData.bgmMute = bgmMute;
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
            sfxMute = audioData.sfxMute;
            bgmMute = audioData.bgmMute;
            Debug.Log("Audio data loaded from " + SavePaths.SettingDataPath);
        }
        else
        {
            Debug.Log("Audio data file not found at " + SavePaths.SettingDataPath);
        }
    }
}
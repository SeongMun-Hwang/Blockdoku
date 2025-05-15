using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioClip blockEraseClip;
    [SerializeField] AudioClip blockThudClip;
    private AudioSource audioSource;
    private int multiplier = 1;

    void Start()
    {
        audioSource=GetComponent<AudioSource>();
    }
    public void PlayerBlockThudAudio()
    {
        audioSource.volume = 1f*multiplier;
        audioSource.PlayOneShot(blockThudClip);
    }
    public void PlayerBlockDestoryAudio()
    {
        int combo = GameManager.Instance.scoreManager.GetCombo();
        audioSource.volume = 0.1f*multiplier;
        audioSource.pitch=1 + (combo * 0.1f);
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
}

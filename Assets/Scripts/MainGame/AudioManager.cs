using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioClip audioClip;
    private AudioSource audioSource;

    void Start()
    {
        audioSource=GetComponent<AudioSource>();
    }

    public void PlayerBlockDestoryAudio()
    {
        int combo = GameManager.Instance.scoreManager.GetCombo();
        audioSource.pitch=1 + (combo * 0.1f);
        audioSource.PlayOneShot(audioClip);
    }
}

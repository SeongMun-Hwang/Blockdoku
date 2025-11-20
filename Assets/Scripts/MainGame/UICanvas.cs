using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UICanvas : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreTmp;
    [SerializeField] TextMeshProUGUI comboTmp;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] GameObject gameResetPanel;

    //음소거 버튼 이미지
    [SerializeField] Image sfxMuteButton;
    [SerializeField] Sprite sfxOff;
    [SerializeField] Sprite sfxOn;

    [SerializeField] Image bgmMuteButton;
    [SerializeField] Sprite bgmOff;
    [SerializeField] Sprite bgmOn;

    [SerializeField] Image vibrationMuteButton;
    [SerializeField] Sprite vibrationOff;
    [SerializeField] Sprite vibrationOn;
    private bool isVibrationMuted = false;

    [SerializeField] GameObject settingPanel;

    private static UICanvas instance;
    public static UICanvas Instance
    {
        get { return instance; }
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    private void Start()
    {
        GameManager.Instance.audioManager.LoadAudioData();
        if (!GameManager.Instance.audioManager.GetSfxMute())
        {
            sfxMuteButton.sprite = sfxOn;
        }
        else
        {
            sfxMuteButton.sprite = sfxOff;
        }

        if (!GameManager.Instance.audioManager.GetBgmMute())
        {
            bgmMuteButton.sprite = bgmOn;
            GameManager.Instance.audioManager.PlayBgm();
        }
        else
        {
            bgmMuteButton.sprite = bgmOff;
        }

        isVibrationMuted = PlayerPrefs.GetInt("VibrationMuted", 0) == 1;
        UpdateVibrationButtonSprite();
    }
    public void SetScore(int score)
    {
        if (scoreTmp == null) return;
        scoreTmp.text = score.ToString();
    }
    public void ShowCombo(string str)
    {
        StartCoroutine(ShowComboCoroutine(str));
    }
    private IEnumerator ShowComboCoroutine(string str)
    {
        comboTmp.text = str;

        Color textColor = comboTmp.color;
        textColor.a = 1f;

        for (float f = 0; f < 1f; f += Time.deltaTime)
        {
            textColor.a = Mathf.Lerp(1f, 0f, f / 1f);
            comboTmp.color = textColor;
            yield return null;
        }
    }
    public void ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true);
        GameManager.Instance.RemoveGameData();
        GameManager.Instance.scoreManager.SaveBestScore();
        AdManager.Instance.ShowInterstitialAd(); // 광고 표시
    }
    public void SettingPanelClicked()
    {
        if (settingPanel.activeSelf) settingPanel.SetActive(false);
        else settingPanel.SetActive(true);
    }
    public void RetryBtnOnclicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void TitleBtnOnClicked()
    {
        SceneManager.LoadScene("Title");
    }
    public void BackBtnOnClicked()
    {
        GameManager.Instance.SaveGameData();
        SceneManager.LoadScene("Title");
    }
    //게임 리셋 버튼 Onclick 함수
    public void ResetBtnOnClicked()
    {
        gameResetPanel.SetActive(true);
    }
    public void ResetPanelYes()
    {
        GameManager.Instance.RemoveGameData();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void ResetPanelNo()
    {
        gameResetPanel.SetActive(false);
    }
    public void SfxMuteBtnOnClicked()
    {
        GameManager.Instance.audioManager.SetSfxMute();
        if (!GameManager.Instance.audioManager.GetSfxMute())
        {
            sfxMuteButton.sprite = sfxOn;
        }
        else
        {
            sfxMuteButton.sprite = sfxOff;
        }
    }
    public void BgmMuteBtnOnClicked()
    {
        GameManager.Instance.audioManager.SetBgmMute();
        if (!GameManager.Instance.audioManager.GetBgmMute())
        {
            bgmMuteButton.sprite = bgmOn;
        }
        else
        {
            bgmMuteButton.sprite = bgmOff;
        }
    }
    public void VibrationMuteBtnOnClicked()
    {
        isVibrationMuted = !isVibrationMuted;
        PlayerPrefs.SetInt("VibrationMuted", isVibrationMuted ? 1 : 0);
        PlayerPrefs.Save();
        UpdateVibrationButtonSprite();
    }

    private void UpdateVibrationButtonSprite()
    {
        if (vibrationMuteButton != null)
        {
            vibrationMuteButton.sprite = isVibrationMuted ? vibrationOff : vibrationOn;
        }
    }

    public void Vibrate()
    {
        if (!isVibrationMuted)
        {
            Handheld.Vibrate();
        }
    }
}

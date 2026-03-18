using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIManager_2D : MonoBehaviour
{
    [Header("Top UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestScoreText;
    public FloatingScore floatingScore; // New field for floating score

    [Header("Panels")]
    public GameObject gameOverPanel;
    public GameObject gameResetPanel;
    public GameObject settingPanel;

    [Header("Buttons & Icons")]
    public Button restartButton;
    public Button titleButton;
    public Button settingsButton;
    public Button GameOverPanelYesButton;
    public Button ResetPanelYesButton;   
    
    [Header("Setting Icons")]
    public Image sfxMuteButtonIcon;
    public Image bgmMuteButtonIcon;
    public Image vibrationMuteButtonIcon;
    public Sprite sfxOn, sfxOff;
    public Sprite bgmOn, bgmOff;
    public Sprite vibrationOn, vibrationOff;

    [Header("Game Over UI")]
    public TextMeshProUGUI finalScoreText;
    public GameObject newBestObj;

    private bool isVibrationMuted = false;

    void Awake()
    {
        // Ensure panels are hidden at the start
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameResetPanel != null) gameResetPanel.SetActive(false);
        if (settingPanel != null) settingPanel.SetActive(false);

        // Assign button listeners
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(ShowResetPanel);
        }
        if (titleButton != null)
        {
            titleButton.onClick.AddListener(() => GameManager_2D.Instance.GoToTitle());
        }
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(ToggleSettingPanel);
        }
        if(ResetPanelYesButton != null)
        {
            ResetPanelYesButton.onClick.AddListener(()=>UI_Functions.Instance.TriggerGameRestart());
        }
        if(GameOverPanelYesButton != null)
        {
            GameOverPanelYesButton.onClick.AddListener(()=>UI_Functions.Instance.BacktoTitleOnClicked());
        }

        isVibrationMuted = PlayerPrefs.GetInt("VibrationMuted", 0) == 1;
    }

    void Start()
    {
        UpdateSettingIcons();
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = $"{score}";
    }

    public void UpdateBestScore(int bestScore)
    {
        if (bestScoreText != null) bestScoreText.text = $"{bestScore}";
    }

    public void ShowFloatingScore(int score, int combo, string specialMessage = "")
    {
        if (floatingScore != null && score != 0)
        {
            floatingScore.Show(score, combo, specialMessage);
        }
    }

    public void ShowGameOverPanel(bool show, int finalScore = 0, int bestScore = 0)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(show);
            if (show)
            {
                if (finalScoreText != null) finalScoreText.text = $"Score: {finalScore}";
                if (newBestObj != null)
                {
                    if (finalScore > bestScore) newBestObj.SetActive(true);
                }
                
                // Call external systems (Ads, Vibration)
                Vibrate();
            }
        }
    }

    // --- Settings Logic ---
    public void ToggleSettingPanel()
    {
        if (settingPanel != null) settingPanel.SetActive(!settingPanel.activeSelf);
    }

    public void SfxMuteBtnOnClicked()
    {
        AudioManager_2D.Instance.ToggleSfxMute();
        UpdateSettingIcons();
    }

    public void BgmMuteBtnOnClicked()
    {
        AudioManager_2D.Instance.ToggleBgmMute();
        UpdateSettingIcons();
    }

    public void VibrationMuteBtnOnClicked()
    {
        isVibrationMuted = !isVibrationMuted;
        PlayerPrefs.SetInt("VibrationMuted", isVibrationMuted ? 1 : 0);
        PlayerPrefs.Save();
        UpdateSettingIcons();
    }

    private void UpdateSettingIcons()
    {
        if (sfxMuteButtonIcon != null) sfxMuteButtonIcon.sprite = AudioManager_2D.Instance.sfxMute ? sfxOff : sfxOn;
        if (bgmMuteButtonIcon != null) bgmMuteButtonIcon.sprite = AudioManager_2D.Instance.bgmMute ? bgmOff : bgmOn;
        if (vibrationMuteButtonIcon != null) vibrationMuteButtonIcon.sprite = isVibrationMuted ? vibrationOff : vibrationOn;
    }

    public void Vibrate()
    {
        if (!isVibrationMuted)
        {
            Handheld.Vibrate();
        }
    }

    // --- Reset Confirmation Logic ---
    public void ShowResetPanel()
    {
        if (gameResetPanel != null) gameResetPanel.SetActive(!gameResetPanel.activeSelf);
    }

    public void ResetPanelYes()
    {
        GameManager_2D.Instance.RemoveGameData();
        UI_Functions.Instance.TriggerGameRestart();
    }
}


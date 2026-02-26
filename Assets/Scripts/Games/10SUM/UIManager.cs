
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestScoreMainGameText; // New field for best score on main game screen
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public Button restartButton;
    public Button titleButton;

    void Awake()
    {
        // Ensure the game over panel is hidden at the start
        gameOverPanel.SetActive(false);

        // Assign button listeners
        restartButton.onClick.AddListener(() => TENSUM_GameManager.Instance.RestartGame());
        titleButton.onClick.AddListener(() => TENSUM_GameManager.Instance.GoToTitle());
    }

    public void UpdateTimer(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void UpdateScore(int score)
    {
        scoreText.text = $"{score}";
    }

    public void UpdateBestScoreMainGame(int bestScore) // New method to update best score on main game screen
    {
        bestScoreMainGameText.text = $"{bestScore}";
    }

    public void ShowGameOverPanel(bool show, int finalScore = 0, int bestScore = 0)
    {
        gameOverPanel.SetActive(show);
        if (show)
        {
            finalScoreText.text = $"Score: {finalScore}";
        }
    }
}

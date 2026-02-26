
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI bestScoreText; // Added this line
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

    public void ShowGameOverPanel(bool show, int finalScore = 0, int bestScore = 0) // Modified this line
    {
        gameOverPanel.SetActive(show);
        if (show)
        {
            finalScoreText.text = $"Score: {finalScore}"; // Modified this line
            bestScoreText.text = $"Best Score: {bestScore}"; // Added this line
        }
    }
}

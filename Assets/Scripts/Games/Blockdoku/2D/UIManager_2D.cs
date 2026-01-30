using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager_2D : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public Button restartButton;
    public Button titleButton;

    void Awake()
    {
        // Ensure the game over panel is hidden at the start
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Assign button listeners
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(() => GameManager_2D.Instance.RestartGame());
        }
        if (titleButton != null)
        {
            titleButton.onClick.AddListener(() => GameManager_2D.Instance.GoToTitle());
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    public void ShowGameOverPanel(bool show, int finalScore = 0)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(show);
            if (show && finalScoreText != null)
            {
                finalScoreText.text = $"Final Score: {finalScore}";
            }
        }
    }
}

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Games._2048
{
    public class UIManager2048 : MonoBehaviour
    {
        public static UIManager2048 Instance { get; private set; }

        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Button backToTitleButton;

        [SerializeField] private Button backToTitlePanelbutton;
        [SerializeField] private Button restartGameButton;
        [SerializeField] private TextMeshProUGUI finalScoreTmp;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }

        void OnEnable()
        {
            if (GameManager2048.Instance != null)
            {
                GameManager2048.Instance.OnScoreChanged += UpdateScoreUI;
                GameManager2048.Instance.OnBestScoreChanged += UpdateBestScoreUI;
                GameManager2048.Instance.OnGameOver += ShowGameOverUI;
            }
        }

        void OnDisable()
        {
            if (GameManager2048.Instance != null)
            {
                GameManager2048.Instance.OnScoreChanged -= UpdateScoreUI;
                GameManager2048.Instance.OnBestScoreChanged -= UpdateBestScoreUI;
                GameManager2048.Instance.OnGameOver -= ShowGameOverUI;
            }
        }

        void Start()
        {
            // Re-subscribe if OnEnable failed due to initialization order
            GameManager2048.Instance.OnScoreChanged -= UpdateScoreUI;
            GameManager2048.Instance.OnScoreChanged += UpdateScoreUI;
            GameManager2048.Instance.OnBestScoreChanged -= UpdateBestScoreUI;
            GameManager2048.Instance.OnBestScoreChanged += UpdateBestScoreUI;
            GameManager2048.Instance.OnGameOver -= ShowGameOverUI;
            GameManager2048.Instance.OnGameOver += ShowGameOverUI;

            if (backToTitleButton != null)
            {
                backToTitleButton.onClick.AddListener(OnBackToTitleClicked);
            }
            if (backToTitlePanelbutton != null)
            {
                backToTitlePanelbutton.onClick.AddListener(OnBackToTitleClicked);
            }
            if (restartGameButton != null)
            {
                restartGameButton.onClick.AddListener(RestartGame);
            }
            
            // Initial UI Sync
            UpdateScoreUI(GameManager2048.Instance.Score);
            UpdateBestScoreUI(GameManager2048.Instance.HighScore);
        }

        private void UpdateScoreUI(int score)
        {
            if (scoreText != null) scoreText.text = score.ToString();
        }

        private void UpdateBestScoreUI(int bestScore)
        {
            if (highScoreText != null) highScoreText.text = bestScore.ToString();
        }

        private void ShowGameOverUI(bool isGameOver)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(isGameOver);
                if (isGameOver)
                {
                    if (finalScoreTmp != null)
                    {
                        finalScoreTmp.text = GameManager2048.Instance.Score.ToString();
                    }
                    UpdateBestScoreUI(GameManager2048.Instance.HighScore);
                }
            }
        }

        private void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnBackToTitleClicked()
        {
            if (UI_Functions.Instance != null)
            {
                UI_Functions.Instance.BacktoTitleOnClicked();
            }
        }
    }
}

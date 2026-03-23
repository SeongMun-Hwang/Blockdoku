using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Games._2048
{
    public class UIManager2048 : MonoBehaviour
    {
        public static UIManager2048 Instance { get; private set; }

        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button backToTitleButton;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            GameManager2048.Instance.OnScoreChanged += UpdateScoreUI;
            GameManager2048.Instance.OnGameOver += ShowGameOverUI;

            restartButton.onClick.AddListener(OnRestartClicked);
            if (backToTitleButton != null)
            {
                backToTitleButton.onClick.AddListener(OnBackToTitleClicked);
            }

            UpdateHighScoreUI();
            gameOverPanel.SetActive(false);
        }

        void OnDestroy()
        {
            if (GameManager2048.Instance != null)
            {
                GameManager2048.Instance.OnScoreChanged -= UpdateScoreUI;
                GameManager2048.Instance.OnGameOver -= ShowGameOverUI;
            }
        }

        private void UpdateScoreUI(int score)
        {
            scoreText.text = score.ToString();
        }

        private void UpdateHighScoreUI()
        {
            highScoreText.text = GameManager2048.Instance.HighScore.ToString();
        }

        private void ShowGameOverUI()
        {
            gameOverPanel.SetActive(true);
            UpdateHighScoreUI();
        }

        private void OnRestartClicked()
        {
            gameOverPanel.SetActive(false);
            GameManager2048.Instance.StartNewGame();
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

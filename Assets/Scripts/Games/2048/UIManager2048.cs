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

        [InspectorName("GameOverPanel")]
        [SerializeField] private Button backToTitlePanelbutton;
        [SerializeField] private Button restartGameButton;
        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            GameManager2048.Instance.OnScoreChanged += UpdateScoreUI;
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

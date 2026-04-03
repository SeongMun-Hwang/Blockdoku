using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MineSweeper_UIManager : MonoBehaviour
{
    [Header("Main Game UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI mineCountText;

    [Header("Difficulty Selection")]
    public GameObject difficultyPanel;
    public Button beginnerButton;
    public Button intermediateButton;
    public Button advancedButton;
    public TextMeshProUGUI bestTimeBeginnerText;
    public TextMeshProUGUI bestTimeIntermediateText;
    public TextMeshProUGUI bestTimeAdvancedText;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI finalTimeText;
    public TextMeshProUGUI bestTimeText;
    public Button restartButton;
    public Button titleButton;

    [Header("Basic UI")]
    [SerializeField] private Button backToTitle;

    [Header("Settings UI")]
    public GameObject settingsPanel;
    public Button settingsButton;
    public Button closeSettingsButton;

    [Header("Camera Control UI")]
    public MineSweeper_CameraController cameraController;
    public Button zoomInButton;
    public Button zoomOutButton;
    public Button resetCameraButton;
    public Vector2 joystickInput; // Assign from an external joystick script or update in Update()

    private int zoomDir = 0; // 1 for zoom in, -1 for zoom out, 0 for none

    void Awake()
    {
        beginnerButton.onClick.AddListener(() => MineSweeper_GameManager.Instance.StartGame(MineSweeperDifficulty.Beginner));
        intermediateButton.onClick.AddListener(() => MineSweeper_GameManager.Instance.StartGame(MineSweeperDifficulty.Intermediate));
        advancedButton.onClick.AddListener(() => MineSweeper_GameManager.Instance.StartGame(MineSweeperDifficulty.Advanced));

        restartButton.onClick.AddListener(() => MineSweeper_GameManager.Instance.RestartGame());
        titleButton.onClick.AddListener(() => MineSweeper_GameManager.Instance.GoToTitle());
        backToTitle.onClick.AddListener(() => MineSweeper_GameManager.Instance.GoToTitle());

        if (settingsButton != null) settingsButton.onClick.AddListener(() => ShowSettingsPanel(true));
        if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(() => ShowSettingsPanel(false));

        // Camera Control listeners (keep onClick for single clicks if desired, or remove if only holding is needed)
        if (resetCameraButton != null) resetCameraButton.onClick.AddListener(() => cameraController.ResetCamera());

        // Ensure settings panel is hidden at start
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    void Update()
    {
        if (cameraController == null) return;

        // Handle Joystick-based movement
        if (joystickInput != Vector2.zero)
        {
            cameraController.Move(joystickInput);
        }

        // Handle Continuous Zoom
        if (zoomDir != 0)
        {
            cameraController.ContinuousZoom(zoomDir);
        }
    }

    // --- UI Event Callbacks for Continuous Zoom ---
    public void OnZoomInDown() { zoomDir = 1; }
    public void OnZoomOutDown() { zoomDir = -1; }
    public void OnZoomPointerUp() { zoomDir = 0; }

    public void ShowSettingsPanel(bool show)
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(show);
        }
    }

    public void UpdateTimer(float time)
    {
        timerText.text = FormatTime(time);
    }

    public void UpdateMineCount(int count)
    {
        mineCountText.text = "Left : " + count.ToString();
    }

    public void ShowDifficultySelection(bool show)
    {
        difficultyPanel.SetActive(show);
    }

    public void ShowGameOverPanel(bool show, bool win = false, float time = 0, float bestTime = 0)
    {
        gameOverPanel.SetActive(show);
        if (show)
        {
            resultText.text = win ? "YOU WIN!" : "GAME OVER";
            finalTimeText.text = "Time: " + FormatTime(time);
            bestTimeText.text = "Best: " + (bestTime == float.MaxValue ? "--:--" : FormatTime(bestTime));
        }
    }

    public void UpdateBestTimes(MineSweeperData data)
    {
        bestTimeBeginnerText.text = data.bestTimeBeginner == float.MaxValue ? "Best: --:--" : "Best: " + FormatTime(data.bestTimeBeginner);
        bestTimeIntermediateText.text = data.bestTimeIntermediate == float.MaxValue ? "Best: --:--" : "Best: " + FormatTime(data.bestTimeIntermediate);
        bestTimeAdvancedText.text = data.bestTimeAdvanced == float.MaxValue ? "Best: --:--" : "Best: " + FormatTime(data.bestTimeAdvanced);
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}

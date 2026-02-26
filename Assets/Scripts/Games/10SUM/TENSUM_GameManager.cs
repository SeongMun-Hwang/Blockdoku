
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class TENSUM_GameManager : MonoBehaviour
{
    public static TENSUM_GameManager Instance { get; private set; }

    public UIManager uiManager;
    public GridManager gridManager;

    private int score = 0;
    private float timer;
    private bool isGameOver = false;

    private const float GAME_DURATION = 120f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        if (isGameOver) return;

        timer -= Time.deltaTime;
        uiManager.UpdateTimer(timer);

        if (timer <= 0)
        {
            uiManager.UpdateTimer(0);
            EndGame();
        }
    }

    public void StartGame()
    {
        score = 0;
        timer = GAME_DURATION;
        isGameOver = false;
        uiManager.UpdateScore(score);
        uiManager.ShowGameOverPanel(false);
        gridManager.InitializeGrid();

        // Load and display best score on main game screen
        int bestScore = 0;
        if (File.Exists(SavePaths.TenSumDataPath))
        {
            string json = File.ReadAllText(SavePaths.TenSumDataPath);
            TenSumData data = JsonUtility.FromJson<TenSumData>(json);
            bestScore = data.bestScore;
        }
        uiManager.UpdateBestScoreMainGame(bestScore);
    }

    public void AddScore(int amount)
    {
        score += amount;
        uiManager.UpdateScore(score);
    }

    private void EndGame()
    {
        isGameOver = true;
        int currentBestScore = 0;
        if (File.Exists(SavePaths.TenSumDataPath))
        {
            string json = File.ReadAllText(SavePaths.TenSumDataPath);
            TenSumData data = JsonUtility.FromJson<TenSumData>(json);
            currentBestScore = data.bestScore;
        }

        uiManager.ShowGameOverPanel(true, score, currentBestScore);
        SaveScore();
    }

    private void SaveScore()
    {
        int bestScore = 0;
        if (File.Exists(SavePaths.TenSumDataPath))
        {
            string json = File.ReadAllText(SavePaths.TenSumDataPath);
            TenSumData data = JsonUtility.FromJson<TenSumData>(json);
            bestScore = data.bestScore;
        }

        if (score > bestScore)
        {
            TenSumData data = new TenSumData
            {
                bestScore = score
            };
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(SavePaths.TenSumDataPath, json);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToTitle()
    {
        SceneManager.LoadScene("Title");
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }
}

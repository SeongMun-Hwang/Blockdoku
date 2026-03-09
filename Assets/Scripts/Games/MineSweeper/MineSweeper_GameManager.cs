using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public enum MineSweeperDifficulty
{
    Beginner,
    Intermediate,
    Advanced
}

public class MineSweeper_GameManager : MonoBehaviour
{
    public static MineSweeper_GameManager Instance { get; private set; }

    public MineSweeper_UIManager uiManager;
    public MineSweeper_GridManager gridManager;

    public bool IsGameOver { get; private set; }
    public bool IsGameStarted { get; private set; }
    private float timer;
    private int remainingMines;
    private MineSweeperDifficulty currentDifficulty;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        ShowDifficultySelection();
    }

    void Update()
    {
        if (IsGameStarted && !IsGameOver)
        {
            timer += Time.deltaTime;
            uiManager.UpdateTimer(timer);
        }
    }

    public void ShowDifficultySelection()
    {
        LoadBestTimes();
        uiManager.ShowDifficultySelection(true);
        uiManager.ShowGameOverPanel(false);
    }

    public void StartGame(MineSweeperDifficulty difficulty)
    {
        currentDifficulty = difficulty;
        IsGameOver = false;
        IsGameStarted = false;
        timer = 0;
        
        int rows, cols, mines;

        switch (difficulty)
        {
            case MineSweeperDifficulty.Beginner:
                rows = 9; cols = 9; mines = 10;
                break;
            case MineSweeperDifficulty.Intermediate:
                rows = 16; cols = 16; mines = 40;
                break;
            case MineSweeperDifficulty.Advanced:
                rows = 30; cols = 16; mines = 99;
                break;
            default:
                rows = 9; cols = 9; mines = 10;
                break;
        }

        remainingMines = mines;
        uiManager.UpdateMineCount(remainingMines);
        uiManager.UpdateTimer(timer);
        uiManager.ShowDifficultySelection(false);
        gridManager.InitializeGrid(rows, cols, mines);
    }

    public void OnGameStarted()
    {
        IsGameStarted = true;
    }

    public void OnFlagToggled(bool isAdded)
    {
        if (isAdded) remainingMines--;
        else remainingMines++;
        uiManager.UpdateMineCount(remainingMines);
    }

    public void GameOver(bool win)
    {
        IsGameOver = true;
        if (win)
        {
            SaveBestTime();
        }

        AdEventBus.TriggerGamePlayEnded(MinigameType.MineSweeper, () =>
        {
            uiManager.ShowGameOverPanel(true, win, timer, GetBestTimeForCurrentDifficulty());
        });
    }

    private void LoadBestTimes()
    {
        MineSweeperData data = new MineSweeperData();
        if (File.Exists(SavePaths.MineSweeperDataPath))
        {
            string json = File.ReadAllText(SavePaths.MineSweeperDataPath);
            if (!string.IsNullOrEmpty(json))
            {
                MineSweeperData loadedData = JsonUtility.FromJson<MineSweeperData>(json);
                if (loadedData != null)
                {
                    data = loadedData;
                }
            }
        }
        uiManager.UpdateBestTimes(data);
    }

    private void SaveBestTime()
    {
        MineSweeperData data = new MineSweeperData();
        if (File.Exists(SavePaths.MineSweeperDataPath))
        {
            string json = File.ReadAllText(SavePaths.MineSweeperDataPath);
            data = JsonUtility.FromJson<MineSweeperData>(json);
        }

        bool newBest = false;
        switch (currentDifficulty)
        {
            case MineSweeperDifficulty.Beginner:
                if (timer < data.bestTimeBeginner) { data.bestTimeBeginner = timer; newBest = true; }
                break;
            case MineSweeperDifficulty.Intermediate:
                if (timer < data.bestTimeIntermediate) { data.bestTimeIntermediate = timer; newBest = true; }
                break;
            case MineSweeperDifficulty.Advanced:
                if (timer < data.bestTimeAdvanced) { data.bestTimeAdvanced = timer; newBest = true; }
                break;
        }

        if (newBest)
        {
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(SavePaths.MineSweeperDataPath, json);
        }
    }

    private float GetBestTimeForCurrentDifficulty()
    {
        if (File.Exists(SavePaths.MineSweeperDataPath))
        {
            string json = File.ReadAllText(SavePaths.MineSweeperDataPath);
            MineSweeperData data = JsonUtility.FromJson<MineSweeperData>(json);
            switch (currentDifficulty)
            {
                case MineSweeperDifficulty.Beginner: return data.bestTimeBeginner;
                case MineSweeperDifficulty.Intermediate: return data.bestTimeIntermediate;
                case MineSweeperDifficulty.Advanced: return data.bestTimeAdvanced;
            }
        }
        return float.MaxValue;
    }

    public void RestartGame()
    {
        AdEventBus.TriggerGamePlayEnded(MinigameType.MineSweeper, () =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
    }

    public void GoToTitle()
    {
        SceneManager.LoadScene("Title");
    }
}

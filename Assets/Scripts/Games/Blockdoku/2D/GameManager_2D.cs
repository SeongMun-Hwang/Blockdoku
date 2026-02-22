using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

using static SavePaths;

public class GameManager_2D : MonoBehaviour
{
    public static GameManager_2D Instance { get; private set; }

    public UIManager_2D uiManager; // To be created
    public GridManager_2D gridManager;
    [SerializeField] public BlockSpawner_2D blockSpawner;
    [SerializeField] public AudioManager_2D audioManager;

    private int score = 0;
    public int combo = 0;
    private bool isGameOver = false;

    void OnEnable()
    {
        UI_Functions.OnGameRestart += HandleGameRestart;
    }

    void OnDisable()
    {
        UI_Functions.OnGameRestart -= HandleGameRestart;
    }

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
        Debug.Log("GameManager_2D Start called.");
        Debug.Log($"BoardDataPath exists: {File.Exists(BoardDataPath)}");
        Debug.Log($"GridManager is null in Start: {gridManager == null}");
        Debug.Log($"BlockSpawner is null in Start: {blockSpawner == null}"); // Added this line

        if (File.Exists(BoardDataPath))
        {
            LoadGameData();
        }
        else
        {
            StartGame();
        }
    }

    void Update()
    {
        if (isGameOver) return;

        // In Blockdoku, game over is not determined by a timer.
        // It's determined by whether the player can place any of the current blocks.
        // This logic will be implemented later.
    }

    private void HandleGameRestart()
    {
        RemoveGameData();
    }

    public void StartGame()
    {
        Debug.Log("GameManager_2D StartGame called.");
        score = 0;
        combo = 0;
        isGameOver = false;
        uiManager.UpdateScore(score);
        uiManager.ShowGameOverPanel(false);
        Debug.Log($"GridManager is null in StartGame: {gridManager == null}");
        Debug.Log("Calling gridManager.InitializeGrid().");
        gridManager.InitializeGrid();
        RemoveGameData(); // Start a new game, so remove old save data
        Debug.Log($"BlockSpawner is null in StartGame: {blockSpawner == null}");
        if (blockSpawner != null)
        {
            blockSpawner.SpawnBlocks(); // <--- Added this line to spawn blocks
            Debug.Log("BlockSpawner.SpawnBlocks() called.");
        }
        else
        {
            Debug.LogError("BlockSpawner is null, cannot spawn blocks.");
        }
    }

    public void AddScore(int amount)
    {
        score += amount * combo;
        uiManager.UpdateScore(score);
    }

    public int GetCombo()
    {
        return combo;
    }

    public void EndGame()
    {
        isGameOver = true;
        uiManager.ShowGameOverPanel(true, score);
        Debug.Log("Game Over! Final Score: " + score);
        SaveGameData();
    }

    public void GoToTitle()
    {
        SaveGameData();
        SceneManager.LoadScene("Title");
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public void CheckGameOver()
    {
        List<GameObject> remainingBlocks = BlockSpawner_2D.Instance.GetSpawnedBlocks();
        if (remainingBlocks.Count == 0) return;

        bool canPlaceAnyBlock = false;
        foreach (GameObject blockGO in remainingBlocks)
        {
            Block_2D block = blockGO.GetComponent<Block_2D>();
            if (CanBlockBePlaced(block))
            {
                canPlaceAnyBlock = true;
                break; // Found a placeable block, no need to check others
            }
        }

        if (!canPlaceAnyBlock)
        {
            EndGame();
        }
    }

    private bool CanBlockBePlaced(Block_2D block)
    {
        List<Vector2Int> shape = block.GetShape();
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (gridManager.IsValidPlacement(new Vector2Int(c, r), shape))
                {
                    return true; // Found a valid placement for this block
                }
            }
        }
        return false; // This block cannot be placed anywhere
    }

    public void SaveGameData()
    {
        gridManager.SaveBoardData_2D(score, combo);
        blockSpawner.SaveBlockData_2D();
        audioManager.SaveAudioData_2D();
        Debug.Log("2D Game data saved!");
    }

    public void LoadGameData()
    {
        (int loadedScore, int loadedCombo) = gridManager.LoadBoardData_2D();
        score = loadedScore;
        combo = loadedCombo;
        uiManager.UpdateScore(score); // Update UI with loaded score
        blockSpawner.LoadBlockData_2D();
        audioManager.LoadAudioData_2D();
        Debug.Log("2D Game data loaded!");
    }

    public void RemoveGameData()
    {
        Debug.Log($"Attempting to remove game data. BoardDataPath exists before: {File.Exists(BoardDataPath)}");
        if (File.Exists(BoardDataPath))
        {
            File.Delete(BoardDataPath);
            Debug.Log($"2D Board Save file deleted. BoardDataPath exists after: {File.Exists(BoardDataPath)}");
        }
        else
        {
            Debug.Log("BoardDataPath did not exist, no need to delete.");
        }

        Debug.Log($"Attempting to remove game data. BlockDataPath exists before: {File.Exists(BlockDataPath)}");
        if (File.Exists(BlockDataPath))
        {
            File.Delete(BlockDataPath);
            Debug.Log($"2D Block Save file deleted. BlockDataPath exists after: {File.Exists(BlockDataPath)}");
        }
        else
        {
            Debug.Log("BlockDataPath did not exist, no need to delete.");
        }
    }
}


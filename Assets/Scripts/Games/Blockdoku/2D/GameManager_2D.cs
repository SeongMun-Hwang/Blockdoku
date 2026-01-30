using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager_2D : MonoBehaviour
{
    public static GameManager_2D Instance { get; private set; }

    public UIManager_2D uiManager; // To be created
    public GridManager_2D gridManager;

    private int score = 0;
    private bool isGameOver = false;

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

        // In Blockdoku, game over is not determined by a timer.
        // It's determined by whether the player can place any of the current blocks.
        // This logic will be implemented later.
    }

    public void StartGame()
    {
        score = 0;
        isGameOver = false;
        uiManager.UpdateScore(score);
        uiManager.ShowGameOverPanel(false);
        gridManager.InitializeGrid();
    }

    public void AddScore(int amount)
    {
        score += amount;
        uiManager.UpdateScore(score);
    }

    public void EndGame()
    {
        isGameOver = true;
        uiManager.ShowGameOverPanel(true, score);
        Debug.Log("Game Over! Final Score: " + score);
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
}

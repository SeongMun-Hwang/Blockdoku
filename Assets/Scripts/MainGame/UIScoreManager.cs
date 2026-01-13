using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIScoreManager : MonoBehaviour
{
    public static UIScoreManager Instance;

    [SerializeField] private TextMeshProUGUI scoreText;
    private int score = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log($"UIScoreManager: Instance set to {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"UIScoreManager: Destroying duplicate {gameObject.name}. Existing instance: {Instance.gameObject.name}");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateScoreUI();
    }

    public void CheckBoardAfterPlacement()
    {
        HashSet<Cell> erasableCells = GetErasableCells();

        if (erasableCells.Count > 0)
        {
            // Add score
            AddScore(erasableCells.Count);
            UpdateScoreUI();

            // Clear the cells
            foreach (Cell cell in erasableCells)
            {
                cell.SetFilled(false);
            }
        }
    }

    private HashSet<Cell> GetErasableCells()
    {
        HashSet<Cell> erasableCells = new HashSet<Cell>();
        Cell[,] boardCells = UIGameManager.Instance.boardCells;

        // Check horizontal rows
        for (int i = 0; i < 9; i++)
        {
            bool isRowFilled = true;
            HashSet<Cell> tempCells = new HashSet<Cell>();
            for (int j = 0; j < 9; j++)
            {
                if (!boardCells[i, j].isFilled)
                {
                    isRowFilled = false;
                    break;
                }
                tempCells.Add(boardCells[i, j]);
            }
            if (isRowFilled)
            {
                erasableCells.UnionWith(tempCells);
            }
        }

        // Check vertical columns
        for (int j = 0; j < 9; j++)
        {
            bool isColFilled = true;
            HashSet<Cell> tempCells = new HashSet<Cell>();
            for (int i = 0; i < 9; i++)
            {
                if (!boardCells[i, j].isFilled)
                {
                    isColFilled = false;
                    break;
                }
                tempCells.Add(boardCells[i, j]);
            }
            if (isColFilled)
            {
                erasableCells.UnionWith(tempCells);
            }
        }

        // Check 3x3 squares
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                bool isBlockFilled = true;
                HashSet<Cell> tempCells = new HashSet<Cell>();
                for (int x = i * 3; x < (i + 1) * 3; x++)
                {
                    for (int y = j * 3; y < (j + 1) * 3; y++)
                    {
                        if (!boardCells[x, y].isFilled)
                        {
                            isBlockFilled = false;
                            break;
                        }
                        tempCells.Add(boardCells[x, y]);
                    }
                    if (!isBlockFilled) break;
                }

                if (isBlockFilled)
                {
                    erasableCells.UnionWith(tempCells);
                }
            }
        }
        return erasableCells;
    }

    public void AddScore(int amount)
    {
        score += amount;
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }
}

using UnityEngine;

public class UIGameManager : MonoBehaviour
{
    public static UIGameManager Instance;

    public GameObject gameBoard;
    public Cell[,] boardCells = new Cell[9, 9];

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        InitializeBoard();
    }

    void InitializeBoard()
    {
        if (gameBoard == null)
        {
            gameBoard = GameObject.Find("GameBoard");
        }

        if (gameBoard != null)
        {
            int childCount = gameBoard.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                int row = i / 9;
                int col = i % 9;
                boardCells[row, col] = gameBoard.transform.GetChild(i).GetComponent<Cell>();
            }
        }
        else
        {
            Debug.LogError("GameBoard not assigned in UIGameManager and could not be found automatically.");
        }
    }
    public bool TryPlaceBlock(int[,] blockShape, int startRow, int startCol)
    {
        if (IsValidPlacement(blockShape, startRow, startCol))
        {
            PlaceBlock(blockShape, startRow, startCol);
            return true;
        }
        return false;
    }

    private bool IsValidPlacement(int[,] blockShape, int startRow, int startCol)
    {
        int shapeRows = blockShape.GetLength(0);
        int shapeCols = blockShape.GetLength(1);

        if (startRow < 0 || startCol < 0 || startRow + shapeRows > 9 || startCol + shapeCols > 9)
        {
            return false; // Out of bounds
        }

        for (int row = 0; row < shapeRows; row++)
        {
            for (int col = 0; col < shapeCols; col++)
            {
                if (blockShape[row, col] == 1 && boardCells[startRow + row, startCol + col].isFilled)
                {
                    return false; // Cell is already occupied
                }
            }
        }
        return true;
    }

    private void PlaceBlock(int[,] blockShape, int startRow, int startCol)
    {
        int shapeRows = blockShape.GetLength(0);
        int shapeCols = blockShape.GetLength(1);

        for (int row = 0; row < shapeRows; row++)
        {
            for (int col = 0; col < shapeCols; col++)
            {
                if (blockShape[row, col] == 1)
                {
                    boardCells[startRow + row, startCol + col].SetFilled(true);
                }
            }
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.Collections; // Added for Coroutines
using System.Linq; // Added for HashSet.ToList()
using UnityEngine.UI; // Added for GridLayoutGroup
using System.IO;

using static SavePaths;

public class GridManager_2D : MonoBehaviour
{
    public static GridManager_2D Instance { get; private set; }

    public GameObject cellPrefab; // UI Image prefab for a cell
    public Transform gridParent; // Parent for the grid cells
    public Color previewColor = new Color(0f, 1f, 0f, 0.5f); // Green, semi-transparent for block placement preview
    public Color clearBlinkColor = Color.cyan; // Cyan, for cells that will be cleared
    public float clearBlinkInterval = 0.3f; // Interval for clear prediction blinking

    // New: Sprites for cell visuals
    public Sprite defaultEmptyCellSprite;
    public Sprite defaultOccupiedCellSprite;

    private const int GRID_SIZE = 9;
    private Cell_2D[,] grid = new Cell_2D[GRID_SIZE, GRID_SIZE];
    private List<Cell_2D> previewCells = new List<Cell_2D>(); // Cells showing block placement preview
    private HashSet<Cell_2D> currentlyBlinkingClearCells = new HashSet<Cell_2D>(); // Cells currently blinking for clear prediction
    private Dictionary<Cell_2D, Color> storedOriginalClearPredictColors = new Dictionary<Cell_2D, Color>(); // Stores colors of cells that blink
    private Coroutine clearPredictBlinkCoroutine;
    private GridLayoutGroup gridLayoutGroup; // New: Reference to GridLayoutGroup


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

        // Get GridLayoutGroup reference
        gridLayoutGroup = gridParent.GetComponent<GridLayoutGroup>();
        if (gridLayoutGroup == null)
        {
            Debug.LogError("GridManager_2D: GridLayoutGroup not found on gridParent!");
        }
    }

    public Color subgridBorderColor = Color.black;
    public float subgridBorderWidth = 5f;

    [System.Serializable]
    public class SaveData_2D
    {
        public bool[] cellOccupiedStates = new bool[GRID_SIZE * GRID_SIZE];
        public int score;
        public int combo;
    }

    public void InitializeGrid()
    {
        // Clear existing grid cells if any
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        for (int r = 0; r < GRID_SIZE; r++)
        {
            for (int c = 0; c < GRID_SIZE; c++)
            {
                GameObject cellGO = Instantiate(cellPrefab, gridParent);
                cellGO.name = $"Cell_{r}_{c}";
                grid[r, c] = cellGO.AddComponent<Cell_2D>();
                grid[r, c].Initialize(r, c, true); // true for isEmtpy

                // Add borders for the 3x3 grid visualization
                if (r % 3 == 2 && r < GRID_SIZE - 1)
                {
                    GameObject borderGO = new GameObject("HorizontalBorder");
                    borderGO.transform.SetParent(cellGO.transform, false);
                    Image borderImage = borderGO.AddComponent<Image>();
                    borderImage.color = subgridBorderColor;
                    RectTransform rt = borderGO.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0, 0);
                    rt.anchorMax = new Vector2(1, 0);
                    rt.pivot = new Vector2(0.5f, 0);
                    rt.anchoredPosition = Vector2.zero;
                    rt.sizeDelta = new Vector2(0, subgridBorderWidth);
                }
                if (c % 3 == 2 && c < GRID_SIZE - 1)
                {
                    GameObject borderGO = new GameObject("VerticalBorder");
                    borderGO.transform.SetParent(cellGO.transform, false);
                    Image borderImage = borderGO.AddComponent<Image>();
                    borderImage.color = subgridBorderColor;
                    RectTransform rt = borderGO.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(1, 0);
                    rt.anchorMax = new Vector2(1, 1);
                    rt.pivot = new Vector2(1, 0.5f);
                    rt.anchoredPosition = Vector2.zero;
                    rt.sizeDelta = new Vector2(subgridBorderWidth, 0);
                }
            }
        }
    }


    public bool IsValidPlacement(Vector2Int gridPosition, List<Vector2Int> blockShape)
    {
        foreach (var pos in blockShape)
        {
            int r = gridPosition.y - pos.y; // Corrected: subtract pos.y
            int c = gridPosition.x + pos.x;

            if (r < 0 || r >= GRID_SIZE || c < 0 || c >= GRID_SIZE)
            {
                return false; // Out of bounds
            }
            if (!grid[r, c].IsEmpty)
            {
                return false; // Cell is not empty
            }
        }
        return true;
    }

    public void PlaceBlock(Vector2Int gridPosition, List<Vector2Int> blockShape)
    {
        // First, clear any ongoing clear prediction blinks
        StopClearPredictBlink();

        foreach (var pos in blockShape)
        {
            int r = gridPosition.y - pos.y; // Corrected: subtract pos.y
            int c = gridPosition.x + pos.x;
            grid[r, c].SetOccupied();
        }

        CheckForCompletedLines();
    }
    
    public void ShowPreview(Vector2Int gridPosition, List<Vector2Int> blockShape)
    {
        ClearPreview(); // Clear previous block placement preview AND stop previous clear prediction blink

        if (IsValidPlacement(gridPosition, blockShape))
        {
            // Display block placement preview (static color)
            foreach (var pos in blockShape)
            {
                int r = gridPosition.y - pos.y;
                int c = gridPosition.x + pos.x;
                if (r >= 0 && r < GRID_SIZE && c >= 0 && c < GRID_SIZE)
                {
                    grid[r, c].SetPreview(previewColor);
                    previewCells.Add(grid[r, c]);
                }
            }

            // Check for potential clears and start blinking if any
            HashSet<Cell_2D> potentialClearedCells = GetPotentialClearedCells(gridPosition, blockShape);
            if (potentialClearedCells.Count > 0)
            {
                currentlyBlinkingClearCells = potentialClearedCells; // Store reference to these cells
                // StopClearPredictBlink() is called by ClearPreview()
                clearPredictBlinkCoroutine = StartCoroutine(ClearPredictBlink(currentlyBlinkingClearCells, clearBlinkColor, clearBlinkInterval));
            }
            else
            {
                StopClearPredictBlink(); // No potential clears, ensure blinking is stopped
            }
        }
        else
        {
            StopClearPredictBlink(); // Invalid placement, ensure blinking is stopped
        }
    }

    public void ClearPreview()
    {
        foreach (var cell in previewCells)
        {
            cell.ClearPreview(); // This also ensures originalColor is restored for previewed cells
        }
        previewCells.Clear();

        StopClearPredictBlink(); // Always stop clear prediction blinking when clearing preview
    }

    private void StopClearPredictBlink()
    {
        if (clearPredictBlinkCoroutine != null)
        {
            StopCoroutine(clearPredictBlinkCoroutine);
            clearPredictBlinkCoroutine = null;
        }
        // Ensure any cells that were blinking revert to their original state
        foreach (var cell in currentlyBlinkingClearCells)
        {
            if (cell != null && storedOriginalClearPredictColors.ContainsKey(cell)) // Check if cell object still exists and we have its original color
            {
                cell.cellImage.color = storedOriginalClearPredictColors[cell]; // Restore original color
            }
        }
        currentlyBlinkingClearCells.Clear();
        storedOriginalClearPredictColors.Clear(); // Clear the stored colors as well
    }


    private HashSet<Cell_2D> GetPotentialClearedCells(Vector2Int gridPosition, List<Vector2Int> blockShape)
    {
        HashSet<Cell_2D> potentialClearedCells = new HashSet<Cell_2D>();

        // Create a temporary grid state
        bool[,] tempGridOccupied = new bool[GRID_SIZE, GRID_SIZE];
        for (int r = 0; r < GRID_SIZE; r++)
        {
            for (int c = 0; c < GRID_SIZE; c++)
            {
                tempGridOccupied[r, c] = !grid[r, c].IsEmpty;
            }
        }

        // Simulate placing the block
        foreach (var pos in blockShape)
        {
            int r = gridPosition.y - pos.y;
            int c = gridPosition.x + pos.x;
            if (r >= 0 && r < GRID_SIZE && c >= 0 && c < GRID_SIZE)
            {
                tempGridOccupied[r, c] = true;
            }
        }

        // Check for completed rows, columns, and squares on the temporary grid
        List<int> tempCompletedRows = new List<int>();
        List<int> tempCompletedCols = new List<int>();

        for (int i = 0; i < GRID_SIZE; i++)
        {
            bool rowComplete = true;
            bool colComplete = true;
            for (int j = 0; j < GRID_SIZE; j++)
            {
                if (!tempGridOccupied[i, j]) rowComplete = false;
                if (!tempGridOccupied[j, i]) colComplete = false;
            }
            if (rowComplete) tempCompletedRows.Add(i);
            if (colComplete) tempCompletedCols.Add(i);
        }

        for (int r = 0; r < GRID_SIZE; r += 3)
        {
            for (int c = 0; c < GRID_SIZE; c += 3)
            {
                bool squareComplete = true;
                for (int i = r; i < r + 3; i++)
                {
                    for (int j = c; j < c + 3; j++)
                    {
                        if (!tempGridOccupied[i, j]) squareComplete = false;
                    }
                }
                if (squareComplete)
                {
                    // Add cells of the completed square to the set
                    for (int i = r; i < r + 3; i++)
                    {
                        for (int j = c; j < c + 3; j++)
                        {
                            potentialClearedCells.Add(grid[i, j]);
                        }
                    }
                }
            }
        }
        
        // Add cells from completed rows and columns to the set
        foreach (var row in tempCompletedRows)
        {
            for (int c = 0; c < GRID_SIZE; c++)
            {
                potentialClearedCells.Add(grid[row, c]);
            }
        }
        foreach (var col in tempCompletedCols)
        {
            for (int r = 0; r < GRID_SIZE; r++)
            {
                potentialClearedCells.Add(grid[r, col]);
            }
        }

        return potentialClearedCells;
    }


    private void CheckForCompletedLines()
    {
        // This method will now ONLY be called after a block is actually placed,
        // so it doesn't need to predict, just execute the clearing.

        List<int> completedRows = new List<int>();
        List<int> completedCols = new List<int>();
        int currentComboCount = 0; // Local variable to count combo for this placement

        // Check rows and columns
        for (int i = 0; i < GRID_SIZE; i++)
        {
            bool rowComplete = true;
            bool colComplete = true;
            for (int j = 0; j < GRID_SIZE; j++)
            {
                if (grid[i, j].IsEmpty) rowComplete = false;
                if (grid[j, i].IsEmpty) colComplete = false;
            }
            if (rowComplete) completedRows.Add(i);
            if (colComplete) completedCols.Add(i);
        }

        // Check 3x3 squares
        for (int r = 0; r < GRID_SIZE; r += 3)
        {
            for (int c = 0; c < GRID_SIZE; c += 3)
            {
                bool squareComplete = true;
                for (int i = r; i < r + 3; i++)
                {
                    for (int j = c; j < c + 3; j++)
                    {
                        if (grid[i, j].IsEmpty) squareComplete = false;
                    }
                }
                if (squareComplete)
                {
                    ClearSquare(r, c);
                    GameManager_2D.Instance.AddScore(9);
                    currentComboCount++;
                }
            }
        }

        // Clear lines and add score
        foreach (var row in completedRows)
        {
            ClearRow(row);
            GameManager_2D.Instance.AddScore(9);
            currentComboCount++;
        }
        foreach (var col in completedCols)
        {
            ClearCol(col);
            GameManager_2D.Instance.AddScore(9);
            currentComboCount++;
        }

        // Update global combo in GameManager
        if (currentComboCount > 0)
        {
            // If any lines were cleared, increment GameManager's combo
            // The score calculation in AddScore uses the combo AFTER it's updated.
            // So, we need to increment GameManager_2D.Instance.combo first.
            GameManager_2D.Instance.combo += currentComboCount; 
        }
        else
        {
            // No lines cleared, reset combo
            GameManager_2D.Instance.combo = 0;
        }
    }
    
    public Vector2 GetCellPitch()
    {
        if (gridLayoutGroup != null)
        {
            return gridLayoutGroup.cellSize + gridLayoutGroup.spacing;
        }
        // Fallback to a default size if GridLayoutGroup is not found
        Debug.LogWarning("GridManager_2D: GridLayoutGroup is null, returning default cell pitch of 50x50.");
        return Vector2.one * 50f;
    }

    public Vector2 GetCellSize()
    {
        if (gridLayoutGroup != null)
        {
            return gridLayoutGroup.cellSize;
        }
        // Fallback to a default size if GridLayoutGroup is not found
        Debug.LogWarning("GridManager_2D: GridLayoutGroup is null, returning default cell size of 50x50.");
        return Vector2.one * 50f; 
    }

    // Coroutine for blinking cells that will be cleared
    private IEnumerator ClearPredictBlink(HashSet<Cell_2D> cells, Color blinkColor, float interval)
    {
        // Populate storedOriginalClearPredictColors when the coroutine starts
        storedOriginalClearPredictColors.Clear(); // Ensure it's clean
        foreach (var cell in cells)
        {
            bool isPartOfBlockPreview = false;
            foreach(var previewCell in previewCells) {
                if (previewCell == cell) {
                    isPartOfBlockPreview = true;
                    break;
                }
            }

            if (!isPartOfBlockPreview)
            {
                // Store the current visual color of the cell
                storedOriginalClearPredictColors[cell] = cell.cellImage.color; 
            }
        }

        while (true)
        {
            foreach (var cell in cells)
            {
                bool isPartOfBlockPreview = false;
                foreach(var previewCell in previewCells) {
                    if (previewCell == cell) {
                        isPartOfBlockPreview = true;
                        break;
                    }
                    if (cell.gameObject == null) continue; // Defensive check
                }

                if (!isPartOfBlockPreview)
                {
                    cell.cellImage.color = blinkColor;
                }
            }
            yield return new WaitForSeconds(interval);

            foreach (var cell in cells)
            {
                bool isPartOfBlockPreview = false;
                foreach(var previewCell in previewCells) {
                    if (previewCell == cell) {
                        isPartOfBlockPreview = true;
                        break;
                    }
                    if (cell.gameObject == null) continue; // Defensive check
                }

                if (!isPartOfBlockPreview && storedOriginalClearPredictColors.ContainsKey(cell))
                {
                    cell.cellImage.color = storedOriginalClearPredictColors[cell];
                }
            }
            yield return new WaitForSeconds(interval);
        }
    }


    private void ClearRow(int row)
    {
        for (int c = 0; c < GRID_SIZE; c++)
        {
            grid[row, c].SetEmpty();
        }
    }

    private void ClearCol(int col)
    {
        for (int r = 0; r < GRID_SIZE; r++)
        {
            grid[r, col].SetEmpty();
        }
    }

    public Vector2Int GetGridPosition(Vector2 worldPosition)
    {
        // Ensure grid is initialized
        if (grid[0, 0] == null)
        {
            Debug.LogWarning("GridManager_2D: Grid not initialized for GetGridPosition.");
            return new Vector2Int(-1, -1);
        }

        // World position of the center of the grid's top-left cell (grid[0,0])
        Vector2 gridOriginWorldPos = grid[0, 0].transform.position;

        // Calculate the offset of the worldPosition (block's anchor) from the grid's top-left cell center
        Vector2 offsetWorld = worldPosition - gridOriginWorldPos;

        // Get cell dimensions (pitch includes cell size + spacing)
        Vector2 cellPitchDesignTime = GetCellPitch();

        // Get the actual runtime scale factor from the Canvas
        Canvas parentCanvas = gridParent.GetComponentInParent<Canvas>();
        float canvasScaleFactor = (parentCanvas != null) ? parentCanvas.scaleFactor : 1.0f;

        // Convert cellPitch to world space pixels by applying the canvas scale factor
        Vector2 cellPitchWorld = cellPitchDesignTime * canvasScaleFactor;

        // To make snapping "looser", we can adjust the rounding threshold.
        // Instead of snapping at 0.5, we can snap at a larger value (e.g., 0.65).
        // This makes the user drag the block further before it snaps to the next cell.
        const float snappingThreshold = 0.65f; // Snap when 65% into the next cell. Standard is 0.5.
        float roundingPoint = 1.0f - snappingThreshold;

        float normX = offsetWorld.x / cellPitchWorld.x;
        float normY = -offsetWorld.y / cellPitchWorld.y;

        // Custom rounding logic for symmetrical, "looser" snapping
        int c = (normX >= 0) ? Mathf.FloorToInt(normX + roundingPoint) : Mathf.CeilToInt(normX - roundingPoint);
        int r = (normY >= 0) ? Mathf.FloorToInt(normY + roundingPoint) : Mathf.CeilToInt(normY - roundingPoint);


        // Ensure calculated indices are within grid bounds
        if (r < 0 || r >= GRID_SIZE || c < 0 || c >= GRID_SIZE)
        {
            return new Vector2Int(-1, -1); // Return invalid position if out of bounds
        }

        return new Vector2Int(c, r); // Return as (column, row)
    }

     private void ClearSquare(int startRow, int startCol)
    {
        for (int r = startRow; r < startRow + 3; r++)
        {
            for (int c = startCol; c < startCol + 3; c++)
            {
                grid[r, c].SetEmpty();
            }
        }
    }

    public void SaveBoardData_2D(int currentScore, int currentCombo)
    {
        SaveData_2D saveData = new SaveData_2D();
        for (int r = 0; r < GRID_SIZE; r++)
        {
            for (int c = 0; c < GRID_SIZE; c++)
            {
                saveData.cellOccupiedStates[r * GRID_SIZE + c] = !grid[r, c].IsEmpty;
            }
        }
        saveData.score = currentScore;
        saveData.combo = currentCombo;

        string json = JsonUtility.ToJson(saveData);
        string path = BoardDataPath;
        File.WriteAllText(path, json);
        Debug.Log("2D Board data saved to " + path);
    }

    public (int score, int combo) LoadBoardData_2D()
    {
        int loadedScore = 0;
        int loadedCombo = 0;

        if (File.Exists(BoardDataPath))
        {
            string json = File.ReadAllText(BoardDataPath);
            SaveData_2D saveData = JsonUtility.FromJson<SaveData_2D>(json);

            // Re-initialize grid before loading states
            InitializeGrid();

            for (int r = 0; r < GRID_SIZE; r++)
            {
                for (int c = 0; c < GRID_SIZE; c++)
                {
                    if (saveData.cellOccupiedStates[r * GRID_SIZE + c])
                    {
                        grid[r, c].SetOccupied();
                    }
                    else
                    {
                        grid[r, c].SetEmpty();
                    }
                }
            }
            loadedScore = saveData.score;
            loadedCombo = saveData.combo;
            Debug.Log("2D Board data loaded from " + BoardDataPath);
        }
        else
        {
            Debug.Log("2D Board save file not found at " + SavePaths.BoardDataPath);
        }
        return (loadedScore, loadedCombo);
    }
}

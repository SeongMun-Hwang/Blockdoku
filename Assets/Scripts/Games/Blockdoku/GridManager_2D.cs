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
    public float clearAnimationSequentialDelay = 0.05f; // Delay between sequential cell clears

    [Header("Shake Effect")]
    [Range(0f, 1f)] public float shakeDuration = 0.15f;
    [Range(0f, 100f)] public float shakeMagnitude = 10f;
    private Vector3 originalGridPos;
    private Coroutine shakeCoroutine;

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

        originalGridPos = gridParent.localPosition;
    }

    public void ShakeGrid(int combo)
    {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        
        // Dynamic magnitude: Add (combo * 0.1 + 1) to base magnitude
        float dynamicMagnitude = shakeMagnitude + (combo * 0.1f + 1f);
        shakeCoroutine = StartCoroutine(ShakeCoroutine(dynamicMagnitude));
    }

    private IEnumerator ShakeCoroutine(float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            gridParent.localPosition = originalGridPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        gridParent.localPosition = originalGridPos;
        shakeCoroutine = null;
    }

    public Color subgridBorderColor = Color.black;
    public float subgridBorderWidth = 5f;

    [System.Serializable]
    public class SaveData_2D
    {
        public bool[] cellOccupiedStates = new bool[GRID_SIZE * GRID_SIZE];
        public List<SerializableColor> cellColors = new List<SerializableColor>(GRID_SIZE * GRID_SIZE); // Changed to List for better JsonUtility serialization
        public int score;
        public int combo;
    }

    public void InitializeGrid()
    {
        // Clear existing grid cells if any
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        for (int r = 0; r < GRID_SIZE; r++)
        {
            for (int c = 0; c < GRID_SIZE; c++)
            {
                GameObject cellGO = Instantiate(cellPrefab, gridParent);
                cellGO.name = $"Cell_{r}_{c}";
                grid[r, c] = cellGO.AddComponent<Cell_2D>();
                grid[r, c].Initialize(r, c, true);

                CreateSubgridBorders(cellGO, r, c);
            }
        }
    }

    private void CreateSubgridBorders(GameObject cellGO, int r, int c)
    {
        // Add borders for the 3x3 grid visualization
        if (r % 3 == 2 && r < GRID_SIZE - 1)
        {
            CreateBorder(cellGO.transform, "HorizontalBorder", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, subgridBorderWidth));
        }
        if (c % 3 == 2 && c < GRID_SIZE - 1)
        {
            CreateBorder(cellGO.transform, "VerticalBorder", new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(subgridBorderWidth, 0));
        }
    }

    private void CreateBorder(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta)
    {
        GameObject borderGO = new GameObject(name);
        borderGO.transform.SetParent(parent, false);
        Image borderImage = borderGO.AddComponent<Image>();
        borderImage.color = subgridBorderColor;
        RectTransform rt = borderGO.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = sizeDelta;
    }


    public bool IsValidPlacementForAll(List<Vector2Int> blockShape)
    {
        for (int r = 0; r < GRID_SIZE; r++)
        {
            for (int c = 0; c < GRID_SIZE; c++)
            {
                if (IsValidPlacement(new Vector2Int(c, r), blockShape))
                    return true;
            }
        }
        return false;
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

    public int PlaceBlock(Vector2Int gridPosition, List<Vector2Int> blockShape, Color blockColor)
    {
        // First, clear any ongoing clear prediction blinks
        StopClearPredictBlink();

        foreach (var pos in blockShape)
        {
            int r = gridPosition.y - pos.y; // Corrected: subtract pos.y
            int c = gridPosition.x + pos.x;
            grid[r, c].SetOccupied(blockColor); // Pass the block's color
        }

        int clearCount = CheckForCompletedLines();

        // Ensure game data is saved after a block is placed.
        if (GameManager_2D.Instance != null)
        {
            GameManager_2D.Instance.SaveGameData();
        }

        return clearCount;
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


    private HashSet<Vector2Int> GetCompletedCellPositions(bool[,] occupiedState)
    {
        HashSet<Vector2Int> completedPositions = new HashSet<Vector2Int>();
        List<int> completedRows = new List<int>();
        List<int> completedCols = new List<int>();

        // Check rows and columns
        for (int i = 0; i < GRID_SIZE; i++)
        {
            bool rowComplete = true;
            bool colComplete = true;
            for (int j = 0; j < GRID_SIZE; j++)
            {
                if (!occupiedState[i, j]) rowComplete = false;
                if (!occupiedState[j, i]) colComplete = false;
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
                        if (!occupiedState[i, j]) squareComplete = false;
                    }
                }
                if (squareComplete)
                {
                    for (int i = r; i < r + 3; i++)
                        for (int j = c; j < c + 3; j++)
                            completedPositions.Add(new Vector2Int(j, i));
                }
            }
        }

        // Add cells from completed rows and columns
        foreach (var row in completedRows)
            for (int c = 0; c < GRID_SIZE; c++) completedPositions.Add(new Vector2Int(c, row));
        
        foreach (var col in completedCols)
            for (int r = 0; r < GRID_SIZE; r++) completedPositions.Add(new Vector2Int(col, r));

        return completedPositions;
    }

    private HashSet<Cell_2D> GetPotentialClearedCells(Vector2Int gridPosition, List<Vector2Int> blockShape)
    {
        bool[,] tempGridOccupied = new bool[GRID_SIZE, GRID_SIZE];
        for (int r = 0; r < GRID_SIZE; r++)
            for (int c = 0; c < GRID_SIZE; c++)
                tempGridOccupied[r, c] = !grid[r, c].IsEmpty;

        foreach (var pos in blockShape)
        {
            int r = gridPosition.y - pos.y;
            int c = gridPosition.x + pos.x;
            if (r >= 0 && r < GRID_SIZE && c >= 0 && c < GRID_SIZE)
                tempGridOccupied[r, c] = true;
        }

        HashSet<Vector2Int> completedPositions = GetCompletedCellPositions(tempGridOccupied);
        HashSet<Cell_2D> potentialClearedCells = new HashSet<Cell_2D>();
        foreach (var pos in completedPositions)
        {
            potentialClearedCells.Add(grid[pos.y, pos.x]);
        }

        return potentialClearedCells;
    }


    private int CalculateLinesClearedCount(bool[,] occupiedState)
    {
        int count = 0;

        // Check rows and columns
        for (int i = 0; i < GRID_SIZE; i++)
        {
            bool rowComplete = true;
            bool colComplete = true;
            for (int j = 0; j < GRID_SIZE; j++)
            {
                if (!occupiedState[i, j]) rowComplete = false;
                if (!occupiedState[j, i]) colComplete = false;
            }
            if (rowComplete) count++;
            if (colComplete) count++;
        }

        // Check 3x3 squares
        for (int r = 0; r < GRID_SIZE; r += 3)
        {
            for (int c = 0; c < GRID_SIZE; c += 3)
            {
                bool squareComplete = true;
                for (int i = r; i < r + 3; i++)
                    for (int j = c; j < c + 3; j++)
                        if (!occupiedState[i, j]) squareComplete = false;
                
                if (squareComplete) count++;
            }
        }

        return count;
    }

    public int CheckForCompletedLines()
    {
        bool[,] currentGridOccupied = new bool[GRID_SIZE, GRID_SIZE];
        for (int r = 0; r < GRID_SIZE; r++)
            for (int c = 0; c < GRID_SIZE; c++)
                currentGridOccupied[r, c] = !grid[r, c].IsEmpty;

        HashSet<Vector2Int> completedPositions = GetCompletedCellPositions(currentGridOccupied);
        
        if (completedPositions.Count == 0)
        {
            GameManager_2D.Instance.combo = 0;
            CheckSymmetry(); 
            return 0;
        }

        HashSet<Cell_2D> cellsToClear = new HashSet<Cell_2D>();
        foreach (var pos in completedPositions)
            cellsToClear.Add(grid[pos.y, pos.x]);

        int linesClearedCount = CalculateLinesClearedCount(currentGridOccupied);
        bool isFullClear = false;

        // Check for Full Clear (Entire grid cleared)
        int totalOccupiedBeforeClear = 0;
        for (int r = 0; r < GRID_SIZE; r++)
            for (int c = 0; c < GRID_SIZE; c++)
                if (!grid[r, c].IsEmpty) totalOccupiedBeforeClear++;

        isFullClear = totalOccupiedBeforeClear == cellsToClear.Count;

        StartCoroutine(SequentialClear(cellsToClear));

        // Update global combo and score
        GameManager_2D.Instance.combo += linesClearedCount;
        GameManager_2D.Instance.AddScore(linesClearedCount * 9);      

        if (isFullClear)
            GameManager_2D.Instance.AddSpecialScore(100, "FULL CLEAR");

        if (AudioManager_2D.Instance != null) 
            AudioManager_2D.Instance.PlayBlockDestroyAudio(GameManager_2D.Instance.combo);

        // Symmetry Check (Skip if Full Clear occurred)
        if (!isFullClear) CheckSymmetry();

        return cellsToClear.Count;
    }

    private void CheckSymmetry()
    {
        bool hSym = true, vSym = true, d1Sym = true, d2Sym = true;
        int occupiedCount = 0;

        for (int r = 0; r < GRID_SIZE; r++)
        {
            for (int c = 0; c < GRID_SIZE; c++)
            {
                if (grid[r, c].IsEmpty) continue;
                occupiedCount++;

                // Horizontal Symmetry (r, c) == (r, 8-c)
                if (grid[r, 8 - c].IsEmpty) hSym = false;
                // Vertical Symmetry (r, c) == (8-r, c)
                if (grid[8 - r, c].IsEmpty) vSym = false;
                // Diagonal 1 (r, c) == (c, r)
                if (grid[c, r].IsEmpty) d1Sym = false;
                // Diagonal 2 (r, c) == (8-c, 8-r)
                if (grid[8 - c, 8 - r].IsEmpty) d2Sym = false;
            }
        }

        // Only reward symmetry if there are enough blocks to make it meaningful (e.g., > 5)
        if (occupiedCount > 5)
        {
            if (hSym) GameManager_2D.Instance.AddSpecialScore(20, "H-SYMMETRY");
            else if (vSym) GameManager_2D.Instance.AddSpecialScore(20, "V-SYMMETRY");
            else if (d1Sym || d2Sym) GameManager_2D.Instance.AddSpecialScore(30, "DIAG-SYMMETRY");
        }
    }

    private IEnumerator SequentialClear(HashSet<Cell_2D> cells)
    {
        List<Cell_2D> sortedCells = cells.ToList();

        // 0: Top-Bottom, 1: Bottom-Top, 2: Left-Right, 3: Right-Left
        int direction = Random.Range(0, 4);

        switch (direction)
        {
            case 0: // Top-Bottom (y: 0 to 8)
                sortedCells = sortedCells.OrderBy(c => c.gridPosition.y).ThenBy(c => c.gridPosition.x).ToList();
                break;
            case 1: // Bottom-Top (y: 8 to 0)
                sortedCells = sortedCells.OrderByDescending(c => c.gridPosition.y).ThenBy(c => c.gridPosition.x).ToList();
                break;
            case 2: // Left-Right (x: 0 to 8)
                sortedCells = sortedCells.OrderBy(c => c.gridPosition.x).ThenBy(c => c.gridPosition.y).ToList();
                break;
            case 3: // Right-Left (x: 8 to 0)
                sortedCells = sortedCells.OrderByDescending(c => c.gridPosition.x).ThenBy(c => c.gridPosition.y).ToList();
                break;
        }

        float startTime = Time.time;
        for (int i = 0; i < sortedCells.Count; i++)
        {
            sortedCells[i].TriggerClearAnimation();
            
            if (clearAnimationSequentialDelay > 0)
            {
                float targetTime = startTime + (i + 1) * clearAnimationSequentialDelay;
                while (Time.time < targetTime)
                {
                    yield return null;
                }
            }
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
            if (!previewCells.Contains(cell))
            {
                // Store the current visual color of the cell
                storedOriginalClearPredictColors[cell] = cell.cellImage.color; 
            }
        }

        while (true)
        {
            float t = Mathf.PingPong(Time.time / interval, 1.0f);
            foreach (var cell in cells)
            {
                if (cell == null || cell.gameObject == null) continue;

                if (!previewCells.Contains(cell) && storedOriginalClearPredictColors.TryGetValue(cell, out Color originalColor))
                {
                    cell.cellImage.color = Color.Lerp(originalColor, blinkColor, t);
                }
            }
            yield return null;
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
        // A lower threshold makes the block snap to the nearest cell more easily.

        float normX = offsetWorld.x / cellPitchWorld.x;
        float normY = -offsetWorld.y / cellPitchWorld.y;

        // Use standard rounding to find the nearest cell index
        int c = Mathf.RoundToInt(normX);
        int r = Mathf.RoundToInt(normY);

        return new Vector2Int(c, r); // Return raw (column, row), caller handles bounds
    }

    /// <summary>
    /// Finds the nearest valid grid position within a 1-cell radius of the target.
    /// This improves player experience by "magnetically" snapping to valid spots.
    /// </summary>
    public Vector2Int GetNearestValidPosition(Vector2 worldPosition, List<Vector2Int> blockShape)
    {
        Vector2Int basePos = GetGridPosition(worldPosition);
        
        // If the base position is already valid, return it immediately
        if (IsValidPlacement(basePos, blockShape))
        {
            return basePos;
        }

        // Search in a 1-cell radius (3x3 area) for a valid spot
        Vector2Int nearestPos = new Vector2Int(-1, -1);
        float minDistance = float.MaxValue;

        // Get cell pitch for distance threshold calculation
        Vector2 cellPitchDesignTime = GetCellPitch();
        Canvas parentCanvas = gridParent.GetComponentInParent<Canvas>();
        float canvasScaleFactor = (parentCanvas != null) ? parentCanvas.scaleFactor : 1.0f;
        Vector2 cellPitchWorld = cellPitchDesignTime * canvasScaleFactor;
        
        // Snapping threshold: only snap if within 1.5x the cell size
        float snapThreshold = cellPitchWorld.x * 1.5f;

        Vector2 gridOriginWorldPos = grid[0, 0].transform.position;

        for (int dr = -1; dr <= 1; dr++)
        {
            for (int dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0) continue; // Already checked basePos

                Vector2Int candidate = new Vector2Int(basePos.x + dc, basePos.y + dr);
                if (IsValidPlacement(candidate, blockShape))
                {
                    // Calculate world position of the candidate cell (even if anchor is out of bounds)
                    Vector2 candidateWorldPos = gridOriginWorldPos + new Vector2(candidate.x * cellPitchWorld.x, -candidate.y * cellPitchWorld.y);
                    float dist = Vector2.Distance(worldPosition, candidateWorldPos);

                    if (dist < minDistance && dist < snapThreshold)
                    {
                        minDistance = dist;
                        nearestPos = candidate;
                    }
                }
            }
        }

        return nearestPos;
    }

    public void SaveBoardData_2D(int currentScore, int currentCombo)
    {
        SaveData_2D saveData = new SaveData_2D();
        // Ensure the list is sized correctly before filling
        saveData.cellColors.Capacity = GRID_SIZE * GRID_SIZE; // Pre-allocate capacity
        for (int i = 0; i < GRID_SIZE * GRID_SIZE; i++) saveData.cellColors.Add(new SerializableColor()); // Fill with default values

        for (int r = 0; r < GRID_SIZE; r++)
        {
            for (int c = 0; c < GRID_SIZE; c++)
            {
                int index = r * GRID_SIZE + c;
                saveData.cellOccupiedStates[index] = !grid[r, c].IsEmpty;
                // Save the color only if the cell is occupied
                saveData.cellColors[index] = grid[r, c].BlockColor;
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

            // Ensure cellColors list is consistent with grid size after loading
            // If the loaded list is smaller, it means older data without color was saved
            // We should ensure the list is at least the expected size before accessing by index.
            while (saveData.cellColors.Count < GRID_SIZE * GRID_SIZE)
            {
                saveData.cellColors.Add(new SerializableColor()); // Add default colors if missing
            }


            for (int r = 0; r < GRID_SIZE; r++)
            {
                for (int c = 0; c < GRID_SIZE; c++)
                {
                    int index = r * GRID_SIZE + c;
                    if (saveData.cellOccupiedStates[index])
                    {
                        // Load and apply the saved color
                        // Use default color (e.g., Color.clear) if for some reason color data is missing for a cell
                        grid[r, c].SetOccupied(saveData.cellColors[index]);
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

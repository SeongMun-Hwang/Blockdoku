using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;

public class GridManager_2D : MonoBehaviour
{
    public static GridManager_2D Instance { get; private set; }

    [Header("Prefabs & Parents")]
    public GameObject cellPrefab;
    public Transform gridParent;
    public RectTransform symmetryEffectContainer;
    public GameObject symmetryEffectPrefab;

    [Header("Visual Settings")]
    public Color previewColor = new Color(0f, 1f, 0f, 0.5f);
    public Color clearBlinkColor = Color.cyan;
    public float clearBlinkInterval = 0.3f;
    public float clearAnimationSequentialDelay = 0.05f;
    public Sprite defaultEmptyCellSprite;
    public Sprite defaultOccupiedCellSprite;
    public Color subgridBorderColor = Color.black;
    public float subgridBorderWidth = 5f;

    [Header("Effects Settings")]
    public float symmetryEffectDuration = 0.8f;
    public float ghostStepDistance = 6f;
    [Range(0f, 1f)] public float shakeDuration = 0.15f;
    [Range(0f, 100f)] public float shakeMagnitude = 10f;

    private const int GRID_SIZE = 9;
    private Cell_2D[,] grid = new Cell_2D[GRID_SIZE, GRID_SIZE];
    private BlockdokuBoard board = new BlockdokuBoard();
    private GridLayoutGroup gridLayoutGroup;
    private Vector3 originalGridPos;
    private Coroutine shakeCoroutine;
    private List<Cell_2D> previewCells = new List<Cell_2D>();
    private HashSet<Cell_2D> currentlyBlinkingClearCells = new HashSet<Cell_2D>();
    private Dictionary<Cell_2D, Color> storedOriginalClearPredictColors = new Dictionary<Cell_2D, Color>();
    private Coroutine clearPredictBlinkCoroutine;

    [System.Serializable]
    public class SaveData_2D
    {
        public bool[] cellOccupiedStates = new bool[GRID_SIZE * GRID_SIZE];
        public List<SerializableColor> cellColors = new List<SerializableColor>(GRID_SIZE * GRID_SIZE);
        public int score;
        public int combo;
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gridLayoutGroup = gridParent.GetComponent<GridLayoutGroup>();
        originalGridPos = gridParent.localPosition;
    }

    public void InitializeGrid()
    {
        foreach (Transform child in gridParent) Destroy(child.gameObject);

        for (int r = 0; r < GRID_SIZE; r++)
        {
            for (int c = 0; c < GRID_SIZE; c++)
            {
                GameObject cellGO = Instantiate(cellPrefab, gridParent);
                cellGO.name = $"Cell_{r}_{c}";
                grid[r, c] = cellGO.AddComponent<Cell_2D>();
                grid[r, c].Initialize(r, c, true);
                board.SetCell(r, c, false, Color.clear);
                CreateSubgridBorders(cellGO, r, c);
            }
        }
    }

    private void CreateSubgridBorders(GameObject cellGO, int r, int c)
    {
        if (r % 3 == 2 && r < GRID_SIZE - 1)
            CreateBorder(cellGO.transform, "HorizontalBorder", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, subgridBorderWidth));
        if (c % 3 == 2 && c < GRID_SIZE - 1)
            CreateBorder(cellGO.transform, "VerticalBorder", new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(subgridBorderWidth, 0));
    }

    private void CreateBorder(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta)
    {
        GameObject borderGO = new GameObject(name);
        borderGO.transform.SetParent(parent, false);
        Image borderImage = borderGO.AddComponent<Image>();
        borderImage.color = subgridBorderColor;
        RectTransform rt = borderGO.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.anchoredPosition = Vector2.zero; rt.sizeDelta = sizeDelta;
    }

    public bool IsValidPlacementForAll(List<Vector2Int> blockShape) => board.IsValidPlacementForAll(blockShape);
    public bool IsValidPlacement(Vector2Int gridPosition, List<Vector2Int> blockShape) => board.IsValidPlacement(gridPosition, blockShape);

    public int PlaceBlock(Vector2Int gridPosition, List<Vector2Int> blockShape, Color blockColor)
    {
        StopClearPredictBlink();
        foreach (var pos in blockShape)
        {
            int r = gridPosition.y - pos.y;
            int c = gridPosition.x + pos.x;
            grid[r, c].SetOccupied(blockColor);
            board.SetCell(r, c, true, blockColor);
        }

        int clearCount = CheckForCompletedLines();
        GameManager_2D.Instance.SaveGameData();
        return clearCount;
    }

    public void ShowPreview(Vector2Int gridPosition, List<Vector2Int> blockShape)
    {
        ClearPreview();
        if (board.IsValidPlacement(gridPosition, blockShape))
        {
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

            HashSet<Cell_2D> potentialClearedCells = GetPotentialClearedCells(gridPosition, blockShape);
            if (potentialClearedCells.Count > 0)
            {
                currentlyBlinkingClearCells = potentialClearedCells;
                clearPredictBlinkCoroutine = StartCoroutine(ClearPredictBlink(currentlyBlinkingClearCells, clearBlinkColor, clearBlinkInterval));
            }
        }
    }

    public void ClearPreview()
    {
        foreach (var cell in previewCells) cell.ClearPreview();
        previewCells.Clear();
        StopClearPredictBlink();
    }

    private void StopClearPredictBlink()
    {
        if (clearPredictBlinkCoroutine != null) { StopCoroutine(clearPredictBlinkCoroutine); clearPredictBlinkCoroutine = null; }
        foreach (var cell in currentlyBlinkingClearCells)
            if (cell != null && storedOriginalClearPredictColors.ContainsKey(cell))
                cell.cellImage.color = storedOriginalClearPredictColors[cell];
        currentlyBlinkingClearCells.Clear();
        storedOriginalClearPredictColors.Clear();
    }

    public HashSet<Cell_2D> GetPotentialClearedCells(Vector2Int gridPosition, List<Vector2Int> blockShape)
    {
        bool[,] tempState = new bool[GRID_SIZE, GRID_SIZE];
        for (int r = 0; r < GRID_SIZE; r++)
            for (int c = 0; c < GRID_SIZE; c++) tempState[r, c] = !grid[r, c].IsEmpty;

        foreach (var pos in blockShape)
        {
            int r = gridPosition.y - pos.y;
            int c = gridPosition.x + pos.x;
            if (r >= 0 && r < GRID_SIZE && c >= 0 && c < GRID_SIZE) tempState[r, c] = true;
        }

        HashSet<Vector2Int> positions = board.GetCompletedPositions(tempState);
        HashSet<Cell_2D> cells = new HashSet<Cell_2D>();
        foreach (var pos in positions) cells.Add(grid[pos.y, pos.x]);
        return cells;
    }

    public int CheckForCompletedLines()
    {
        HashSet<Vector2Int> completedPositions = board.GetCompletedPositions();
        if (completedPositions.Count == 0)
        {
            if (!CheckSymmetry()) GameManager_2D.Instance.combo = 0;
            return 0;
        }

        HashSet<Cell_2D> cellsToClear = new HashSet<Cell_2D>();
        foreach (var pos in completedPositions)
        {
            cellsToClear.Add(grid[pos.y, pos.x]);
            board.SetCell(pos.y, pos.x, false, Color.clear);
        }

        List<Color> clearColors = cellsToClear.Select(c => c.BlockColor).Distinct().ToList();
        
        int linesClearedCount = CalculateLinesClearedCount();
        bool isFullClear = board.IsBoardEmpty();

        StartCoroutine(SequentialClear(cellsToClear));
        GameManager_2D.Instance.combo += linesClearedCount;

        if (isFullClear)
        {
            GameManager_2D.Instance.combo++;
            GameManager_2D.Instance.AddSpecialScore(100, "FULL CLEAR");
            PlayFullClearAnimation(clearColors);
        }

        GameManager_2D.Instance.AddScore(linesClearedCount * 9);
        if (AudioManager_2D.Instance != null) AudioManager_2D.Instance.PlayBlockDestroyAudio(GameManager_2D.Instance.combo);

        if (!isFullClear) CheckSymmetry();
        return cellsToClear.Count;
    }

    private int CalculateLinesClearedCount()
    {
        int count = 0;
        bool[,] state = new bool[GRID_SIZE, GRID_SIZE];
        for (int r = 0; r < GRID_SIZE; r++)
            for (int c = 0; c < GRID_SIZE; c++) state[r, c] = !grid[r, c].IsEmpty;

        for (int i = 0; i < GRID_SIZE; i++)
        {
            bool rC = true, cC = true;
            for (int j = 0; j < GRID_SIZE; j++) { if (!state[i, j]) rC = false; if (!state[j, i]) cC = false; }
            if (rC) count++; if (cC) count++;
        }
        for (int r = 0; r < GRID_SIZE; r += 3)
            for (int c = 0; c < GRID_SIZE; c += 3)
            {
                bool sC = true;
                for (int i = r; i < r + 3; i++)
                    for (int j = c; j < c + 3; j++) if (!state[i, j]) sC = false;
                if (sC) count++;
            }
        return count;
    }

    public void PlayFullClearAnimation(List<Color> sourceColors = null)
    {
        if (symmetryEffectPrefab == null || symmetryEffectContainer == null) return;
        if (GameManager_2D.Instance.uiManager != null) GameManager_2D.Instance.uiManager.Vibrate();
        ShakeGrid(GameManager_2D.Instance.combo + 5);

        List<Color> colors = GetRandomActiveColors(2);
        if (sourceColors != null && sourceColors.Count > 0)
        {
            colors[0] = sourceColors[Random.Range(0, sourceColors.Count)];
            colors[1] = sourceColors[Random.Range(0, sourceColors.Count)];
        }

        float w = symmetryEffectContainer.rect.width / 2f;
        float h = symmetryEffectContainer.rect.height / 2f;
        Vector3[] points = { new Vector3(-w, h), new Vector3(0, h), new Vector3(w, h), new Vector3(w, 0), new Vector3(w, -h), new Vector3(0, -h), new Vector3(-w, -h), new Vector3(-w, 0) };

        int start1 = Random.Range(0, 8);
        int start2 = (start1 + 4) % 8;
        bool isCW = Random.value > 0.5f;

        Vector3[] path1 = new Vector3[9], path2 = new Vector3[9];
        for (int i = 0; i <= 8; i++)
        {
            path1[i] = points[isCW ? (start1 + i) % 8 : (start1 - i + 8) % 8];
            path2[i] = points[isCW ? (start2 + i) % 8 : (start2 - i + 8) % 8];
        }
        LaunchSymmetryEffect(path1, colors[0], symmetryEffectDuration * 2.5f);
        LaunchSymmetryEffect(path2, colors[1], symmetryEffectDuration * 2.5f);
    }

    public void PlaySymmetryAnimation(string type)
    {
        if (symmetryEffectPrefab == null || symmetryEffectContainer == null) return;
        if (GameManager_2D.Instance.uiManager != null) GameManager_2D.Instance.uiManager.Vibrate();
        var colors = GetRandomActiveColors(2);
        bool isReverse = Random.value > 0.5f;
        Vector3[] p1, p2;
        GetSymmetryPaths(type, isReverse, out p1, out p2);
        LaunchSymmetryEffect(p1, colors[0]);
        LaunchSymmetryEffect(p2, colors[1]);
    }

    private void LaunchSymmetryEffect(Vector3[] path, Color color, float? overrideDuration = null)
    {
        GameObject effectGO = Instantiate(symmetryEffectPrefab, symmetryEffectContainer);
        RectTransform rt = effectGO.GetComponent<RectTransform>();
        Image img = effectGO.GetComponent<Image>();
        if (img != null) img.color = color;
        rt.localPosition = path[0];

        float totalDist = 0;
        for (int i = 0; i < path.Length - 1; i++) totalDist += Vector3.Distance(path[i], path[i + 1]);

        float duration = overrideDuration ?? symmetryEffectDuration;
        Sequence seq = DOTween.Sequence();
        Vector2 lastGhostPos = path[0];

        for (int i = 0; i < path.Length - 1; i++)
        {
            Vector3 end = path[i + 1];
            float segDist = Vector3.Distance(path[i], end);
            seq.Append(rt.DOLocalMove(end, (segDist / totalDist) * duration).SetEase(Ease.Linear).OnUpdate(() => {
                if (Vector2.Distance(rt.localPosition, lastGhostPos) >= ghostStepDistance)
                {
                    CreateTrailGhost(rt.localPosition, color);
                    lastGhostPos = rt.localPosition;
                }
            }));
        }
        seq.SetEase(Ease.OutQuad).OnComplete(() => rt.DOScale(0f, 0.2f).OnComplete(() => Destroy(effectGO)));
    }

    private void CreateTrailGhost(Vector2 pos, Color color)
    {
        GameObject ghostGO = Instantiate(symmetryEffectPrefab, symmetryEffectContainer);
        RectTransform rt = ghostGO.GetComponent<RectTransform>();
        Image img = ghostGO.GetComponent<Image>();
        rt.localPosition = pos;
        if (img != null) img.color = color;
        img.DOFade(0f, 0.4f);
        rt.DOScale(0f, 0.4f).SetEase(Ease.InQuad).OnComplete(() => Destroy(ghostGO));
    }

    private List<Color> GetRandomActiveColors(int count)
    {
        var activeColors = grid.Cast<Cell_2D>().Where(c => !c.IsEmpty).Select(c => c.BlockColor).Distinct().ToList();
        if (activeColors.Count == 0) return Enumerable.Repeat(Color.white, count).ToList();
        return Enumerable.Range(0, count).Select(_ => activeColors[Random.Range(0, activeColors.Count)]).ToList();
    }

    private void GetSymmetryPaths(string type, bool isRev, out Vector3[] p1, out Vector3[] p2)
    {
        float w = symmetryEffectContainer.rect.width / 2f, h = symmetryEffectContainer.rect.height / 2f;
        Vector3 TL = new Vector3(-w, h), TR = new Vector3(w, h), BL = new Vector3(-w, -h), BR = new Vector3(w, -h);
        Vector3 TC = new Vector3(0, h), BC = new Vector3(0, -h), LC = new Vector3(-w, 0), RC = new Vector3(w, 0);

        if (type == "H") { p1 = isRev ? new[] { BC, BR, TR, TC } : new[] { TC, TR, BR, BC }; p2 = isRev ? new[] { BC, BL, TL, TC } : new[] { TC, TL, BL, BC }; }
        else if (type == "V") { p1 = isRev ? new[] { RC, TR, TL, LC } : new[] { LC, TL, TR, RC }; p2 = isRev ? new[] { RC, BR, BL, LC } : new[] { LC, BL, BR, RC }; }
        else { p1 = isRev ? new[] { BR, TR, TL } : new[] { TL, TR, BR }; p2 = isRev ? new[] { BR, BL, TL } : new[] { TL, BL, BR }; }
    }

    private bool CheckSymmetry()
    {
        int occupiedCount;
        SymmetryType type = board.CheckSymmetry(out occupiedCount);
        if (type != SymmetryType.None)
        {
            GameManager_2D.Instance.combo++;
            int bonus = 30 + (occupiedCount * 3);
            string msg = type == SymmetryType.Horizontal ? "H-SYMMETRY" : type == SymmetryType.Vertical ? "V-SYMMETRY" : "DIAG-SYMMETRY";
            GameManager_2D.Instance.AddSpecialScore(bonus, msg);
            PlaySymmetryAnimation(type == SymmetryType.Horizontal ? "H" : type == SymmetryType.Vertical ? "V" : "D");
            return true;
        }
        return false;
    }

    public void ShakeGrid(int combo)
    {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        float dynamicMagnitude = shakeMagnitude + (combo * 0.5f);
        shakeCoroutine = StartCoroutine(ShakeCoroutine(dynamicMagnitude));
    }

    private IEnumerator ShakeCoroutine(float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            gridParent.localPosition = originalGridPos + (Vector3)Random.insideUnitCircle * magnitude;
            elapsed += Time.deltaTime; yield return null;
        }
        gridParent.localPosition = originalGridPos;
        shakeCoroutine = null;
    }

    private IEnumerator SequentialClear(HashSet<Cell_2D> cells)
    {
        List<Cell_2D> sorted = cells.ToList();
        int dir = Random.Range(0, 4);
        if (dir == 0) sorted = sorted.OrderBy(c => c.gridPosition.y).ToList();
        else if (dir == 1) sorted = sorted.OrderByDescending(c => c.gridPosition.y).ToList();
        else if (dir == 2) sorted = sorted.OrderBy(c => c.gridPosition.x).ToList();
        else sorted = sorted.OrderByDescending(c => c.gridPosition.x).ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].TriggerClearAnimation();
            yield return new WaitForSeconds(clearAnimationSequentialDelay);
        }
    }

    public Vector2Int GetGridPosition(Vector2 worldPos)
    {
        Vector3 localPos = gridParent.InverseTransformPoint(worldPos);
        Vector3 originLocalPos = gridParent.InverseTransformPoint(grid[0, 0].transform.position);
        Vector2 offset = (Vector2)(localPos - originLocalPos);
        Vector2 pitch = GetCellPitch();
        return new Vector2Int(Mathf.RoundToInt(offset.x / pitch.x), Mathf.RoundToInt(-offset.y / pitch.y));
    }

    public Vector2 GetCellPitch() => gridLayoutGroup.cellSize + gridLayoutGroup.spacing;
    public Vector2 GetCellSize() => gridLayoutGroup.cellSize;

    public Vector2Int GetNearestValidPosition(Vector2 worldPosition, List<Vector2Int> blockShape)
    {
        Vector2Int basePos = GetGridPosition(worldPosition);
        if (board.IsValidPlacement(basePos, blockShape)) return basePos;

        Vector2Int nearestPos = new Vector2Int(-1, -1);
        float minDistance = float.MaxValue;
        Vector2 cellPitch = GetCellPitch();
        float cellWidthWorld = cellPitch.x * gridParent.lossyScale.x;
        float snapThreshold = cellWidthWorld * 1.5f;

        for (int dr = -1; dr <= 1; dr++)
            for (int dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0) continue;
                Vector2Int candidate = new Vector2Int(basePos.x + dc, basePos.y + dr);
                if (candidate.x >= 0 && candidate.x < GRID_SIZE && candidate.y >= 0 && candidate.y < GRID_SIZE)
                    if (board.IsValidPlacement(candidate, blockShape))
                    {
                        Vector2 candidateWorldPos = grid[candidate.y, candidate.x].transform.position;
                        float dist = Vector2.Distance(worldPosition, candidateWorldPos);
                        if (dist < minDistance && dist < snapThreshold) { minDistance = dist; nearestPos = candidate; }
                    }
            }
        return nearestPos;
    }

    private IEnumerator ClearPredictBlink(HashSet<Cell_2D> cells, Color blinkColor, float interval)
    {
        storedOriginalClearPredictColors.Clear();
        foreach (var cell in cells)
            if (!previewCells.Contains(cell)) storedOriginalClearPredictColors[cell] = cell.cellImage.color;

        while (true)
        {
            float t = Mathf.PingPong(Time.time / interval, 1.0f);
            foreach (var cell in cells)
                if (cell != null && !previewCells.Contains(cell) && storedOriginalClearPredictColors.TryGetValue(cell, out Color originalColor))
                    cell.cellImage.color = Color.Lerp(originalColor, blinkColor, t);
            yield return null;
        }
    }

    public void SaveBoardData_2D(int score, int combo)
    {
        SaveData_2D data = new SaveData_2D { score = score, combo = combo };
        data.cellColors = new List<SerializableColor>(GRID_SIZE * GRID_SIZE);
        for (int i = 0; i < GRID_SIZE * GRID_SIZE; i++) data.cellColors.Add(new SerializableColor());

        for (int r = 0; r < GRID_SIZE; r++)
            for (int c = 0; c < GRID_SIZE; c++)
            {
                int idx = r * GRID_SIZE + c;
                data.cellOccupiedStates[idx] = !grid[r, c].IsEmpty;
                data.cellColors[idx] = grid[r, c].BlockColor;
            }
        SaveManager.SaveData("save.json", data);
    }

    public (int score, int combo) LoadBoardData_2D()
    {
        if (!SaveManager.Exists("save.json")) return (0, 0);
        SaveData_2D data = SaveManager.LoadData<SaveData_2D>("save.json");
        InitializeGrid();
        for (int r = 0; r < GRID_SIZE; r++)
            for (int c = 0; c < GRID_SIZE; c++)
            {
                int idx = r * GRID_SIZE + c;
                if (data.cellOccupiedStates[idx])
                {
                    grid[r, c].SetOccupied(data.cellColors[idx]);
                    board.SetCell(r, c, true, data.cellColors[idx]);
                }
            }
        return (data.score, data.combo);
    }
}

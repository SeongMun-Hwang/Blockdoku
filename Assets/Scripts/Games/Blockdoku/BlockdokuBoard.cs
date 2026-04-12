using UnityEngine;
using System.Collections.Generic;

public class BlockdokuBoard
{
    public const int GRID_SIZE = 9;
    private bool[,] occupied = new bool[GRID_SIZE, GRID_SIZE];
    private Color[,] colors = new Color[GRID_SIZE, GRID_SIZE];

    public bool IsEmpty(int r, int c) => !occupied[r, c];
    public Color GetColor(int r, int c) => colors[r, c];

    public void SetCell(int r, int c, bool isOccupied, Color color)
    {
        occupied[r, c] = isOccupied;
        colors[r, c] = isOccupied ? color : Color.clear;
    }

    public bool IsBoardEmpty()
    {
        for (int r = 0; r < GRID_SIZE; r++)
            for (int c = 0; c < GRID_SIZE; c++)
                if (occupied[r, c]) return false;
        return true;
    }

    public bool IsValidPlacement(Vector2Int gridPosition, List<Vector2Int> blockShape)
    {
        foreach (var pos in blockShape)
        {
            int r = gridPosition.y - pos.y;
            int c = gridPosition.x + pos.x;

            if (r < 0 || r >= GRID_SIZE || c < 0 || c >= GRID_SIZE) return false;
            if (occupied[r, c]) return false;
        }
        return true;
    }

    public bool IsValidPlacementForAll(List<Vector2Int> blockShape)
    {
        for (int r = 0; r < GRID_SIZE; r++)
            for (int c = 0; c < GRID_SIZE; c++)
                if (IsValidPlacement(new Vector2Int(c, r), blockShape)) return true;
        return false;
    }

    public HashSet<Vector2Int> GetCompletedPositions(bool[,] tempState = null)
    {
        bool[,] state = tempState ?? occupied;
        HashSet<Vector2Int> completed = new HashSet<Vector2Int>();
        
        List<int> rows = new List<int>();
        List<int> cols = new List<int>();

        for (int i = 0; i < GRID_SIZE; i++)
        {
            bool rowComplete = true;
            bool colComplete = true;
            for (int j = 0; j < GRID_SIZE; j++)
            {
                if (!state[i, j]) rowComplete = false;
                if (!state[j, i]) colComplete = false;
            }
            if (rowComplete) rows.Add(i);
            if (colComplete) cols.Add(i);
        }

        for (int r = 0; r < GRID_SIZE; r += 3)
        {
            for (int c = 0; c < GRID_SIZE; c += 3)
            {
                bool squareComplete = true;
                for (int i = r; i < r + 3; i++)
                    for (int j = c; j < c + 3; j++)
                        if (!state[i, j]) squareComplete = false;

                if (squareComplete)
                {
                    for (int i = r; i < r + 3; i++)
                        for (int j = c; j < c + 3; j++)
                            completed.Add(new Vector2Int(j, i));
                }
            }
        }

        foreach (int r in rows) for (int c = 0; c < GRID_SIZE; c++) completed.Add(new Vector2Int(c, r));
        foreach (int c in cols) for (int r = 0; r < GRID_SIZE; r++) completed.Add(new Vector2Int(c, r));

        return completed;
    }

    public SymmetryType CheckSymmetry(out int occupiedCount)
    {
        bool h = true, v = true, d1 = true, d2 = true;
        occupiedCount = 0;

        for (int r = 0; r < GRID_SIZE; r++)
        {
            for (int c = 0; c < GRID_SIZE; c++)
            {
                if (!occupied[r, c]) continue;
                occupiedCount++;

                if (!occupied[r, 8 - c]) h = false;
                if (!occupied[8 - r, c]) v = false;
                if (!occupied[c, r]) d1 = false;
                if (!occupied[8 - c, 8 - r]) d2 = false;
            }
        }

        if (occupiedCount == 0) return SymmetryType.None;
        if (h) return SymmetryType.Horizontal;
        if (v) return SymmetryType.Vertical;
        if (d1 || d2) return SymmetryType.Diagonal;
        return SymmetryType.None;
    }
}

public enum SymmetryType { None, Horizontal, Vertical, Diagonal }

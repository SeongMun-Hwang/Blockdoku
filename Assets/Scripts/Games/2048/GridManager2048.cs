using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Games._2048
{
    public class GridManager2048 : MonoBehaviour
    {
        public static GridManager2048 Instance { get; private set; }

        public GameObject cellPrefab;
        public GameObject tilePrefab;
        public RectTransform gridParent;
        public int size = 4;

        private Cell2048[,] cells;
        private List<Tile2048> tiles = new List<Tile2048>();

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void InitializeGrid()
        {
            foreach (Transform child in gridParent)
            {
                Destroy(child.gameObject);
            }

            cells = new Cell2048[size, size];
            tiles.Clear();

            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    GameObject cellGO = Instantiate(cellPrefab, gridParent);
                    Cell2048 cell = cellGO.GetComponent<Cell2048>();
                    cell.Initialize(r, c);
                    cells[r, c] = cell;
                }
            }

            SpawnTile();
            SpawnTile();
        }

        public void SpawnTile()
        {
            List<Cell2048> emptyCells = new List<Cell2048>();
            foreach (var cell in cells)
            {
                if (!cell.IsOccupied) emptyCells.Add(cell);
            }

            if (emptyCells.Count > 0)
            {
                Cell2048 randomCell = emptyCells[Random.Range(0, emptyCells.Count)];
                GameObject tileGO = Instantiate(tilePrefab, gridParent);
                tileGO.transform.position = randomCell.transform.position;
                Tile2048 tile = tileGO.GetComponent<Tile2048>();
                
                int value = Random.value < 0.9f ? 2 : 4;
                tile.Initialize(value);
                tile.PlaySpawnAnimation();
                randomCell.SetTile(tile);
                tiles.Add(tile);
            }
        }

        public bool Move(MoveDirection direction)
        {
            bool moved = false;
            
            // Reset merged flags
            foreach (var cell in cells)
            {
                if (cell.currentTile != null) cell.currentTile.mergedThisTurn = false;
            }

            // Define traversal order based on direction
            int[] rows = Enumerable.Range(0, size).ToArray();
            int[] cols = Enumerable.Range(0, size).ToArray();

            if (direction == MoveDirection.Down) rows = rows.Reverse().ToArray();
            if (direction == MoveDirection.Right) cols = cols.Reverse().ToArray();

            foreach (int r in rows)
            {
                foreach (int c in cols)
                {
                    Cell2048 currentCell = cells[r, c];
                    if (!currentCell.IsOccupied) continue;

                    Cell2048 targetCell = FindTarget(currentCell, direction);
                    if (targetCell != null)
                    {
                        Tile2048 currentTile = currentCell.currentTile;
                        if (targetCell.IsOccupied && targetCell.currentTile.Value == currentTile.Value && !targetCell.currentTile.mergedThisTurn)
                        {
                            // Merge
                            currentTile.MergeInto(targetCell);
                            tiles.Remove(currentTile);
                            moved = true;
                            
                            GameManager2048.Instance.AddScore(targetCell.currentTile.Value * 2);
                            
                            if (AudioManager_2D.Instance != null)
                                AudioManager_2D.Instance.PlayBlockThudAudio();
                        }
                        else if (!targetCell.IsOccupied)
                        {
                            // Move
                            currentTile.MoveTo(targetCell);
                            moved = true;
                        }
                    }
                }
            }

            return moved;
        }

        private Cell2048 FindTarget(Cell2048 startCell, MoveDirection direction)
        {
            Cell2048 furthestEmptyCell = null;
            int r = startCell.gridPosition.y;
            int c = startCell.gridPosition.x;

            int dr = 0, dc = 0;
            switch (direction)
            {
                case MoveDirection.Up: dr = -1; break;
                case MoveDirection.Down: dr = 1; break;
                case MoveDirection.Left: dc = -1; break;
                case MoveDirection.Right: dc = 1; break;
            }

            int nr = r + dr;
            int nc = c + dc;

            while (nr >= 0 && nr < size && nc >= 0 && nc < size)
            {
                Cell2048 nextCell = cells[nr, nc];
                if (!nextCell.IsOccupied)
                {
                    furthestEmptyCell = nextCell;
                }
                else
                {
                    // If occupied, return this cell as a potential merge target
                    if (nextCell.currentTile.Value == startCell.currentTile.Value && !nextCell.currentTile.mergedThisTurn)
                    {
                        return nextCell;
                    }
                    break;
                }
                nr += dr;
                nc += dc;
            }

            return furthestEmptyCell;
        }

        public bool CanMove()
        {
            foreach (var cell in cells)
            {
                if (!cell.IsOccupied) return true;

                int r = cell.gridPosition.y;
                int c = cell.gridPosition.x;

                // Check adjacent cells for merges
                int[] dr = { -1, 1, 0, 0 };
                int[] dc = { 0, 0, -1, 1 };

                for (int i = 0; i < 4; i++)
                {
                    int nr = r + dr[i];
                    int nc = c + dc[i];

                    if (nr >= 0 && nr < size && nc >= 0 && nc < size)
                    {
                        if (cells[nr, nc].currentTile.Value == cell.currentTile.Value)
                            return true;
                    }
                }
            }
            return false;
        }
    }
}

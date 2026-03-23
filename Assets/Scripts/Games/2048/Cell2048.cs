using UnityEngine;

namespace Games._2048
{
    public class Cell2048 : BaseCell
    {
        public Tile2048 currentTile;

        public bool IsOccupied => currentTile != null;

        public void SetTile(Tile2048 tile)
        {
            currentTile = tile;
            if (tile != null)
            {
                tile.cell = this;
            }
        }

        public void ClearTile()
        {
            currentTile = null;
        }
    }
}

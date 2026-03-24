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
                tile.transform.SetParent(this.transform);
                // DOTween 이동 전/후에 위치를 맞추기 위해 localPosition 초기화는 상황에 맞게 조절
            }
        }

        public void ClearTile()
        {
            currentTile = null;
        }
    }
}

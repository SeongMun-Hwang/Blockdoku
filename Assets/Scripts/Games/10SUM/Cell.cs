using UnityEngine;

public class Cell : MonoBehaviour
{
    public Vector2Int gridPosition;

    public void Initialize(int row, int col)
    {
        gridPosition = new Vector2Int(col, row);
    }
}

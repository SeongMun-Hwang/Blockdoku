using UnityEngine;

public class BaseCell : MonoBehaviour
{
    public Vector2Int gridPosition;

    public virtual void Initialize(int row, int col)
    {
        gridPosition = new Vector2Int(col, row);
    }
}

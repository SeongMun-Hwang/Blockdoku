using UnityEngine;

public class BaseCell : MonoBehaviour
{
    public Vector2Int gridPosition;
    public bool IsEmpty { get; protected set; }

    public virtual void Initialize(int row, int col, bool isEmpty = true)
    {
        gridPosition = new Vector2Int(col, row);
        IsEmpty = isEmpty;
    }

    public virtual void SetEmpty()
    {
        IsEmpty = true;
    }

    public virtual void SetOccupied()
    {
        IsEmpty = false;
    }
}

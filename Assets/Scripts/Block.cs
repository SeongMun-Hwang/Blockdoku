using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] BlockArray blockArray;
    public int[,] shape;

    private void OnEnable()
    {
        LoadShape();
    }
    private void LoadShape()
    {
        int rows = blockArray.shapeRows.Count;
        int cols = blockArray.shapeRows[0].Length;
        shape = new int[rows, cols];

        for(int i = 0; i < rows; i++)
        {
            string row = blockArray.shapeRows[i];
            for(int j = 0; j < cols; j++)
            {
                shape[i, j] = (row[j] == '1') ? 1 : 0;
            }
        }
    }
    public void RotateShape(int n)
    {
        for (int r = 0; r < n; r++)
        {
            int rows = shape.GetLength(0);
            int cols = shape.GetLength(1);
            int[,] rotated = new int[cols, rows];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    rotated[j, rows - 1 - i] = shape[i, j];
                }
            }
            shape = rotated;
        }
    }
}

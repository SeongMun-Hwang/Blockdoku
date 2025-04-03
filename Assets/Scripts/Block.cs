using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] BlockArray blockArray;
    public int[,] shape;

    private void Start()
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
}

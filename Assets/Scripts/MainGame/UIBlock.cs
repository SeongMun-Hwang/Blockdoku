using System.Collections.Generic;
using UnityEngine;

public class UIBlock : MonoBehaviour
{
    // Define the shape in the inspector using a list of strings
    public List<string> shapeRows = new List<string>();
    
    // The processed shape
    public int[,] shape;

    void Awake()
    {
        LoadShape();
    }

    private void LoadShape()
    {
        if (shapeRows.Count == 0) return;

        int rows = shapeRows.Count;
        int cols = shapeRows[0].Length;
        shape = new int[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            string row = shapeRows[i];
            for (int j = 0; j < cols; j++)
            {
                shape[i, j] = (row[j] == '1') ? 1 : 0;
            }
        }
    }
}

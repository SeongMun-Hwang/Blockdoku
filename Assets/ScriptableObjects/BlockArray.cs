using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "BlockArray", menuName = "Scriptable Objects/BlockArray")]
public class BlockArray : ScriptableObject
{
    public List<string> shapeRows;

    public List<Vector2Int> GetShape(int rotationCount = 0)
    {
        List<Vector2Int> shape = new List<Vector2Int>();
        if (shapeRows == null) return shape;

        for (int r = 0; r < shapeRows.Count; r++)
        {
            string row = shapeRows[r];
            for (int c = 0; c < row.Length; c++)
            {
                if (row[c] == '1')
                {
                    // Invert 'r' to treat top-left as (0,0)
                    shape.Add(new Vector2Int(c, -r));
                }
            }
        }

        // Apply rotations
        for (int i = 0; i < rotationCount; i++)
        {
            // 90-degree clockwise rotation: (x, y) -> (y, -x)
            shape = shape.Select(p => new Vector2Int(p.y, -p.x)).ToList();
        }

        return shape;
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Block_2D : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public BlockArray blockData;
    private List<Vector2Int> shape;
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalPosition;
    private Transform originalParent;
    private Vector2Int lastGridPosition;

    private List<RectTransform> childCubes = new List<RectTransform>();

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Find the visual cubes that are children of this block
        foreach (Transform child in transform)
        {
            RectTransform childRect = child.GetComponent<RectTransform>();
            if (childRect != null)
            {
                childCubes.Add(childRect);
            }
        }

        LoadShape(); // Loads shape data
    }

    private void LoadShape()
    {
        shape = new List<Vector2Int>();
        if (blockData == null) return;

        // Find the top-leftmost '1' to use as the pivot/origin (0,0)
        Vector2Int offset = new Vector2Int(int.MaxValue, int.MinValue);
        for (int r = 0; r < blockData.shapeRows.Count; r++)
        {
            for (int c = 0; c < blockData.shapeRows[r].Length; c++)
            {
                if (blockData.shapeRows[r][c] == '1')
                {
                    if (c < offset.x) offset.x = c;
                    if (r > offset.y) offset.y = r; // Using regular row index here
                }
            }
        }
        if (offset.x == int.MaxValue) offset = Vector2Int.zero;


        for (int r = 0; r < blockData.shapeRows.Count; r++)
        {
            string row = blockData.shapeRows[r];
            for (int c = 0; c < row.Length; c++)
            {
                if (row[c] == '1')
                {
                    // Position relative to the top-left corner of the shape's bounding box
                    shape.Add(new Vector2Int(c - offset.x, -(r - offset.y)));
                }
            }
        }

        // Update visual child cubes to match the loaded shape
        Vector2 cellSize = Vector2.one * 50f; // Default value in case GridManager is not available
        if (GridManager_2D.Instance != null)
        {
            cellSize = GridManager_2D.Instance.GetCellSize();
        }

        // Deactivate all child cubes first
        foreach (RectTransform child in childCubes)
        {
            child.gameObject.SetActive(false);
        }

        if (childCubes.Count < shape.Count)
        {
            Debug.LogError($"Block_2D: Not enough child cubes ({childCubes.Count}) for shape ({shape.Count}) in Block '{gameObject.name}'. Please check the prefab.");
            // Optionally, handle this by destroying extra shape elements or just proceeding with what's available
            return; // Exit if prefab is malformed, to prevent further errors
        }

        // If there are more child cubes than shape elements, it means some cubes will remain inactive
        // This is fine, as long as there are *at least* enough child cubes.

        for (int i = 0; i < shape.Count; i++)
        {
            RectTransform childRect = childCubes[i];
            childRect.gameObject.SetActive(true);
            // Position relative to the parent Block_2D's RectTransform.
            // The 'shape' provides coordinates relative to the block's pivot (top-left '1').
            // We need to multiply by cellSize to get actual pixel offsets.
            childRect.anchoredPosition = new Vector2(shape[i].x * cellSize.x, shape[i].y * cellSize.y);
        }
    }

    public List<Vector2Int> GetShape()
    {
        return shape;
    }

    public void RotateShape(int n)
    {
        for (int r_rot = 0; r_rot < n; r_rot++) // n rotations
        {
            char[,] currentChars = ConvertShapeRowsToCharArray(blockData.shapeRows);
            int rows = currentChars.GetLength(0);
            int cols = currentChars.GetLength(1);

            char[,] rotatedChars = new char[cols, rows]; // new rows = old cols, new cols = old rows

            for (int i = 0; i < rows; i++) // old row
            {
                for (int j = 0; j < cols; j++) // old col
                {
                    rotatedChars[j, (rows - 1) - i] = currentChars[i, j];
                }
            }
            blockData.shapeRows = ConvertCharArrayToShapeRows(rotatedChars);
        }
        LoadShape(); // This will reload the shape data and update visuals
    }

    private char[,] ConvertShapeRowsToCharArray(List<string> shapeRows)
    {
        if (shapeRows == null || shapeRows.Count == 0) return new char[0, 0];
        int rows = shapeRows.Count;
        int cols = shapeRows[0].Length;
        char[,] charArray = new char[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                charArray[i, j] = shapeRows[i][j];
            }
        }
        return charArray;
    }

    private List<string> ConvertCharArrayToShapeRows(char[,] charArray)
    {
        List<string> shapeRows = new List<string>();
        int rows = charArray.GetLength(0);
        int cols = charArray.GetLength(1);
        for (int i = 0; i < rows; i++)
        {
            string row = "";
            for (int j = 0; j < cols; j++)
            {
                row += charArray[i, j];
            }
            shapeRows.Add(row);
        }
        return shapeRows;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        canvas = GetComponentInParent<Canvas>();
        
        if (GridManager_2D.Instance != null)
        {
            GridManager_2D.Instance.ClearPreview();
        }

        // Bring to front
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();

        // Optional: Scale up
        rectTransform.localScale = Vector3.one * 1.2f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        if (GridManager_2D.Instance != null)
        {
            Vector2Int gridPosition = GridManager_2D.Instance.GetGridPosition(rectTransform.position);
            if (gridPosition != lastGridPosition)
            {
                lastGridPosition = gridPosition;
                GridManager_2D.Instance.ShowPreview(gridPosition, shape);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Optional: Scale down
        rectTransform.localScale = Vector3.one;

        if (GridManager_2D.Instance != null)
        {
            GridManager_2D.Instance.ClearPreview();
            Vector2Int gridPosition = GridManager_2D.Instance.GetGridPosition(rectTransform.position);

            if (GridManager_2D.Instance.IsValidPlacement(gridPosition, shape))
            {
                GridManager_2D.Instance.PlaceBlock(gridPosition, shape);
                BlockSpawner_2D.Instance.BlockPlaced(gameObject);
                Destroy(gameObject);
            }
            else
            {
                // Return to original position
                transform.SetParent(originalParent);
                rectTransform.anchoredPosition = originalPosition;
            }
        }
        else
        {
             // Fallback if GridManager is not found
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = originalPosition;
        }
    }
}

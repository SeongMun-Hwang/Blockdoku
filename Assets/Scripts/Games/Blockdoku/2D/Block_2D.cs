using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Block_2D : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public BlockArray blockData;
    [SerializeField] private float dragMovementMultiplier = 1.5f; // Adjust this value to change movement sensitivity
    [SerializeField] private float yOffsetOnGrab = 30f; // The amount the block moves up when grabbed
    private Vector3 grabWorldSpaceOffset;
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

        // Update visual child cubes to match the loaded shape, accounting for grid spacing
        Vector2 cellPitch = Vector2.one * 50f; // Default value in case GridManager is not available
        if (GridManager_2D.Instance != null)
        {
            cellPitch = GridManager_2D.Instance.GetCellPitch(); // Use GetCellPitch to include spacing
        }

        // Deactivate all child cubes first
        foreach (RectTransform child in childCubes)
        {
            child.gameObject.SetActive(false);
        }

        if (childCubes.Count < shape.Count)
        {
            Debug.LogError($"Block_2D: Not enough child cubes ({childCubes.Count}) for shape ({shape.Count}) in Block '{gameObject.name}'. Please check the prefab.");
            return; 
        }

        // Reposition child cubes to reflect the current shape (including rotation)
        for (int i = 0; i < shape.Count; i++)
        {
            RectTransform childRect = childCubes[i];
            childRect.gameObject.SetActive(true);
            childRect.anchoredPosition = new Vector2(shape[i].x * cellPitch.x, shape[i].y * cellPitch.y);
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

        // Calculate the world space offset for the visual lift
        Vector3 originalWorldPosition = rectTransform.position;
        rectTransform.anchoredPosition += new Vector2(0, yOffsetOnGrab);
        grabWorldSpaceOffset = rectTransform.position - originalWorldPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Apply movement with multiplier
        rectTransform.anchoredPosition += (eventData.delta / canvas.scaleFactor) * dragMovementMultiplier;

        if (GridManager_2D.Instance != null)
        {
            // Use the logical position (without the visual offset) for grid calculations
            Vector3 checkPosition = rectTransform.position - grabWorldSpaceOffset;
            Vector2Int gridPosition = GridManager_2D.Instance.GetGridPosition(checkPosition);
            if (gridPosition != lastGridPosition)
            {
                lastGridPosition = gridPosition;
                GridManager_2D.Instance.ShowPreview(gridPosition, shape);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (GridManager_2D.Instance != null)
        {
            GridManager_2D.Instance.ClearPreview();
            // Use the logical position (without the visual offset) for final placement
            Vector3 checkPosition = rectTransform.position - grabWorldSpaceOffset;
            Vector2Int gridPosition = GridManager_2D.Instance.GetGridPosition(checkPosition);

            if (GridManager_2D.Instance.IsValidPlacement(gridPosition, shape))
            {
                GridManager_2D.Instance.PlaceBlock(gridPosition, shape);
                BlockSpawner_2D.Instance.BlockPlaced(gameObject);
                Destroy(gameObject);
            }
            else
            {
                // Return to original position if placement is invalid
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

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public class Block_2D : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Data")]
    public BlockArray blockData;
    [SerializeField] private GameObject cellPrefab; // The prefab for a single visual cell

    [Header("Interaction")]
    [SerializeField] private float dragMovementMultiplier = 1.5f; // Adjust this value to change movement sensitivity
    [SerializeField] private float yOffsetOnGrab = 30f; // The amount the block moves up when grabbed

    private Vector3 grabWorldSpaceOffset;
    private List<Vector2Int> shape;
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalPosition;
    private Transform originalParent;
    private Vector2Int lastGridPosition;

    private readonly List<Transform> childCubes = new List<Transform>();

    /// <summary>
    /// Initializes the block with data, loads its shape, and applies rotation.
    /// This should be called by the spawner right after instantiation.
    /// </summary>
    public void Initialize(BlockArray data, int rotationCount)
    {
        rectTransform = GetComponent<RectTransform>();
        blockData = data;
        LoadShapeFromData();
        RotateShape(rotationCount);
        UpdateVisuals();
    }

    /// <summary>
    /// Parses the BlockArray data into a local 'shape' list.
    /// </summary>
    private void LoadShapeFromData()
    {
        shape = new List<Vector2Int>();
        if (blockData == null) return;

        for (int r = 0; r < blockData.shapeRows.Count; r++)
        {
            string row = blockData.shapeRows[r];
            for (int c = 0; c < row.Length; c++)
            {
                if (row[c] == '1')
                {
                    // Invert 'r' to treat top-left as (0,0) in a standard Cartesian coordinate system for easier rotation
                    shape.Add(new Vector2Int(c, -r));
                }
            }
        }
    }

    /// <summary>
    /// Rotates the local shape data N times.
    /// This does NOT modify the source BlockArray asset.
    /// </summary>
    private void RotateShape(int n)
    {
        for (int i = 0; i < n; i++)
        {
            // 90-degree clockwise rotation matrix: (x, y) -> (y, -x)
            shape = shape.Select(p => new Vector2Int(p.y, -p.x)).ToList();
        }
    }

    /// <summary>
    /// Clears existing visuals and creates new ones based on the current 'shape' data.
    /// It automatically centers the final visual shape.
    /// </summary>
    private void UpdateVisuals()
    {
        // Clear old visuals
        foreach (Transform child in childCubes)
        {
            Destroy(child.gameObject);
        }
        childCubes.Clear();

        if (shape == null || shape.Count == 0) return;
        
        // --- Centering Logic ---
        // Find the bounds of the current shape
        float minX = shape.Min(p => p.x);
        float maxX = shape.Max(p => p.x);
        float minY = shape.Min(p => p.y);
        float maxY = shape.Max(p => p.y);

        // The center of the shape is the average of its min/max bounds
        Vector2 shapeCenter = new Vector2((minX + maxX) / 2.0f, (minY + maxY) / 2.0f);
        // --- End of Centering Logic ---

        Vector2 cellPitch = GridManager_2D.Instance != null ? GridManager_2D.Instance.GetCellPitch() : new Vector2(50, 50);

        // Create and position new visuals
        foreach (Vector2Int pos in shape)
        {
            GameObject newCell = Instantiate(cellPrefab, transform);
            RectTransform cellRect = newCell.GetComponent<RectTransform>();

            // Position relative to the calculated center
            Vector2 centeredPos = new Vector2(pos.x - shapeCenter.x, pos.y - shapeCenter.y);
            cellRect.anchoredPosition = centeredPos * cellPitch;
            
            childCubes.Add(newCell.transform);
        }
    }

    public List<Vector2Int> GetShape()
    {
        return shape;
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

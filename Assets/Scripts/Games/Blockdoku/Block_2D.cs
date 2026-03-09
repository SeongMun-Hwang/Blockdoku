using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI; // Added for Image component

public class Block_2D : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Data")]
    [SerializeField] private BlockArray blockData;
    public BlockArray BlockData { get { return blockData; } } // Public getter for blockData
    [SerializeField] private GameObject cellPrefab; // The prefab for a single visual cell
    public Color blockColor; // New: Stores the color of this block

    [Header("Interaction")]
    [SerializeField] private float dragMovementMultiplier = 1.2f; 
    [SerializeField] private float spawnScale = 0.5f;
    [SerializeField] private float dragScale = 1.0f;
    [SerializeField] private float dragYOffset = 150.0f; // Offset in pixels/units to move block above finger

    private Vector2 originalScale;
    private Vector3 grabWorldSpaceOffset;
    private List<Vector2Int> shape;
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalPosition;
    private Transform originalParent;
    private Vector2Int lastGridPosition;
    private int currentRotationStep; // New field to store rotation step
    public int CurrentRotationStep { get { return currentRotationStep; } } // Public getter

    private readonly List<Transform> childCubes = new List<Transform>();
    private Vector2 anchorOffsetPixels;

    /// <summary>
    /// Initializes the block with data, loads its shape, and applies rotation.
    /// This should be called by the spawner right after instantiation.
    /// </summary>
    public void Initialize(BlockArray data, int rotationCount, Color color)
    {
        rectTransform = GetComponent<RectTransform>();
        blockData = data;
        blockColor = color; // Assign the color
        
        // Apply initial spawn scale
        rectTransform.localScale = Vector3.one * spawnScale;
        originalScale = Vector3.one * spawnScale;

        LoadShapeFromData();
        RotateShape(rotationCount);
        UpdateVisuals();
        SetColor(blockColor); // Apply the color to the visuals
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
        currentRotationStep = n; // Store the rotation step
        for (int i = 0; i < n; i++)
        {
            // 90-degree clockwise rotation matrix: (x, y) -> (y, -x)
            shape = shape.Select(p => new Vector2Int(p.y, -p.x)).ToList();
        }
    }


    /// <summary>
    /// Clears existing visuals and creates new ones based on the current 'shape' data.
    /// It automatically centers the final visual shape and matches its cell size to the grid.
    /// </summary>
    private void UpdateVisuals()
    {
        // Clear old visuals
        foreach (Transform child in childCubes)
        {
            Destroy(child.gameObject);
        }
        childCubes.Clear();

        if (shape == null || shape.Count == 0 || GridManager_2D.Instance == null) return;
        
        // --- Centering Logic ---
        // Find the bounds of the current shape
        float minX = shape.Min(p => p.x);
        float maxX = shape.Max(p => p.x);
        float minY = shape.Min(p => p.y);
        float maxY = shape.Max(p => p.y);

        // The center of the shape is the average of its min/max bounds
        Vector2 shapeCenter = new Vector2((minX + maxX) / 2.0f, (minY + maxY) / 2.0f);
        // --- End of Centering Logic ---

        Vector2 cellPitch = GridManager_2D.Instance.GetCellPitch();
        Vector2 cellSize = GridManager_2D.Instance.GetCellSize();

        // Create and position new visuals
        foreach (Vector2Int pos in shape)
        {
            GameObject newCell = Instantiate(cellPrefab, transform);
            RectTransform cellRect = newCell.GetComponent<RectTransform>();

            // Set the size of the cell to match the grid
            cellRect.sizeDelta = cellSize;

            // Position relative to the calculated center
            Vector2 centeredPos = new Vector2(pos.x - shapeCenter.x, pos.y - shapeCenter.y);
            cellRect.anchoredPosition = centeredPos * cellPitch;
            
            childCubes.Add(newCell.transform);
        }

        // --- Calculate anchorOffsetPixels ---
        if (shape.Count > 0)
        {
            float maxAnchorY = shape.Max(p => p.y); // For consistency, though now we're using Vector2Int.zero
            
            // Define the anchor cell as (0,0) in the block's local coordinate system (fixed origin)
            Vector2Int anchorCellLocal = Vector2Int.zero; 

            // Calculate the position of this anchor cell relative to the block's *current* pivot
            // (which is where the shapeCenter is adjusted to)
            // The anchorOffsetPixels represents the shift from the block's (container's) pivot
            // to the center of the designated anchorCellLocal.
            anchorOffsetPixels = new Vector2(anchorCellLocal.x - shapeCenter.x, anchorCellLocal.y - shapeCenter.y) * cellPitch;
        }
        else
        {
            anchorOffsetPixels = Vector2.zero;
        }
    }

    public List<Vector2Int> GetShape()
    {
        return shape;
    }

    /// <summary>
    /// Sets the color of all individual cells within this block.
    /// </summary>
    public void SetColor(Color color)
    {
        blockColor = color; // Also update the stored blockColor
        foreach (Transform child in childCubes)
        {
            Image cellImage = child.GetComponent<Image>();
            if (cellImage != null)
            {
                cellImage.color = color;
            }
        }
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

        // Scale up for dragging
        rectTransform.localScale = Vector3.one * dragScale;

        // Apply initial Y offset to lift it above finger
        rectTransform.anchoredPosition += new Vector2(0, dragYOffset);

        grabWorldSpaceOffset = Vector3.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Apply movement with multiplier
        rectTransform.anchoredPosition += (eventData.delta / canvas.scaleFactor) * dragMovementMultiplier;

        if (GridManager_2D.Instance != null)
        {
            // Note: anchorOffsetPixels is local to the block, it scales with the block.
            // When dragged, scale is 1.0, so it matches the grid.
            Vector3 checkPosition = rectTransform.position + (Vector3)(anchorOffsetPixels * rectTransform.localScale.x);
            Vector2Int gridPosition = GridManager_2D.Instance.GetNearestValidPosition(checkPosition, shape);
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
            Vector3 checkPosition = rectTransform.position + (Vector3)(anchorOffsetPixels * rectTransform.localScale.x);
            Vector2Int gridPosition = GridManager_2D.Instance.GetNearestValidPosition(checkPosition, shape);

            // If GetNearestValidPosition found a valid spot (it returns -1, -1 if not found)
            if (gridPosition.x != -1 && gridPosition.y != -1)
            {
                BlockSpawner_2D.Instance.BlockPlaced(gameObject, gridPosition, shape, blockColor);
                if (AudioManager_2D.Instance != null) AudioManager_2D.Instance.PlayBlockThudAudio();
                
                // Hide or disable the block instead of immediate Destroy to avoid issues while the routine starts
                gameObject.SetActive(false);
                Destroy(gameObject, 0.1f); 
            }
            else
            {
                // Return to original position and scale if placement is invalid
                transform.SetParent(originalParent);
                rectTransform.localScale = (Vector3)originalScale;
                rectTransform.anchoredPosition = originalPosition;
            }
        }
        else
        {
             // Fallback if GridManager is not found
            transform.SetParent(originalParent);
            rectTransform.localScale = (Vector3)originalScale;
            rectTransform.anchoredPosition = originalPosition;
        }
    }
}

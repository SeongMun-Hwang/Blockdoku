using UnityEngine;
using UnityEngine.UI;

public class Cell_2D : MonoBehaviour
{
    public Vector2Int gridPosition;
    public bool IsEmpty { get; private set; }
    public bool IsPreviewing { get; private set; }
    public Color BlockColor { get; private set; } // New: Stores the color of the block occupying this cell

    public Image cellImage;
    private Animator animator;

    void Awake()
    {
        if (cellImage == null)
        {
            cellImage = GetComponent<Image>();
        }
        animator = GetComponent<Animator>();
    }

    public void TriggerClearAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Clear");
        }
        
        // Logic state update
        IsEmpty = true;
        BlockColor = Color.clear;
    }

    public void Initialize(int row, int col, bool isEmpty)
    {
        gridPosition = new Vector2Int(col, row);
        IsEmpty = isEmpty;
        IsPreviewing = false;
        BlockColor = Color.clear; // Initialize with a transparent color

        if (IsEmpty) SetEmpty(); else SetOccupied(BlockColor); // Initialize visuals based on IsEmpty, pass stored color
    }

    public void SetOccupied(Color color)
    {
        IsEmpty = false;
        BlockColor = color; // Store the color
        if (cellImage != null && GridManager_2D.Instance != null)
        {
            cellImage.sprite = GridManager_2D.Instance.defaultOccupiedCellSprite;
            cellImage.color = BlockColor; // Apply the block's color
            transform.localScale = Vector3.one; // Ensure scale is 1 when occupied
        }
    }

    public void SetEmpty()
    {
        IsEmpty = true;
        BlockColor = Color.clear; // Reset color when empty
        if (cellImage != null && GridManager_2D.Instance != null)
        {
            cellImage.sprite = GridManager_2D.Instance.defaultEmptyCellSprite;
            cellImage.color = Color.white; // Default color for empty cells
            transform.localScale = Vector3.one; // Ensure scale is reset to 1
        }
    }

    public void SetPreview(Color previewColor)
    {
        if (IsEmpty)
        {
            IsPreviewing = true;
            cellImage.color = previewColor;
        }
    }

    public void ClearPreview()
    {
        if (IsPreviewing)
        {
            IsPreviewing = false;
            if (IsEmpty)
            {
                SetEmpty(); // Restore empty visuals
            }
            else
            {
                SetOccupied(BlockColor); // Restore occupied visuals with its stored color
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class Cell_2D : MonoBehaviour
{
    public Vector2Int gridPosition;
    public bool IsEmpty { get; private set; }
    public bool IsPreviewing { get; private set; }

    public Image cellImage;
    // Removed: public Sprite emptySprite;
    // Removed: public Sprite occupiedSprite;
    // Removed: private Color originalColor;

    void Awake()
    {
        // Ensure cellImage is assigned if not set in editor (e.g. if prefab is instantiated)
        if (cellImage == null)
        {
            cellImage = GetComponent<Image>();
        }
    }

    // Simplified Initialize: no more sprite parameters
    public void Initialize(int row, int col, bool isEmpty)
    {
        gridPosition = new Vector2Int(col, row);
        IsEmpty = isEmpty;
        IsPreviewing = false;
        // Call SetEmpty directly for initial state to apply correct sprite and color
        if (IsEmpty) SetEmpty(); else SetOccupied(); // Initialize visuals based on IsEmpty
    }

    public void SetOccupied()
    {
        IsEmpty = false;
        // Directly set sprite and color from GridManager_2D
        if (cellImage != null && GridManager_2D.Instance != null)
        {
            cellImage.sprite = GridManager_2D.Instance.defaultOccupiedCellSprite;
            cellImage.color = Color.gray; // Or whatever default color for occupied
        }
    }

    public void SetEmpty()
    {
        IsEmpty = true;
        // Directly set sprite and color from GridManager_2D
        if (cellImage != null && GridManager_2D.Instance != null)
        {
            cellImage.sprite = GridManager_2D.Instance.defaultEmptyCellSprite;
            cellImage.color = Color.white; // Or whatever default color for empty
        }
    }

    public void SetPreview(Color previewColor)
    {
        // Only set preview if the cell is currently empty
        // If it's not empty, it shouldn't be previewed, but for defensive purposes
        if (IsEmpty)
        {
            IsPreviewing = true;
            cellImage.color = previewColor;
            // Note: Sprite is not changed for preview, only color
        }
    }

    public void ClearPreview()
    {
        if (IsPreviewing)
        {
            IsPreviewing = false;
            // Revert to correct state based on IsEmpty by calling SetEmpty/SetOccupied
            if (IsEmpty)
            {
                SetEmpty(); // Restore empty visuals (white color, empty sprite)
            }
            else
            {
                SetOccupied(); // Restore occupied visuals (gray color, occupied sprite)
            }
        }
    }
}

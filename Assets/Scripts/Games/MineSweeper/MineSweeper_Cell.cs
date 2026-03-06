using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MineSweeper_Cell : BaseCell, IPointerClickHandler
{
    // public Vector2Int gridPosition; // Removed since it's in BaseCell
    public bool IsMine { get; private set; }
    public bool IsRevealed { get; private set; }
    public bool IsFlagged { get; private set; }
    public int AdjacentMines { get; private set; }

    [SerializeField] private Image cellImage;
    [SerializeField] private TextMeshProUGUI mineText;
    [SerializeField] private Sprite hiddenSprite;
    [SerializeField] private Sprite revealedSprite;
    [SerializeField] private Sprite mineSprite;
    [SerializeField] private Sprite flagSprite;

    private MineSweeper_GridManager gridManager;

    public void Initialize(int row, int col, MineSweeper_GridManager manager, Sprite hidden, Sprite revealed, Sprite mine, Sprite flag)
    {
        base.Initialize(row, col);
        gridManager = manager;
        hiddenSprite = hidden;
        revealedSprite = revealed;
        mineSprite = mine;
        flagSprite = flag;

        IsMine = false;
        IsRevealed = false;
        IsFlagged = false;
        AdjacentMines = 0;

        cellImage.sprite = hiddenSprite;
        mineText.text = "";
    }

    public void SetMine(bool isMine)
    {
        IsMine = isMine;
    }

    public void SetAdjacentMines(int count)
    {
        AdjacentMines = count;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (MineSweeper_GameManager.Instance.IsGameOver) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (!IsFlagged && !IsRevealed)
            {
                gridManager.OnCellClicked(gridPosition.y, gridPosition.x);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (!IsRevealed)
            {
                ToggleFlag();
            }
        }
    }

    public void Reveal()
    {
        if (IsRevealed) return;
        IsRevealed = true;
        IsFlagged = false;

        if (IsMine)
        {
            cellImage.sprite = mineSprite;
            mineText.text = "";
        }
        else
        {
            cellImage.sprite = revealedSprite;
            if (AdjacentMines > 0)
            {
                mineText.text = AdjacentMines.ToString();
                mineText.color = GetMineCountColor(AdjacentMines);
            }
            else
            {
                mineText.text = "";
            }
        }
    }

    private void ToggleFlag()
    {
        IsFlagged = !IsFlagged;
        cellImage.sprite = IsFlagged ? flagSprite : hiddenSprite;
        gridManager.OnFlagToggled(IsFlagged);
    }

    private Color GetMineCountColor(int count)
    {
        switch (count)
        {
            case 1: return Color.blue;
            case 2: return new Color(0, 0.5f, 0); // Dark green
            case 3: return Color.red;
            case 4: return new Color(0, 0, 0.5f); // Dark blue
            case 5: return new Color(0.5f, 0, 0); // Dark red
            case 6: return Color.cyan;
            case 7: return Color.black;
            case 8: return Color.gray;
            default: return Color.white;
        }
    }
}

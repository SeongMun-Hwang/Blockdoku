using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragDrop : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform gameBoard; // Assign in inspector
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private UIBlock uiBlock;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        uiBlock = GetComponent<UIBlock>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        originalPosition = rectTransform.anchoredPosition;

        // Automatically find the GameBoard if it's not assigned in the inspector
        if (gameBoard == null)
        {
            gameBoard = GameObject.Find("GameBoard").GetComponent<RectTransform>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Get the world position of the block's four corners
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        // Index 1 corresponds to the top-left corner of the RectTransform
        Vector3 topLeftCornerWorldPos = corners[1];

        // Convert the world position of the corner to a local point within the GameBoard
        // For a Screen Space - Overlay canvas, the camera parameter is null and the world position is used directly.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(gameBoard, topLeftCornerWorldPos, null, out Vector2 localPoint);

        // Adjust the local point to be relative to the GameBoard's bottom-left corner
        localPoint += new Vector2(gameBoard.rect.width / 2f, gameBoard.rect.height / 2f);

        // Calculate the grid cell size
        float cellWidth = gameBoard.rect.width / 9f;
        float cellHeight = gameBoard.rect.height / 9f;

        // Determine the starting grid cell based on the block's top-left corner
        int startCol = Mathf.FloorToInt(localPoint.x / cellWidth);
        int startRow = 8 - Mathf.FloorToInt(localPoint.y / cellHeight);

        // Try to place the block at the calculated starting position
        if (UIGameManager.Instance.TryPlaceBlock(uiBlock.shape, startRow, startCol))
        {
            // Successfully placed, check for completed lines/squares
            UIScoreManager.Instance.CheckBoardAfterPlacement();
            
            // Notify the spawner and destroy the used block
            UIBlockSpawner.Instance.RemoveBlock(gameObject);
            Destroy(gameObject);
        }
        else
        {
            // Invalid placement, return to original position
            rectTransform.anchoredPosition = originalPosition;
        }
    }
}

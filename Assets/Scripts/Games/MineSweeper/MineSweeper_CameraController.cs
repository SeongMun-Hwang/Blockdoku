using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MineSweeper_CameraController : MonoBehaviour, IDragHandler, IScrollHandler
{
    public RectTransform targetTransform; // gridParent
    public float minScale = 1f;
    public float maxScale = 5f;
    public float zoomStep = 0.2f;
    public float zoomContinuousSpeed = 2f; // Speed for button-hold zoom
    public float moveSpeed = 500f;

    private Vector3 initialScale;
    private Vector2 initialPosition;

    private float currentScale = 1f;
    private Vector2 panOffset;

    // For Touch Pinch
    private float initialTouchDist;
    private float initialTouchScale;

    void Awake()
    {
        if (targetTransform == null) targetTransform = GetComponent<RectTransform>();
        initialScale = targetTransform.localScale;
        initialPosition = targetTransform.anchoredPosition;
    }

    public void ResetCamera()
    {
        currentScale = 1f;
        panOffset = Vector2.zero;
        UpdateTransform();
    }

    // --- Button Actions ---
    public void ZoomIn()
    {
        currentScale = Mathf.Clamp(currentScale + zoomStep, minScale, maxScale);
        UpdateTransform();
    }

    public void ZoomOut()
    {
        currentScale = Mathf.Clamp(currentScale - zoomStep, minScale, maxScale);
        UpdateTransform();
    }

    public void ContinuousZoom(float direction)
    {
        currentScale = Mathf.Clamp(currentScale + direction * zoomContinuousSpeed * Time.deltaTime, minScale, maxScale);
        UpdateTransform();
    }

    public void Move(Vector2 direction)
    {
        panOffset += direction * moveSpeed * Time.deltaTime;
        ClampPosition();
        UpdateTransform();
    }

    // --- Touch & Mouse Logic ---
    public void OnScroll(PointerEventData eventData)
    {
        float delta = eventData.scrollDelta.y;
        if (delta > 0) ZoomIn();
        else if (delta < 0) ZoomOut();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Only allow pan if zoomed in or if specifically allowed
        if (Input.touchCount > 1) return; // Ignore single drag if pinching

        panOffset += eventData.delta / currentScale;
        ClampPosition();
        UpdateTransform();
    }

    void Update()
    {
        HandlePinchZoom();
    }

    private void HandlePinchZoom()
    {
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            if (touch1.phase == TouchPhase.Began)
            {
                initialTouchDist = Vector2.Distance(touch0.position, touch1.position);
                initialTouchScale = currentScale;
            }
            else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                float currentDist = Vector2.Distance(touch0.position, touch1.position);
                if (Mathf.Approximately(initialTouchDist, 0)) return;

                float factor = currentDist / initialTouchDist;
                currentScale = Mathf.Clamp(initialTouchScale * factor, minScale, maxScale);
                UpdateTransform();
            }
        }
    }

    private void UpdateTransform()
    {
        targetTransform.localScale = initialScale * currentScale;
        targetTransform.anchoredPosition = initialPosition + panOffset * currentScale;
    }

    private void ClampPosition()
    {
        // Add logic to keep the board within view if needed
        // For now, allow free panning
    }
}

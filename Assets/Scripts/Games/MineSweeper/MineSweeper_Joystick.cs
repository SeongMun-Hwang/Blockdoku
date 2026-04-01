using UnityEngine;
using UnityEngine.EventSystems;

public class MineSweeper_Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public RectTransform handle;
    public RectTransform background;
    public MineSweeper_UIManager uiManager;

    private float radius;
    private Vector2 inputVector;

    void Start()
    {
        radius = background.rect.width / 2;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            inputVector = localPoint / radius;
            if (inputVector.magnitude > 1.0f)
            {
                inputVector = inputVector.normalized;
            }

            handle.anchoredPosition = inputVector * radius;
            uiManager.joystickInput = inputVector;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
        uiManager.joystickInput = Vector2.zero;
    }
}

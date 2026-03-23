using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace Games._2048
{
    public enum MoveDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public class InputManager2048 : MonoBehaviour
    {
        public static event Action<MoveDirection> OnMove;

        private Vector2 startTouchPosition;
        private Vector2 endTouchPosition;
        private float swipeThreshold = 50f;

        void Update()
        {
            HandleKeyboardInput();
            HandleTouchInput();
        }

        private void HandleKeyboardInput()
        {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
                OnMove?.Invoke(MoveDirection.Up);
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
                OnMove?.Invoke(MoveDirection.Down);
            else if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
                OnMove?.Invoke(MoveDirection.Left);
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
                OnMove?.Invoke(MoveDirection.Right);
        }

        private void HandleTouchInput()
        {
            if (Touchscreen.current == null) return;

            var touch = Touchscreen.current.primaryTouch;
            if (touch.press.wasPressedThisFrame)
            {
                startTouchPosition = touch.position.ReadValue();
            }

            if (touch.press.wasReleasedThisFrame)
            {
                endTouchPosition = touch.position.ReadValue();
                DetectSwipe();
            }
        }

        private void DetectSwipe()
        {
            Vector2 swipeVector = endTouchPosition - startTouchPosition;

            if (swipeVector.magnitude > swipeThreshold)
            {
                if (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y))
                {
                    // Horizontal swipe
                    if (swipeVector.x > 0) OnMove?.Invoke(MoveDirection.Right);
                    else OnMove?.Invoke(MoveDirection.Left);
                }
                else
                {
                    // Vertical swipe
                    if (swipeVector.y > 0) OnMove?.Invoke(MoveDirection.Up);
                    else OnMove?.Invoke(MoveDirection.Down);
                }
            }
        }
    }
}

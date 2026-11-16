using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Features.Clicker.UI.Panels
{
    [DisallowMultipleComponent]
    public class ClickerTarget : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public event Action Pressed;
        public event Action Released;

        private bool _isPressed;
        private int _pointerId;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || _isPressed == true)
            {
                return;
            }

            _isPressed = true;
            _pointerId = eventData.pointerId;
            Pressed?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != _pointerId)
            {
                return;
            }

            Release();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventData.pointerId != _pointerId)
            {
                return;
            }

            Release();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus == false)
            {
                Release();
            }
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused == true)
            {
                Release();
            }
        }

        private void OnDisable()
        {
            Release();
        }

        private void Release()
        {
            if (_isPressed == false)
            {
                return;
            }

            _isPressed = false;
            Released?.Invoke();
        }
    }
}

using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Shared.UI.Effects
{
    [DisallowMultipleComponent]
    public class ButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Selectable _selectable;
        [SerializeField] private Vector3 _restScale = Vector3.one;
        [SerializeField] private float _hoverScale = 1.03f;
        [SerializeField] private float _pressScale = 0.96f;
        [SerializeField] private float _duration = 0.12f;
        [SerializeField] private Ease _ease = Ease.OutQuad;

        [Header("Color")]
        [SerializeField] private bool _useColorLerp;
        [SerializeField] private Graphic _colorTarget;
        [SerializeField] private Color _normalColor = new Color32(0x15, 0x18, 0x21, 0xFF);
        [SerializeField] private Color _hoverColor = new Color32(0x1D, 0x21, 0x2C, 0xFF);

        private bool _isHovered;
        private bool _isPressed;
        private Tween _scaleTween;
        private Tween _colorTween;

        private void OnDisable()
        {
            _scaleTween?.Kill();
            _colorTween?.Kill();
            _scaleTween = null;
            _colorTween = null;
            _isHovered = false;
            _isPressed = false;
            _target.localScale = _restScale;

            if (_useColorLerp == true)
            {
                _colorTarget.color = _normalColor;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            Refresh();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            _isPressed = false;
            Refresh();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            Refresh();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            Refresh();
        }

        private void Refresh()
        {
            var isInteractable = _selectable == null || _selectable.IsInteractable() == true;
            var scale = 1f;

            if (isInteractable == true && _isPressed == true)
            {
                scale = _pressScale;
            }
            else if (isInteractable == true && _isHovered == true)
            {
                scale = _hoverScale;
            }

            _scaleTween?.Kill();
            _scaleTween = _target
                .DOScale(_restScale * scale, _duration)
                .SetEase(_ease)
                .SetUpdate(true)
                .SetLink(gameObject);

            if (_useColorLerp == true)
            {
                var color = isInteractable == true && _isHovered == true ? _hoverColor : _normalColor;

                _colorTween?.Kill();
                _colorTween = DOTween
                    .To(() => _colorTarget.color, value => _colorTarget.color = value, color, _duration)
                    .SetEase(_ease)
                    .SetUpdate(true)
                    .SetLink(gameObject);
            }
        }

#if UNITY_EDITOR

        private void Reset()
        {
            _target = transform;
            _selectable = GetComponent<Selectable>();
            _colorTarget = GetComponent<Graphic>();
        }

#endif
    }
}

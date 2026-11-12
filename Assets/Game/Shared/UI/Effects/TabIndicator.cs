using System;
using DG.Tweening;
using UnityEngine;

namespace Shared.UI.Effects
{
    [DisallowMultipleComponent]
    public class TabIndicator : MonoBehaviour
    {
        public int SelectedIndex => _selectedIndex;

        [SerializeField] private RectTransform _indicator;
        [SerializeField] private TabIndicatorItem[] _tabs = Array.Empty<TabIndicatorItem>();
        [SerializeField] private int _initialIndex;
        [SerializeField] private bool _matchWidth = true;
        [SerializeField] private float _widthPadding;
        [SerializeField] private float _duration = 0.2f;
        [SerializeField] private Ease _ease = Ease.OutCubic;
        [SerializeField] private Color _activeColor = new Color32(0xF2, 0xF4, 0xF8, 0xFF);
        [SerializeField] private Color _inactiveColor = new Color32(0x8A, 0x93, 0xA6, 0xFF);

        private int _selectedIndex = -1;
        private Tween _tween;

        private void Start()
        {
            if (_selectedIndex < 0)
            {
                SelectImmediately(_initialIndex);
            }
        }

        private void OnEnable()
        {
            if (_selectedIndex >= 0)
            {
                SelectImmediately(_selectedIndex);
            }
        }

        private void OnDisable()
        {
            _tween?.Kill();
            _tween = null;
        }

        public void Select(int index)
        {
            _selectedIndex = index;
            ApplyColors();

            var target = _tabs[index].Rect;
            var startPosition = _indicator.localPosition.x;
            var startWidth = _indicator.rect.width;

            _tween?.Kill();
            _tween = DOVirtual
                .Float(0f, 1f, _duration, progress => Move(target, startPosition, startWidth, progress))
                .SetEase(_ease)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        public void Select(RectTransform tab)
        {
            Select(IndexOf(tab));
        }

        public void SelectImmediately(int index)
        {
            _tween?.Kill();
            _tween = null;
            _selectedIndex = index;
            ApplyColors();
            Canvas.ForceUpdateCanvases();
            Move(_tabs[index].Rect, _indicator.localPosition.x, _indicator.rect.width, 1f);
        }

        private int IndexOf(RectTransform tab)
        {
            for (var index = 0; index < _tabs.Length; index++)
            {
                if (_tabs[index].Rect == tab)
                {
                    return index;
                }
            }

            throw new ArgumentException($"Tab {tab.name} is not registered in {name}", nameof(tab));
        }

        private void Move(RectTransform target, float startPosition, float startWidth, float progress)
        {
            var parent = (RectTransform)_indicator.parent;
            var targetCenter = parent.InverseTransformPoint(target.TransformPoint(target.rect.center));
            var position = _indicator.localPosition;
            position.x = Mathf.LerpUnclamped(startPosition, targetCenter.x, progress);
            _indicator.localPosition = position;

            if (_matchWidth == true)
            {
                var targetWidth = target.rect.width + _widthPadding;
                _indicator.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.LerpUnclamped(startWidth, targetWidth, progress));
            }
        }

        private void ApplyColors()
        {
            for (var index = 0; index < _tabs.Length; index++)
            {
                var color = index == _selectedIndex ? _activeColor : _inactiveColor;

                foreach (var graphic in _tabs[index].Graphics)
                {
                    graphic.color = color;
                }
            }
        }
    }
}

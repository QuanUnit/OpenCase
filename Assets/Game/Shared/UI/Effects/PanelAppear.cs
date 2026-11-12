using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Shared.UI.Effects
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public class PanelAppear : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _content;
        [SerializeField] private float _offsetY = 24f;
        [SerializeField] private float _duration = 0.25f;
        [SerializeField] private Ease _ease = Ease.OutCubic;
        [SerializeField] private bool _playOnEnable = true;

        private Vector2 _basePosition;
        private bool _isAnimating;
        private Tween _tween;

        private void OnEnable()
        {
            if (_playOnEnable == true)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            Complete();
        }

        public void Play()
        {
            if (_isAnimating == true)
            {
                _tween?.Kill();
            }
            else
            {
                _basePosition = _content.anchoredPosition;
            }

            _isAnimating = true;
            Apply(0f);

            _tween = DOVirtual
                .Float(0f, 1f, _duration, Apply)
                .SetEase(_ease)
                .SetUpdate(true)
                .SetLink(gameObject)
                .OnComplete(() => _isAnimating = false);
        }

        public void Complete()
        {
            _tween?.Kill();
            _tween = null;

            if (_isAnimating == true)
            {
                Apply(1f);
            }

            _isAnimating = false;
        }

        private void Apply(float progress)
        {
            _canvasGroup.alpha = progress;
            _content.anchoredPosition = _basePosition - new Vector2(0f, _offsetY * (1f - progress));
        }

#if UNITY_EDITOR

        private void Reset()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _content = (RectTransform)transform;
        }

        private void OnValidate()
        {
            if (_content == null || _content.parent == null)
            {
                return;
            }

            var layoutGroup = _content.parent.GetComponent<LayoutGroup>();

            if (layoutGroup == null)
            {
                return;
            }

            var layoutElement = _content.GetComponent<LayoutElement>();

            if (layoutElement == null || layoutElement.ignoreLayout == false)
            {
                Debug.LogWarning($"{nameof(PanelAppear)} on {name}: content {_content.name} is driven by {layoutGroup.GetType().Name}; use a content child that is not layout-driven or set LayoutElement.ignoreLayout", this);
            }
        }

#endif
    }
}

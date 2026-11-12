using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Shared.UI.Effects
{
    [DisallowMultipleComponent]
    public class PulseGlow : MonoBehaviour
    {
        public bool IsPlaying => _tween != null;

        [SerializeField] private Graphic _glow;
        [SerializeField, Range(0f, 1f)] private float _minAlpha = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _maxAlpha = 0.55f;
        [SerializeField] private float _halfPeriod = 1.2f;
        [SerializeField] private Ease _ease = Ease.InOutSine;
        [SerializeField] private bool _playOnEnable = true;

        private Tween _tween;
        private float _originalAlpha;

        private void OnEnable()
        {
            if (_playOnEnable == true)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            Stop();
        }

        public void Play()
        {
            if (_tween == null)
            {
                _originalAlpha = _glow.color.a;
            }

            _tween?.Kill();
            ApplyAlpha(_minAlpha);

            _tween = DOVirtual
                .Float(_minAlpha, _maxAlpha, _halfPeriod, ApplyAlpha)
                .SetEase(_ease)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        public void Stop()
        {
            if (_tween == null)
            {
                return;
            }

            _tween.Kill();
            _tween = null;
            ApplyAlpha(_originalAlpha);
        }

        public void SetColor(Color color)
        {
            var alpha = _glow.color.a;
            color.a = alpha;
            _glow.color = color;
        }

        private void ApplyAlpha(float alpha)
        {
            var color = _glow.color;
            color.a = alpha;
            _glow.color = color;
        }
    }
}

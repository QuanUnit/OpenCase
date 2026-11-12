using DG.Tweening;
using UnityEngine;

namespace Shared.UI.Effects
{
    [DisallowMultipleComponent]
    public class ShineSweep : MonoBehaviour
    {
        [SerializeField] private RectTransform _area;
        [SerializeField] private RectTransform _shine;
        [SerializeField] private float _sweepDuration = 0.6f;
        [SerializeField] private float _interval = 3f;
        [SerializeField] private float _startDelay = 0.5f;
        [SerializeField] private Ease _ease = Ease.InOutSine;
        [SerializeField] private bool _playOnEnable = true;

        private Sequence _sequence;

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
            _sequence?.Kill();
            Apply(0f);

            var pause = Mathf.Max(_interval - _sweepDuration, 0f);

            _sequence = DOTween.Sequence()
                .Append(DOVirtual.Float(0f, 1f, _sweepDuration, Apply).SetEase(_ease))
                .AppendInterval(pause)
                .SetLoops(-1, LoopType.Restart)
                .SetDelay(_startDelay)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        public void Stop()
        {
            _sequence?.Kill();
            _sequence = null;
            Apply(0f);
        }

        private void Apply(float progress)
        {
            var travel = (_area.rect.width + _shine.rect.width) * 0.5f + _shine.rect.width;
            var position = _shine.anchoredPosition;
            position.x = Mathf.LerpUnclamped(-travel, travel, progress);
            _shine.anchoredPosition = position;
        }
    }
}

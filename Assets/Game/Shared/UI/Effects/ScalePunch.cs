using DG.Tweening;
using UnityEngine;

namespace Shared.UI.Effects
{
    [DisallowMultipleComponent]
    public class ScalePunch : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _restScale = Vector3.one;
        [SerializeField] private float _punchScale = 1.12f;
        [SerializeField] private float _duration = 0.35f;
        [SerializeField, Range(0f, 1f)] private float _riseShare = 0.4f;
        [SerializeField] private Ease _riseEase = Ease.OutQuad;
        [SerializeField] private Ease _settleEase = Ease.OutBack;

        private Sequence _sequence;

        private void OnDisable()
        {
            Complete();
        }

        public void Play()
        {
            _sequence?.Kill();
            _target.localScale = _restScale;

            var riseDuration = _duration * _riseShare;
            var settleDuration = _duration - riseDuration;

            _sequence = DOTween.Sequence()
                .Append(_target.DOScale(_restScale * _punchScale, riseDuration).SetEase(_riseEase))
                .Append(_target.DOScale(_restScale, settleDuration).SetEase(_settleEase))
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        public void Complete()
        {
            _sequence?.Kill();
            _sequence = null;
            _target.localScale = _restScale;
        }

#if UNITY_EDITOR

        private void Reset()
        {
            _target = transform;
        }

#endif
    }
}

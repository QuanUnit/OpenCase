using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Shared.UI.Effects
{
    [DisallowMultipleComponent]
    public class ScreenFlash : MonoBehaviour
    {
        [SerializeField] private Graphic _graphic;
        [SerializeField, Range(0f, 1f)] private float _peakAlpha = 0.025f;
        [SerializeField] private float _duration = 0.4f;
        [SerializeField, Range(0f, 1f)] private float _riseShare = 0.25f;
        [SerializeField] private Ease _riseEase = Ease.OutQuad;
        [SerializeField] private Ease _fadeEase = Ease.InQuad;

        private Sequence _sequence;

        private void OnDisable()
        {
            Stop();
        }

        public void Play(Color color)
        {
            _sequence?.Kill();

            color.a = 0f;
            _graphic.color = color;
            _graphic.enabled = true;

            var riseDuration = _duration * _riseShare;
            var fadeDuration = _duration - riseDuration;

            _sequence = DOTween.Sequence()
                .Append(DOVirtual.Float(0f, _peakAlpha, riseDuration, ApplyAlpha).SetEase(_riseEase))
                .Append(DOVirtual.Float(_peakAlpha, 0f, fadeDuration, ApplyAlpha).SetEase(_fadeEase))
                .SetUpdate(true)
                .SetLink(gameObject)
                .OnComplete(Stop);
        }

        public void Stop()
        {
            _sequence?.Kill();
            _sequence = null;
            ApplyAlpha(0f);
            _graphic.enabled = false;
        }

        private void ApplyAlpha(float alpha)
        {
            var color = _graphic.color;
            color.a = alpha;
            _graphic.color = color;
        }
    }
}

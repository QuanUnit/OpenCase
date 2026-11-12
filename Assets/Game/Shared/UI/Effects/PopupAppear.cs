using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Shared.UI.Effects
{
    [DisallowMultipleComponent]
    public class PopupAppear : MonoBehaviour
    {
        [SerializeField] private Graphic _backdrop;
        [SerializeField] private Transform _card;
        [SerializeField, Range(0f, 1f)] private float _backdropAlpha = 0.85f;
        [SerializeField] private float _backdropDuration = 0.2f;
        [SerializeField] private float _cardStartScale = 0.9f;
        [SerializeField] private float _cardDuration = 0.3f;
        [SerializeField] private Ease _cardEase = Ease.OutBack;
        [SerializeField] private bool _playOnEnable = true;

        private Vector3 _cardBaseScale;
        private Tween _backdropTween;
        private Tween _cardTween;

        private void Awake()
        {
            _cardBaseScale = _card.localScale;
        }

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
            KillTweens();
            ApplyBackdrop(0f);
            _card.localScale = _cardBaseScale * _cardStartScale;

            _backdropTween = DOVirtual
                .Float(0f, _backdropAlpha, _backdropDuration, ApplyBackdrop)
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .SetLink(gameObject);

            _cardTween = _card
                .DOScale(_cardBaseScale, _cardDuration)
                .SetEase(_cardEase)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        public void Complete()
        {
            KillTweens();
            ApplyBackdrop(_backdropAlpha);
            _card.localScale = _cardBaseScale;
        }

        private void ApplyBackdrop(float alpha)
        {
            var color = _backdrop.color;
            color.a = alpha;
            _backdrop.color = color;
        }

        private void KillTweens()
        {
            _backdropTween?.Kill();
            _cardTween?.Kill();
            _backdropTween = null;
            _cardTween = null;
        }
    }
}

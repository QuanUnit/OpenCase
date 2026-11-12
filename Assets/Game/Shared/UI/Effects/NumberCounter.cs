using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Shared.UI.Effects
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class NumberCounter : MonoBehaviour
    {
        public float Value => _targetValue;

        [SerializeField] private TMP_Text _text;
        [SerializeField] private int _decimals = 2;
        [SerializeField] private string _prefix = "";
        [SerializeField] private string _postfix = " $";
        [SerializeField] private float _duration = 0.4f;
        [SerializeField] private Ease _ease = Ease.OutCubic;
        [SerializeField] private bool _flash = true;
        [SerializeField] private Color _increaseColor = new Color32(0x3D, 0xDC, 0x84, 0xFF);
        [SerializeField] private Color _decreaseColor = new Color32(0xFF, 0x4D, 0x5E, 0xFF);
        [SerializeField] private float _flashInDuration = 0.08f;
        [SerializeField] private float _flashOutDuration = 0.4f;
        [SerializeField] private float _changeTolerance = 0.0001f;

        private float _shownValue;
        private float _targetValue;
        private bool _hasValue;
        private Color _baseColor;
        private Tween _valueTween;
        private Sequence _flashSequence;

        private void Awake()
        {
            _baseColor = _text.color;
        }

        private void OnDisable()
        {
            _valueTween?.Kill();
            _valueTween = null;

            if (_hasValue == true)
            {
                Show(_targetValue);
            }

            if (_flashSequence != null)
            {
                _flashSequence.Kill();
                _flashSequence = null;
                _text.color = _baseColor;
            }
        }

        public void SetValue(float value)
        {
            if (_hasValue == false || isActiveAndEnabled == false)
            {
                SetValueImmediately(value);
                return;
            }

            var direction = NumberCounterFormatter.GetChangeDirection(_targetValue, value, _changeTolerance);
            _targetValue = value;

            if (direction == 0)
            {
                return;
            }

            _valueTween?.Kill();
            _valueTween = DOVirtual
                .Float(_shownValue, value, _duration, Show)
                .SetEase(_ease)
                .SetUpdate(true)
                .SetLink(gameObject);

            if (_flash == true)
            {
                Flash(direction > 0 ? _increaseColor : _decreaseColor);
            }
        }

        public void SetValueImmediately(float value)
        {
            _valueTween?.Kill();
            _valueTween = null;
            _hasValue = true;
            _targetValue = value;
            Show(value);
        }

        public void SetBaseColor(Color color)
        {
            _baseColor = color;

            if (_flashSequence == null)
            {
                _text.color = color;
            }
        }

        private void Flash(Color color)
        {
            _flashSequence?.Kill();

            var startColor = _text.color;

            _flashSequence = DOTween.Sequence()
                .Append(DOVirtual.Float(0f, 1f, _flashInDuration, progress => _text.color = Color.LerpUnclamped(startColor, color, progress)))
                .Append(DOVirtual.Float(0f, 1f, _flashOutDuration, progress => _text.color = Color.LerpUnclamped(color, _baseColor, progress)))
                .SetUpdate(true)
                .SetLink(gameObject)
                .OnComplete(() => _flashSequence = null);
        }

        private void Show(float value)
        {
            _shownValue = value;
            _text.text = NumberCounterFormatter.Format(value, _decimals, _prefix, _postfix);
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            _text = GetComponent<TMP_Text>();
        }

#endif
    }
}

using System;
using System.Globalization;
using Shared.Tools.Mathematics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Upgrade.UI.Panels
{
    public class UpgradeExtraMoneyInput : MonoBehaviour
    {
        public event Action<float> ValueChanged;

        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _maxButton;
        [SerializeField] private CanvasGroup _controlsGroup;
        [SerializeField, Range(0f, 1f)] private float _disabledAlpha = 0.4f;
        [SerializeField] private string _valueFormat = "0.00";

        private float _max;
        private float _value;

        public float Value => _value;

        private void OnEnable()
        {
            _slider.onValueChanged.AddListener(SliderChangeHandle);
            _inputField.onEndEdit.AddListener(InputEndEditHandle);
            _maxButton.onClick.AddListener(MaxClickHandle);
        }

        public void SetRange(float max)
        {
            _max = Mathf.Max(0f, max);

            var isInteractable = _max > 0f;

            _value = Clamp(_value);
            _slider.SetValueWithoutNotify(_value);
            _slider.minValue = 0f;
            _slider.maxValue = _max;
            _controlsGroup.interactable = isInteractable;
            _controlsGroup.blocksRaycasts = isInteractable;
            _controlsGroup.alpha = isInteractable == true ? 1f : _disabledAlpha;

            Apply(_value);
        }

        public void SetValue(float value)
        {
            Apply(value);
        }

        private void Apply(float value)
        {
            _value = Clamp(value);
            _slider.SetValueWithoutNotify(_value);
            _inputField.SetTextWithoutNotify(Format(_value));
        }

        private void ApplyAndNotify(float value)
        {
            var previous = _value;

            Apply(value);

            if (_value == previous)
            {
                return;
            }

            ValueChanged?.Invoke(_value);
        }

        private float Clamp(float value)
        {
            return Mathf.Clamp(MoneyMath.RoundToMoney(value), 0f, _max);
        }

        private bool TryParse(string text, out float value)
        {
            var normalized = text.Replace(',', '.');

            var isParsed = float.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

            return isParsed == true && float.IsFinite(value) == true;
        }

        private string Format(float value)
        {
            return value.ToString(_valueFormat, CultureInfo.InvariantCulture);
        }

        private void ParseAndClamp(string text)
        {
            var isParsed = TryParse(text, out var parsed);

            if (isParsed == false)
            {
                _inputField.SetTextWithoutNotify(Format(_value));
                return;
            }

            ApplyAndNotify(parsed);
        }

        private void SliderChangeHandle(float value)
        {
            ApplyAndNotify(value);
        }

        private void InputEndEditHandle(string text)
        {
            ParseAndClamp(text);
        }

        private void MaxClickHandle()
        {
            ApplyAndNotify(_max);
        }

        private void OnDisable()
        {
            _slider.onValueChanged.RemoveListener(SliderChangeHandle);
            _inputField.onEndEdit.RemoveListener(InputEndEditHandle);
            _maxButton.onClick.RemoveListener(MaxClickHandle);
        }
    }
}

using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Upgrade.UI.Panels
{
    public class UpgradeMultiplierOption : MonoBehaviour
    {
        public event Action<UpgradeMultiplierOption> Clicked;

        [SerializeField] private Button _button;
        [SerializeField] private Image _visual;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private string _labelFormat = "x{0:0.##}";

        [Header("Colors")]
        [SerializeField] private Color _normalColor;
        [SerializeField] private Color _selectedColor;
        [SerializeField] private Color _disabledColor;
        [SerializeField] private Color _normalTextColor;
        [SerializeField] private Color _selectedTextColor;
        [SerializeField] private Color _disabledTextColor;

        private void OnEnable()
        {
            _button.onClick.AddListener(ClickHandle);
        }

        public void SetMultiplier(float multiplier)
        {
            _label.text = string.Format(CultureInfo.InvariantCulture, _labelFormat, multiplier);
        }

        public void SetState(bool isAvailable, bool isSelected)
        {
            _button.interactable = isAvailable;

            if (isAvailable == false)
            {
                _visual.color = _disabledColor;
                _label.color = _disabledTextColor;
                return;
            }

            _visual.color = isSelected == true ? _selectedColor : _normalColor;
            _label.color = isSelected == true ? _selectedTextColor : _normalTextColor;
        }

        private void ClickHandle()
        {
            Clicked?.Invoke(this);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(ClickHandle);
        }
    }
}

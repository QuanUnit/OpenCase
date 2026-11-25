using System;
using System.Globalization;
using Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Upgrade.UI.Panels
{
    public class UpgradeTargetOption : MonoBehaviour
    {
        public event Action<UpgradeTargetOption> Clicked;

        [SerializeField] private Button _button;
        [SerializeField] private Image _background;
        [SerializeField] private Image _outline;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TextedCurrencyView _price;
        [SerializeField] private TMP_Text _chanceText;
        [SerializeField] private string _chanceFormat = "{0:0.0}%";
        [SerializeField] private Image _rarityStrip;
        [SerializeField] private Image _rarityGlow;
        [SerializeField, Range(0f, 1f)] private float _rarityGlowAlpha = 0.3f;

        [Header("Colors")]
        [SerializeField] private Color _normalColor;
        [SerializeField] private Color _selectedColor;
        [SerializeField] private Color _normalOutlineColor;
        [SerializeField] private Color _selectedOutlineColor;

        private void OnEnable()
        {
            _button.onClick.AddListener(ClickHandle);
        }

        public void SetView(Data data)
        {
            _icon.sprite = data.Icon;
            _icon.enabled = data.Icon != null;
            _name.text = data.Name;
            _price.SetView(data.Price);
            _rarityStrip.color = data.RarityColor;
            _rarityGlow.color = new Color(data.RarityColor.r, data.RarityColor.g, data.RarityColor.b, _rarityGlowAlpha);

            SetChance(data.Chance);
        }

        public void SetChance(float chance)
        {
            _chanceText.text = string.Format(CultureInfo.InvariantCulture, _chanceFormat, chance * 100f);
        }

        public void SetSelected(bool isSelected)
        {
            _background.color = isSelected == true ? _selectedColor : _normalColor;
            _outline.color = isSelected == true ? _selectedOutlineColor : _normalOutlineColor;
        }

        private void ClickHandle()
        {
            Clicked?.Invoke(this);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(ClickHandle);
        }

        public struct Data
        {
            public Sprite Icon;
            public string Name;
            public float Price;
            public Color RarityColor;
            public float Chance;
        }
    }
}

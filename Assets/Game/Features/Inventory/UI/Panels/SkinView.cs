using System;
using Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Inventory.UI.Panels
{
    public class SkinView : MonoBehaviour
    {
        public event Action<SkinView> Clicked; 

        public float Price { get; private set; }
        
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TextedCurrencyView _price;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _rarity;
        [SerializeField] private Button _button;
        [SerializeField] private Image _rarityGlow;
        [SerializeField, Range(0f, 1f)] private float _rarityGlowAlpha = 0.3f;
        
        public void SetView(Data data)
        {
            _name.text = data.Name;
            _price.SetView(data.Price);
            _icon.sprite = data.Icon;
            _icon.enabled = data.Icon != null;
            _rarity.color = data.RarityColor;
            _rarityGlow.color = new Color(data.RarityColor.r, data.RarityColor.g, data.RarityColor.b, _rarityGlowAlpha);

            Price = data.Price;
        }

        private void OnEnable()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(ClickHandle);
            }
        }

        private void ClickHandle()
        {
            Clicked?.Invoke(this);
        }

        private void OnDisable()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(ClickHandle);
            }
        }

        public struct Data
        {
            public string Name;
            public Sprite Icon;
            public Color RarityColor;
            public float Price;
        }
    }
}
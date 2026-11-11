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
        
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TextedCurrencyView _price;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _rarity;
        [SerializeField] private Button _button;
        
        public void SetView(Data data)
        {
            _name.text = data.Name;
            _price.SetView(data.Price);
            _icon.sprite = data.Icon;
            _rarity.color = data.RarityColor;
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
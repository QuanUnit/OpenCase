using System;
using Common.UI;
using QCore.UISystem.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.Inventory.Panels
{
    public class SkinDetailsPopup : Panel
    {
        public event Action CloseClicked;
        public event Action SellClicked;
        
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _sellButton;
        [SerializeField] private Image _skinIcon;
        [SerializeField] private Image _rarityBar;
        [SerializeField] private TMP_Text _skinName;
        [SerializeField] private TMP_Text _floatValue;
        [SerializeField] private TextedCurrencyView _currencyView;

        public void SetView(Data data)
        {
            _skinIcon.sprite = data.SkinIcon;
            _rarityBar.color = data.RarityColor;
            _skinName.text = data.SkinName;
            _currencyView.SetView(data.Price);
            _floatValue.SetText($"{data.FloatValue:F7}");
        }

        private void OnEnable()
        {
            _closeButton.onClick.AddListener(CloseClickHandle);
            _sellButton.onClick.AddListener(SellClickHandle);
        }

        private void SellClickHandle()
        {
            SellClicked?.Invoke();
        }

        private void CloseClickHandle()
        {
            CloseClicked?.Invoke();
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveListener(CloseClickHandle);
            _sellButton.onClick.RemoveListener(SellClickHandle);
        }

        public struct Data
        {
            public Sprite SkinIcon;
            public Color RarityColor;
            public string SkinName;
            public float Price;
            public float FloatValue;
        }
    }
}
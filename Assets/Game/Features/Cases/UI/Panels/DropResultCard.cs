using System;
using Shared.UI;
using Shared.UI.Effects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Cases.UI.Panels
{
    public class DropResultCard : MonoBehaviour
    {
        public event Action<DropResultCard> SellClicked;

        public RectTransform SellButtonRect => (RectTransform)_sellButton.transform;

        [Header("Info")]
        [SerializeField] private CanvasGroup _infoCanvasGroup;
        [SerializeField] private Image _rarityStrip;
        [SerializeField] private TMP_Text _weaponText;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _wearText;
        [SerializeField] private GameObject _statTrakBadge;
        [SerializeField] private GameObject _souvenirBadge;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private TextedCurrencyView _price;

        [Header("Sell")]
        [SerializeField] private Button _sellButton;
        [SerializeField] private TMP_Text _sellLabel;
        [SerializeField] private string _sellLabelFormat = "SELL  {0}";
        [SerializeField] private GameObject _soldBadge;
        [SerializeField] private TMP_Text _soldLabel;
        [SerializeField] private string _soldLabelFormat = "SOLD +{0}";

        [Header("Colors")]
        [SerializeField] private Image _outline;
        [SerializeField] private Color _outlineColor = new Color32(0x2A, 0x2F, 0x3D, 0xFF);
        [SerializeField] private Color _nameColor = new Color32(0xF2, 0xF4, 0xF8, 0xFF);
        [SerializeField] private Color _priceColor = new Color32(0xFF, 0x8A, 0x1F, 0xFF);
        [SerializeField] private Color _soldTextColor = new Color32(0x8A, 0x93, 0xA6, 0xFF);
        [SerializeField, Range(0f, 1f)] private float _soldInfoAlpha = 0.6f;

        [Header("Effects")]
        [SerializeField] private ScalePunch _appearPunch;
        [SerializeField] private ScalePunch _soldPunch;

        private void OnEnable()
        {
            _sellButton.onClick.AddListener(SellClickHandle);
        }

        public void Show(Data data)
        {
            _rarityStrip.color = data.RarityColor;
            _weaponText.text = data.WeaponName;
            _nameText.text = data.SkinName;
            _wearText.text = data.WearName;
            _statTrakBadge.SetActive(data.IsStatTrak);
            _souvenirBadge.SetActive(data.IsSouvenir);
            _price.SetView(data.SellPrice);
            _sellLabel.text = string.Format(_sellLabelFormat, NumberCounterFormatter.FormatMoney(data.SellPrice));

            _infoCanvasGroup.alpha = 1f;
            _nameText.color = _nameColor;
            _priceText.color = _priceColor;
            _outline.color = _outlineColor;
            _sellButton.gameObject.SetActive(true);
            _soldBadge.SetActive(false);

            gameObject.SetActive(true);
            _appearPunch.Play();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void MarkBest(Color rarityColor)
        {
            _outline.color = rarityColor;
        }

        public void MarkSold(float amount)
        {
            ApplyInactiveState();

            _soldLabel.text = string.Format(_soldLabelFormat, NumberCounterFormatter.FormatMoney(amount));
            _soldBadge.SetActive(true);
            _soldPunch.Play();
        }

        public void MarkUnavailable()
        {
            ApplyInactiveState();
        }

        private void ApplyInactiveState()
        {
            _infoCanvasGroup.alpha = _soldInfoAlpha;
            _nameText.color = _soldTextColor;
            _priceText.color = _soldTextColor;
            _outline.color = _outlineColor;
            _sellButton.gameObject.SetActive(false);
        }

        private void SellClickHandle()
        {
            SellClicked?.Invoke(this);
        }

        private void OnDisable()
        {
            _sellButton.onClick.RemoveListener(SellClickHandle);
        }

        public struct Data
        {
            public string WeaponName;
            public string SkinName;
            public string WearName;
            public bool IsStatTrak;
            public bool IsSouvenir;
            public float SellPrice;
            public Color RarityColor;
        }
    }
}

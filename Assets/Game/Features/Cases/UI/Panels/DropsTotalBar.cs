using System;
using Shared.UI;
using Shared.UI.Effects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Cases.UI.Panels
{
    public class DropsTotalBar : MonoBehaviour
    {
        public event Action SellAllClicked;

        public RectTransform SellAllButtonRect => (RectTransform)_sellAllButton.transform;

        [Header("Unsold")]
        [SerializeField] private GameObject _unsoldSummary;
        [SerializeField] private TMP_Text _countText;
        [SerializeField] private string _countFormat = "{0} ITEMS";
        [SerializeField] private string _singleCountFormat = "{0} ITEM";
        [SerializeField] private TextedCurrencyView _total;

        [Header("Sell all")]
        [SerializeField] private Button _sellAllButton;
        [SerializeField] private TMP_Text _sellAllLabel;
        [SerializeField] private string _sellAllLabelFormat = "SELL ALL  {0}";
        [SerializeField] private GameObject _soldBadge;
        [SerializeField] private TMP_Text _soldLabel;
        [SerializeField] private string _allSoldLabelFormat = "ALL SOLD +{0}";
        [SerializeField] private string _allSoldLabel = "ALL SOLD";

        [Header("Effects")]
        [SerializeField] private ScalePunch _totalPunch;

        private void OnEnable()
        {
            _sellAllButton.onClick.AddListener(SellAllClickHandle);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetState(int unsoldCount, float unsoldTotal, float soldTotal)
        {
            var hasUnsold = unsoldCount > 0;

            _countText.text = string.Format(unsoldCount == 1 ? _singleCountFormat : _countFormat, unsoldCount);
            _total.SetView(unsoldTotal);
            _sellAllLabel.text = string.Format(_sellAllLabelFormat, NumberCounterFormatter.FormatMoney(unsoldTotal));
            _soldLabel.text = soldTotal > 0f ? string.Format(_allSoldLabelFormat, NumberCounterFormatter.FormatMoney(soldTotal)) : _allSoldLabel;

            _unsoldSummary.SetActive(hasUnsold);
            _sellAllButton.gameObject.SetActive(hasUnsold);
            _soldBadge.SetActive(hasUnsold == false);

            if (hasUnsold == true && gameObject.activeInHierarchy == true)
            {
                _totalPunch.Play();
            }
        }

        private void SellAllClickHandle()
        {
            SellAllClicked?.Invoke();
        }

        private void OnDisable()
        {
            _sellAllButton.onClick.RemoveListener(SellAllClickHandle);
        }
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Contracts.UI.Panels
{
    public class ContractSlotView : MonoBehaviour
    {
        public event Action<ContractSlotView> Clicked;

        [SerializeField] private Button _button;
        [SerializeField] private GameObject _filled;
        [SerializeField] private GameObject _placeholder;
        [SerializeField] private TMP_Text _number;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _rarityStrip;

        private void OnEnable()
        {
            _button.onClick.AddListener(ClickHandle);
        }

        public void SetNumber(int number)
        {
            _number.SetText("{0}", number);
        }

        public void SetView(Sprite icon, Color rarityColor)
        {
            _icon.sprite = icon;
            _icon.enabled = icon != null;
            _rarityStrip.color = rarityColor;

            _filled.SetActive(true);
            _placeholder.SetActive(false);
            _button.interactable = true;
        }

        public void SetEmpty()
        {
            _filled.SetActive(false);
            _placeholder.SetActive(true);
            _button.interactable = false;
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

using System;
using Shared.UI.Effects;
using Features.Cases.UI.Panels;
using Features.Clicker.UI.Panels;
using Features.Inventory.UI.Panels;
using Features.OthersDrop.UI.Panels;
using QCore.UISystem.Panels;
using UnityEngine;
using UnityEngine.UI;

namespace Features.GameHub.UI.Panels
{
    public class GameHubPanel : Panel
    {
        public event Action CasesClickHandle;
        public event Action InventoryClickHandle;
        public event Action EarnClickHandle;
        
        public CasesSubPanel CasesSubPanel => _casesSubPanel;
        public InventorySubPanel InventorySubPanel => _inventorySubPanel;
        public ClickerSubPanel ClickerSubPanel => _clickerSubPanel;
        public OthersDropBar OthersDropBar => _othersDropBar;
        
        [Header("Buttons")]
        [SerializeField] private Button _casesButton;
        [SerializeField] private Button _inventoryButonl;
        [SerializeField] private Button _earnButton;
        [SerializeField] private TabIndicator _tabIndicator;
        
        [Header("Sub Panels")]
        [SerializeField] private CasesSubPanel _casesSubPanel;
        [SerializeField] private InventorySubPanel _inventorySubPanel;
        [SerializeField] private ClickerSubPanel _clickerSubPanel;

        [Header("Hub elements")] 
        [SerializeField] private OthersDropBar _othersDropBar;
        [SerializeField] private NumberCounter _softCurrencyCounter;

        
        private void OnEnable()
        {
            _casesButton.onClick.AddListener(CasesButtonClickHandle);
            _inventoryButonl.onClick.AddListener(InventoryButtonClickHandle);
            _earnButton.onClick.AddListener(EarnButtonClickHandle);
        }

        public void SetSoftCurrency(float value)
        {
            _softCurrencyCounter.SetValue(value);
        }

        public void SetActiveTab(GameHubTab tab)
        {
            var index = (int)tab;

            if (_tabIndicator.gameObject.activeSelf == false)
            {
                _tabIndicator.gameObject.SetActive(true);
                _tabIndicator.SelectImmediately(index);
                return;
            }

            _tabIndicator.Select(index);
        }

        private void InventoryButtonClickHandle()
        {
            InventoryClickHandle?.Invoke();
        }

        private void CasesButtonClickHandle()
        {
            CasesClickHandle?.Invoke();
        }

        private void EarnButtonClickHandle()
        {
            EarnClickHandle?.Invoke();
        }

        private void OnDisable()
        {
            _casesButton.onClick.RemoveListener(CasesButtonClickHandle);
            _inventoryButonl.onClick.RemoveListener(InventoryButtonClickHandle);
            _earnButton.onClick.RemoveListener(EarnButtonClickHandle);
        }
    }
}

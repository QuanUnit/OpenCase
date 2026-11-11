using System;
using Shared.UI;
using Features.Cases.UI.Panels;
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
        
        public CasesSubPanel CasesSubPanel => _casesSubPanel;
        public InventorySubPanel InventorySubPanel => _inventorySubPanel;
        public OthersDropBar OthersDropBar => _othersDropBar;
        
        [Header("Buttons")]
        [SerializeField] private Button _casesButton;
        [SerializeField] private Button _inventoryButonl;
        
        [Header("Sub Panels")]
        [SerializeField] private CasesSubPanel _casesSubPanel;
        [SerializeField] private InventorySubPanel _inventorySubPanel;

        [Header("Hub elements")] 
        [SerializeField] private OthersDropBar _othersDropBar;
        [SerializeField] private TextedCurrencyView _softCurencyView;

        
        private void OnEnable()
        {
            _casesButton.onClick.AddListener(CasesButtonClickHandle);
            _inventoryButonl.onClick.AddListener(InventoryButtonClickHandle);
        }

        public void SetSoftCurrency(float value)
        {
            _softCurencyView.SetView(value);
        }

        private void InventoryButtonClickHandle()
        {
            InventoryClickHandle?.Invoke();
        }

        private void CasesButtonClickHandle()
        {
            CasesClickHandle?.Invoke();
        }

        private void OnDisable()
        {
            _casesButton.onClick.RemoveListener(CasesButtonClickHandle);
            _inventoryButonl.onClick.RemoveListener(InventoryButtonClickHandle);
        }
    }
}
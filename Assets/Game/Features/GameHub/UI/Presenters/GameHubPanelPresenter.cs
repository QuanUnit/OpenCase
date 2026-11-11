using Features.Cases.UI.Panels;
using Features.Inventory.UI.Panels;
using Features.OthersDrop.UI.Panels;
using Features.GameHub.UI.Panels;
using QCore.UISystem.Panels;
using QCore.UISystem.Presenters;
using QCore.UISystem.Services;
using Features.Wallet.Services;

namespace Features.GameHub.UI.Presenters
{
    public class GameHubPanelPresenter : PanelPresenter<GameHubPanel>
    {
        private readonly IUIPanelsService _panelsService;
        private readonly SoftCurrencyWallet _softCurrencyWallet;
        
        private Panel _openedChildPanel;

        public GameHubPanelPresenter(IUIPanelsService panelsService, SoftCurrencyWallet softCurrencyWallet)
        {
            _panelsService = panelsService;
            _softCurrencyWallet = softCurrencyWallet;
        }
        
        protected override void StartPresentingInternal()
        {
            Panel.CasesClickHandle += CasesClickHandle;
            Panel.InventoryClickHandle += InventoryClickHandle;

            _softCurrencyWallet.Changed += SoftCurrencyWalletChangeHandle;
            
            Panel.SetSoftCurrency(_softCurrencyWallet.Currency);

            _panelsService.Open(() => Panel.OthersDropBar);
        }

        private void SoftCurrencyWalletChangeHandle(IWallet sender, float delta, float current)
        {
            Panel.SetSoftCurrency(current);
        }

        private void InventoryClickHandle()
        {
            CloseActiveChildPanel();
            
            var data = _panelsService.Open(() => Panel.InventorySubPanel);
            _openedChildPanel = data.Panel;
        }

        private void CasesClickHandle()
        {
            CloseActiveChildPanel();
            
            var data = _panelsService.Open(() => Panel.CasesSubPanel);
            _openedChildPanel = data.Panel;
        }

        private void CloseActiveChildPanel()
        {
            if (_openedChildPanel != null)
            {
                _panelsService.Close(_openedChildPanel.GetType());
            }
        }

        protected override void StopPresentingInternal()
        {
            Panel.CasesClickHandle -= CasesClickHandle;
            Panel.InventoryClickHandle -= InventoryClickHandle;
            _softCurrencyWallet.Changed -= SoftCurrencyWalletChangeHandle;

            CloseActiveChildPanel();
            
            _panelsService.Close<OthersDropBar>();
        }
    }
}
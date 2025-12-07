using GameHub.Cases.Panels;
using GameHub.Inventory.Panels;
using GameHub.Root.Panels;
using QCore.UISystem.Panels;
using QCore.UISystem.Presenters;
using QCore.UISystem.Services;
using Zenject;

namespace GameHub.Root.Presenters
{
    public class GameHubPanelPresenter : PanelPresenter<GameHubPanel>
    {
        [Inject] private IUIPanelsService _panelsService;

        private Panel _openedChildPanel;
        
        protected override void StartPresentingInternal()
        {
            Panel.CasesClickHandle += CasesClickHandle;
            Panel.InventoryClickHandle += InventoryClickHandle;
        }

        private void InventoryClickHandle()
        {
            CloseActiveChildPanel();
            
            PanelAndPresenterData<InventorySubPanel> data = _panelsService.Open(() => Panel.InventorySubPanel);
            _openedChildPanel = data.Panel;
        }

        private void CasesClickHandle()
        {
            CloseActiveChildPanel();
            
            PanelAndPresenterData<CasesSubPanel> data = _panelsService.Open(() => Panel.CasesSubPanel);
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

            CloseActiveChildPanel();
        }
    }
}
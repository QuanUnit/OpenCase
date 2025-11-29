using GameHub.Cases.Presenters;
using GameHub.Root.Panels;
using QCore.UISystem.Presenters;
using QCore.UISystem.Services;
using Zenject;

namespace GameHub.Root.Presenters
{
    public class GameHubPanelPresenter : PanelPresenter<GameHubPanel>
    {
        [Inject] private IUIPanelsService _panelsService;
        
        protected override void StartPresentingInternal()
        {
            Panel.CasesClickHandle += CasesClickHandle;
        }

        private void CasesClickHandle()
        {
            _panelsService.Open(() => Panel.CasesSubPanel);
        }

        protected override void StopPresentingInternal()
        {
            Panel.CasesClickHandle -= CasesClickHandle;
        }
    }
}
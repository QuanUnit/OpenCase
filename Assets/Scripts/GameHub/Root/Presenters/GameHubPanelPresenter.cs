using GameHub.Root.Panels;
using QCore.UISystem.Presenters;

namespace GameHub.Root.Presenters
{
    public class GameHubPanelPresenter : PanelPresenter<GameHubPanel>
    {
        
        
        protected override void StartPresentingInternal()
        {
            Panel.CasesClickHandle += CasesClickHandle;
        }

        private void CasesClickHandle()
        {
            
        }

        protected override void StopPresentingInternal()
        {
            Panel.CasesClickHandle -= CasesClickHandle;
        }
    }
}
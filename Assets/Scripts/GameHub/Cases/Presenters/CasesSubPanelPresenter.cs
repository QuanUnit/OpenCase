using GameHub.Cases.Panels;
using QCore.Tools.Identifiers;
using QCore.UISystem.Panels;
using QCore.UISystem.Presenters;
using QCore.UISystem.Services;
using Services.Cases;
using Zenject;

namespace GameHub.Cases.Presenters
{
    public class CasesSubPanelPresenter : PanelPresenter<CasesSubPanel>
    {
        [Inject] private IUIPanelsService _panelsService;
        [Inject] private ICasesDatabaseService _casesDatabaseService;
        
        private Panel _openedChildPanel;
        
        protected override void StartPresentingInternal()
        {
            Panel.CaseOpenClicked += CaseOpenClickHandle;
            
            Panel.ClearCasesViews();
            Panel.CreateCasesViews(_casesDatabaseService.AllCases);
        }

        private void CaseOpenClickHandle(QGuid caseId)
        {
            PanelAndPresenterData<CaseSpinSubPanel> data = _panelsService.OpenWithPayload(() => Panel.CaseSpinSubPanel, caseId);
            _openedChildPanel = data.Panel;
        }

        protected override void StopPresentingInternal()
        {
            if (_openedChildPanel != null)
            {
                _panelsService.Close(_openedChildPanel.GetType());
            }
            
            Panel.CaseOpenClicked -= CaseOpenClickHandle;
        }
    }
}
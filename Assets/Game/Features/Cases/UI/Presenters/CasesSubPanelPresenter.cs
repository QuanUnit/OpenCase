using Features.Cases.UI.Panels;
using Features.Cases.Data;
using QCore.UISystem.Panels;
using QCore.UISystem.Presenters;
using QCore.UISystem.Services;
using Features.Cases.Services;

namespace Features.Cases.UI.Presenters
{
    public class CasesSubPanelPresenter : PanelPresenter<CasesSubPanel>
    {
        private readonly IUIPanelsService _panelsService;
        private readonly ICasesDatabaseService _casesDatabaseService;
        
        private Panel _openedChildPanel;

        public CasesSubPanelPresenter(IUIPanelsService panelsService, ICasesDatabaseService casesDatabaseService)
        {
            _panelsService = panelsService;
            _casesDatabaseService = casesDatabaseService;
        }
        
        protected override void StartPresentingInternal()
        {
            Panel.CaseOpenClicked += CaseOpenClickHandle;
            
            Panel.ClearCasesViews();
            Panel.CreateCasesViews(_casesDatabaseService.AllCases);
        }

        private void CaseOpenClickHandle(CaseId caseId)
        {
            var data = _panelsService.OpenWithPayload(() => Panel.CaseSpinSubPanel, caseId);
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
using Features.Cases.Services;
using Features.Cases.Configs;
using Features.Cases.UI.Panels;
using Features.Cases.UI.Presenters;
using Shared.Lifecycle;
using Shared.UI.PanelPresenters;
using UnityEngine;
using Zenject;

namespace Features.Cases.Installers
{
    public class CasesInstaller : MonoInstaller
    {
        [SerializeField] private AllCasesConfig _allCasesConfig;
        
        public override void InstallBindings()
        {
            Container.BindInstance(_allCasesConfig).AsSingle();

            Container.Bind(typeof(ICasesDatabaseService), typeof(IInitializableService), typeof(IAssetsLoader)).To<CasesDatabaseService>().AsSingle();

            Container.BindPanelPresenter<CasesSubPanel, CasesSubPanelPresenter>();
            Container.BindPanelPresenter<CaseSpinSubPanel, CaseSpinSubPanelPresenter>();
        }
    }
}

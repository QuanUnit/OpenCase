using Features.Upgrade.Configs;
using Features.Upgrade.Services;
using Features.Upgrade.UI.Panels;
using Features.Upgrade.UI.Presenters;
using Shared.UI.PanelPresenters;
using UnityEngine;
using Zenject;

namespace Features.Upgrade.Installers
{
    public class UpgradeInstaller : MonoInstaller
    {
        [SerializeField] private UpgradeConfig _upgradeConfig;

        public override void InstallBindings()
        {
            Container.BindInstance(_upgradeConfig).AsSingle();

            Container.Bind<IChanceRoller>().To<ChanceRoller>().AsSingle();
            Container.Bind<IUpgradeService>().To<UpgradeService>().AsSingle();

            Container.BindPanelPresenter<UpgradeSubPanel, UpgradeSubPanelPresenter>();
        }
    }
}

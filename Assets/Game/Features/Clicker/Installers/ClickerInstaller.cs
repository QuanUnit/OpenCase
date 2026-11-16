using Features.Clicker.Configs;
using Features.Clicker.Services;
using Features.Clicker.UI.Panels;
using Features.Clicker.UI.Presenters;
using Shared.UI.PanelPresenters;
using UnityEngine;
using Zenject;

namespace Features.Clicker.Installers
{
    public class ClickerInstaller : MonoInstaller
    {
        [SerializeField] private ClickerConfig _clickerConfig;

        public override void InstallBindings()
        {
            Container.BindInstance(_clickerConfig).AsSingle();

            Container.Bind<IClickerService>().To<ClickerService>().AsSingle();

            Container.BindPanelPresenter<ClickerSubPanel, ClickerSubPanelPresenter>();
        }
    }
}

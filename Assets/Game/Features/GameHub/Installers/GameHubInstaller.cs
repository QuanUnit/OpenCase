using Features.GameHub.UI.Panels;
using Features.GameHub.UI.Presenters;
using Shared.UI.PanelPresenters;
using Zenject;

namespace Features.GameHub.Installers
{
    public class GameHubInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindPanelPresenter<GameHubPanel, GameHubPanelPresenter>();
        }
    }
}

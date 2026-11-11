using Features.OthersDrop.UI.Panels;
using Features.OthersDrop.UI.Presenters;
using Shared.UI.PanelPresenters;
using Zenject;

namespace Features.OthersDrop.Installers
{
    public class OthersDropInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindPanelPresenter<OthersDropBar, OthersDropBarPresenter>();
        }
    }
}

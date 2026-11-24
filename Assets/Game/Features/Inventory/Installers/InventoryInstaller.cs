using Features.Inventory.Services;
using Features.Inventory.UI.Panels;
using Features.Inventory.UI.Presenters;
using Shared.UI.PanelPresenters;
using Zenject;

namespace Features.Inventory.Installers
{
    public class InventoryInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IInventoryService>().To<InventoryService>().AsSingle();
            Container.Bind<ISkinSellService>().To<SkinSellService>().AsSingle();

            Container.BindPanelPresenter<InventorySubPanel, InventorySubPanelPresenter>();
            Container.BindPanelPresenter<SkinDetailsPopup, SkinDetailsPopupPresenter>();
        }
    }
}

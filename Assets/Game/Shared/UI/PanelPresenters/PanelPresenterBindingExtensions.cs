using QCore.UISystem.Panels;
using QCore.UISystem.Presenters;
using Zenject;

namespace Shared.UI.PanelPresenters
{
    public static class PanelPresenterBindingExtensions
    {
        public static void BindPanelPresenter<TPanel, TPresenter>(this DiContainer container)
            where TPanel : Panel
            where TPresenter : IPanelPresenter<TPanel>
        {
            var binding = new PanelPresenterBinding(typeof(TPanel), typeof(TPresenter));

            container.Bind<PanelPresenterBinding>().FromInstance(binding).AsCached();
        }
    }
}

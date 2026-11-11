using System;

namespace Shared.UI.PanelPresenters
{
    public sealed class PanelPresenterBinding
    {
        public Type PanelType { get; }
        public Type PresenterType { get; }

        public PanelPresenterBinding(Type panelType, Type presenterType)
        {
            PanelType = panelType;
            PresenterType = presenterType;
        }
    }
}

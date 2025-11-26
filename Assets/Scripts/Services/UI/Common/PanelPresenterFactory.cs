using System;
using System.Collections.Generic;
using GameHub.Root.Panels;
using GameHub.Root.Presenters;
using QCore.UISystem.Panels;
using QCore.UISystem.Presenters;
using QCore.UISystem.Services.Factories;
using Zenject;

namespace Services.UI.Common
{
    public class PanelPresenterFactory : IPanelPresenterFactory
    {
        [Inject] private DiContainer _container;

        private Dictionary<Type, Type> _panelTypeToPresenterType = new Dictionary<Type, Type>()
        {
            { typeof(GameHubPanel), typeof(GameHubPanelPresenter) },
        };
        
        public IPanelPresenter<T> Create<T>(T panel) where T : Panel
        {
            return (IPanelPresenter<T>)Create((Panel)panel);
        }

        public IPanelPresenter Create(Panel panel)
        {
            Type panelType = panel.GetType();
            IPanelPresenter presenter = CreateInternal(panelType);
            return presenter;
        }

        private IPanelPresenter CreateInternal(Type panelType)
        {
            Type presenterType = _panelTypeToPresenterType[panelType];

            return (IPanelPresenter)_container.Instantiate(presenterType);
        }
    }
}
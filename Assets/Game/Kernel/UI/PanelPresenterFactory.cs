using System;
using System.Collections.Generic;
using QCore.UISystem.Panels;
using QCore.UISystem.Presenters;
using QCore.UISystem.Services.Factories;
using Shared.UI.PanelPresenters;
using Zenject;

namespace Kernel.UI
{
    public class PanelPresenterFactory : IPanelPresenterFactory
    {
        private readonly DiContainer _container;
        private readonly Dictionary<Type, Type> _panelTypeToPresenterType = new Dictionary<Type, Type>();

        public PanelPresenterFactory(DiContainer container, List<PanelPresenterBinding> bindings)
        {
            _container = container;

            foreach (var binding in bindings)
            {
                _panelTypeToPresenterType.Add(binding.PanelType, binding.PresenterType);
            }
        }

        public IPanelPresenter<T> Create<T>(T panel) where T : Panel
        {
            return (IPanelPresenter<T>)Create((Panel)panel);
        }

        public IPanelPresenter Create(Panel panel)
        {
            var panelType = panel.GetType();
            var presenter = CreateInternal(panelType);

            return presenter;
        }

        private IPanelPresenter CreateInternal(Type panelType)
        {
            if (_panelTypeToPresenterType.TryGetValue(panelType, out var presenterType) == false)
            {
                throw new InvalidOperationException($"No presenter bound for panel {panelType.Name}");
            }

            return (IPanelPresenter)_container.Instantiate(presenterType);
        }
    }
}

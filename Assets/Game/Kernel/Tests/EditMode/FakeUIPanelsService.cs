using System;
using System.Collections.Generic;
using QCore.UISystem.Panels;
using QCore.UISystem.Services;

namespace Kernel.Tests.EditMode
{
    public class FakeUIPanelsService : IUIPanelsService
    {
        private readonly List<string> _callOrder;

        public int InitCount { get; private set; }

        public FakeUIPanelsService(List<string> callOrder)
        {
            _callOrder = callOrder;
        }

        public void Init()
        {
            InitCount++;
            _callOrder.Add("panels");
        }

        public PanelAndPresenterData<T> Open<T>(string rootName = null, bool useAnimation = true) where T : Panel
        {
            throw new NotImplementedException();
        }

        public PanelAndPresenterData<T> Open<T>(Func<T> panelGetter, bool useAnimation = true) where T : Panel
        {
            throw new NotImplementedException();
        }

        public PanelAndPresenterData Open(Type panelType, string rootName = null, bool useAnimation = true)
        {
            throw new NotImplementedException();
        }

        public PanelAndPresenterData Open(Func<Panel> panelGetter, bool useAnimation = true)
        {
            throw new NotImplementedException();
        }

        public PanelAndPresenterData<TPanel> OpenWithPayload<TPanel, TPayload>(TPayload payload, string rootName = null, bool useAnimation = true) where TPanel : Panel
        {
            throw new NotImplementedException();
        }

        public PanelAndPresenterData<TPanel> OpenWithPayload<TPanel, TPayload>(Func<TPanel> panelGetter, TPayload payload, bool useAnimation = true) where TPanel : Panel
        {
            throw new NotImplementedException();
        }

        public PanelAndPresenterData OpenWithPayload<TPayload>(Type panelType, TPayload payload, string rootName = null, bool useAnimation = true)
        {
            throw new NotImplementedException();
        }

        public PanelAndPresenterData OpenWithPayload<TPayload>(Func<Panel> panelGetter, TPayload payload, bool useAnimation = true)
        {
            throw new NotImplementedException();
        }

        public PanelAndPresenterData<T> Get<T>() where T : Panel
        {
            throw new NotImplementedException();
        }

        public PanelAndPresenterData Get(Type panelType)
        {
            throw new NotImplementedException();
        }

        public bool TryGet<T>(out PanelAndPresenterData<T> found) where T : Panel
        {
            throw new NotImplementedException();
        }

        public bool TryGet(Type panelType, out PanelAndPresenterData found)
        {
            throw new NotImplementedException();
        }

        public PanelAndPresenterData GetOrOpen(Type panelType, string rootName = null, bool useAnimation = true)
        {
            throw new NotImplementedException();
        }

        public PanelAndPresenterData<T> GetOrOpen<T>(string rootName = null, bool useAnimation = true) where T : Panel
        {
            throw new NotImplementedException();
        }

        public void Close<T>(bool useAnimation = true) where T : Panel
        {
            throw new NotImplementedException();
        }

        public void Close(Type type, bool useAnimation = true)
        {
            throw new NotImplementedException();
        }

        public void CloseAll(bool useAnimation = true)
        {
            throw new NotImplementedException();
        }
    }
}

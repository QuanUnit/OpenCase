using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Kernel.UI;
using NUnit.Framework;
using Shared.UI.PanelPresenters;
using Zenject;

namespace Kernel.Tests.EditMode
{
    public class PanelPresenterFactoryTests
    {
        private FakePanel CreatePanel()
        {
            return (FakePanel)FormatterServices.GetUninitializedObject(typeof(FakePanel));
        }

        private PanelPresenterFactory CreateFactory(List<PanelPresenterBinding> bindings)
        {
            var container = new DiContainer();

            return new PanelPresenterFactory(container, bindings);
        }

        [Test]
        public void Create_BoundPanel_ReturnsPresenterOfBoundType()
        {
            var bindings = new List<PanelPresenterBinding>
            {
                new PanelPresenterBinding(typeof(FakePanel), typeof(FakePanelPresenter))
            };
            var factory = CreateFactory(bindings);
            var panel = CreatePanel();

            var presenter = factory.Create(panel);

            Assert.IsInstanceOf<FakePanelPresenter>(presenter);
        }

        [Test]
        public void Create_BoundThroughContainerExtension_ReturnsPresenterOfBoundType()
        {
            var container = new DiContainer();
            container.BindPanelPresenter<FakePanel, FakePanelPresenter>();
            container.BindPanelPresenter<OtherFakePanel, OtherFakePanelPresenter>();
            container.Bind<PanelPresenterFactory>().AsSingle();
            var factory = container.Resolve<PanelPresenterFactory>();
            var panel = CreatePanel();

            var presenter = factory.Create(panel);

            Assert.IsInstanceOf<FakePanelPresenter>(presenter);
        }

        [Test]
        public void Create_UnboundPanel_ThrowsWithPanelTypeName()
        {
            var bindings = new List<PanelPresenterBinding>
            {
                new PanelPresenterBinding(typeof(OtherFakePanel), typeof(OtherFakePanelPresenter))
            };
            var factory = CreateFactory(bindings);
            var panel = CreatePanel();

            var exception = Assert.Throws<InvalidOperationException>(() => factory.Create(panel));

            Assert.AreEqual("No presenter bound for panel FakePanel", exception.Message);
        }

        [Test]
        public void Constructor_DuplicatePanelBinding_Throws()
        {
            var bindings = new List<PanelPresenterBinding>
            {
                new PanelPresenterBinding(typeof(FakePanel), typeof(FakePanelPresenter)),
                new PanelPresenterBinding(typeof(FakePanel), typeof(FakePanelPresenter))
            };

            Assert.Throws<ArgumentException>(() => CreateFactory(bindings));
        }
    }
}

using System.Collections.Generic;
using Kernel.Flow.States;
using NUnit.Framework;
using Shared.Lifecycle;

namespace Kernel.Tests.EditMode
{
    public class BootstrapStateTests
    {
        [Test]
        public void InitServices_InitsPanelsThenEveryService()
        {
            var callOrder = new List<string>();
            var panelsService = new FakeUIPanelsService(callOrder);
            var firstService = new FakeInitializableService("first", callOrder);
            var secondService = new FakeInitializableService("second", callOrder);
            var services = new List<IInitializableService> { firstService, secondService };
            var state = new BootstrapState(panelsService, services);

            state.InitServices();

            Assert.AreEqual(1, panelsService.InitCount);
            Assert.AreEqual(1, firstService.InitCount);
            Assert.AreEqual(1, secondService.InitCount);
            CollectionAssert.AreEqual(new List<string> { "panels", "first", "second" }, callOrder);
        }
    }
}

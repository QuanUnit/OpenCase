using System.Collections.Generic;
using Kernel.Flow.States;
using NUnit.Framework;
using Shared.Lifecycle;

namespace Kernel.Tests.EditMode
{
    public class CleaningSessionStateTests
    {
        [Test]
        public void Enter_ReleasesEveryLoaderOnce()
        {
            var firstLoader = new FakeAssetsLoader();
            var secondLoader = new FakeAssetsLoader();
            var loaders = new List<IAssetsLoader> { firstLoader, secondLoader };
            var state = new CleaningSessionState(loaders);

            state.Enter();

            Assert.AreEqual(1, firstLoader.ReleaseCount);
            Assert.AreEqual(1, secondLoader.ReleaseCount);
        }
    }
}

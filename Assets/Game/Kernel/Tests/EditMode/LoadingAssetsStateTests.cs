using System.Collections.Generic;
using Kernel.Flow.States;
using NUnit.Framework;
using Shared.Lifecycle;

namespace Kernel.Tests.EditMode
{
    public class LoadingAssetsStateTests
    {
        [Test]
        public void LoadAssets_AwaitsEveryLoader()
        {
            var firstLoader = new FakeAssetsLoader();
            var secondLoader = new FakeAssetsLoader();
            var loaders = new List<IAssetsLoader> { firstLoader, secondLoader };
            var state = new LoadingAssetsState(loaders);

            state.LoadAssets().GetAwaiter().GetResult();

            Assert.AreEqual(1, firstLoader.LoadCount);
            Assert.AreEqual(1, secondLoader.LoadCount);
        }
    }
}

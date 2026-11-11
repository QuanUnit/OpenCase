using System.Collections.Generic;
using NUnit.Framework;
using Shared.Tools.Collections;

namespace Shared.Tests.EditMode
{
    public class CollectionsExtensionsTests
    {
        [Test]
        public void GetRandomReadOnly_ManyCalls_ReturnsElementOfList()
        {
            var source = new List<int> { 1, 2, 3 };

            for (var index = 0; index < 100; index++)
            {
                Assert.IsTrue(source.Contains(source.GetRandomReadOnly()) == true);
            }
        }

        [Test]
        public void GetRandomReadOnly_SingleElement_ReturnsIt()
        {
            var source = new List<int> { 42 };

            Assert.AreEqual(42, source.GetRandomReadOnly());
        }
    }
}

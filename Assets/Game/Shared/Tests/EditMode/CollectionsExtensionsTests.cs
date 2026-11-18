using System.Collections.Generic;
using System.Linq;
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

        [Test]
        public void IndexOfMax_SeveralElements_ReturnsIndexOfLargestValue()
        {
            var source = new List<float> { 0.5f, 3f, 1.2f };

            Assert.AreEqual(1, source.IndexOfMax(value => value));
        }

        [Test]
        public void IndexOfMax_EqualValues_ReturnsFirstIndex()
        {
            var source = new List<float> { 2f, 2f };

            Assert.AreEqual(0, source.IndexOfMax(value => value));
        }

        [Test]
        public void IndexOfMax_NegativeValues_ReturnsIndexOfLargestValue()
        {
            var source = new List<float> { -5f, -1f, -3f };

            Assert.AreEqual(1, source.IndexOfMax(value => value));
        }

        [Test]
        public void IndexOfMax_EmptyList_ReturnsMinusOne()
        {
            var source = new List<float>();

            Assert.AreEqual(-1, source.IndexOfMax(value => value));
        }

        [Test]
        public void Shuffle_List_KeepsSameElements()
        {
            var source = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };

            source.Shuffle();

            CollectionAssert.AreEquivalent(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 }, source);
        }

        [Test]
        public void Shuffle_ManyCalls_ChangesOrderAtLeastOnce()
        {
            var original = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
            var isChanged = false;

            for (var attempt = 0; attempt < 20; attempt++)
            {
                var source = new List<int>(original);
                source.Shuffle();

                if (source.SequenceEqual(original) == false)
                {
                    isChanged = true;
                }
            }

            Assert.IsTrue(isChanged == true);
        }
    }
}

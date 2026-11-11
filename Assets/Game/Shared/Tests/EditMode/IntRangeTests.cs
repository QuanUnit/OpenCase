using NUnit.Framework;
using Shared.Tools.Mathematics;

namespace Shared.Tests.EditMode
{
    public class IntRangeTests
    {
        [TestCase(1, true)]
        [TestCase(5, true)]
        [TestCase(0, false)]
        [TestCase(6, false)]
        public void IsInside_Boundaries_ReturnsExpected(int value, bool expected)
        {
            var range = new IntRange(1, 5);

            Assert.AreEqual(expected, range.IsInside(value));
        }

        [Test]
        public void Random_ManyCalls_StaysInsideAndHitsBothEdges()
        {
            var range = new IntRange(1, 5);
            var hitMin = false;
            var hitMax = false;

            for (var index = 0; index < 200; index++)
            {
                var value = range.Random();

                Assert.IsTrue(range.IsInside(value) == true);

                if (value == 1)
                {
                    hitMin = true;
                }

                if (value == 5)
                {
                    hitMax = true;
                }
            }

            Assert.IsTrue(hitMin == true);
            Assert.IsTrue(hitMax == true);
        }
    }
}

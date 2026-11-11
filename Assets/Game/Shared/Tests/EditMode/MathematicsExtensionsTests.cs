using System;
using NUnit.Framework;
using Shared.Tools.Mathematics;

namespace Shared.Tests.EditMode
{
    public class MathematicsExtensionsTests
    {
        [TestCase(-0.1f)]
        [TestCase(1.1f)]
        public void IsProke_OutOfRange_Throws(float value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => value.IsProke());
        }

        [Test]
        public void IsProke_One_AlwaysTrue()
        {
            for (var index = 0; index < 100; index++)
            {
                Assert.IsTrue(1f.IsProke() == true);
            }
        }
    }
}

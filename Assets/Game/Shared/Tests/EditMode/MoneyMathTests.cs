using NUnit.Framework;
using Shared.Tools.Mathematics;

namespace Shared.Tests.EditMode
{
    public class MoneyMathTests
    {
        [TestCase(0.3899999f, 0.39f)]
        [TestCase(1.9999985f, 2f)]
        [TestCase(0.124f, 0.12f)]
        [TestCase(0.126f, 0.13f)]
        [TestCase(0f, 0f)]
        public void RoundToMoney_Value_ReturnsNearestCent(float value, float expected)
        {
            var result = MoneyMath.RoundToMoney(value);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RoundToMoney_RepeatedCentSteps_StayOnCentGrid()
        {
            var value = 0f;

            for (var index = 1; index <= 200; index++)
            {
                value = MoneyMath.RoundToMoney(value + 0.01f);

                Assert.AreEqual(index / 100f, value);
            }
        }

        [Test]
        public void FloorToMoney_FractionalCents_RoundsDown()
        {
            var result = MoneyMath.FloorToMoney(25.555f);

            Assert.AreEqual(25.55f, result);
        }

        [TestCase(0.01f, 1)]
        [TestCase(0.0099999f, 1)]
        [TestCase(0.39f, 39)]
        [TestCase(0.004f, 0)]
        public void ToMinorUnits_Value_ReturnsRoundedCents(float value, int expected)
        {
            var result = MoneyMath.ToMinorUnits(value);

            Assert.AreEqual(expected, result);
        }
    }
}

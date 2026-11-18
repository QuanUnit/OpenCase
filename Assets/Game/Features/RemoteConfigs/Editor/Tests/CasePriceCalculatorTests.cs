using System;
using Features.RemoteConfigs.Editor.Cases;
using NUnit.Framework;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class CasePriceCalculatorTests
    {
        private CasePriceCalculator _calculator;

        [SetUp]
        public void SetUp()
        {
            _calculator = new CasePriceCalculator(0.01f);
        }

        [TestCase(0.2723d, 0.9d, 0.3f)]
        [TestCase(9.1d, 0.9d, 10.11f)]
        [TestCase(0.225d, 1d, 0.23f)]
        [TestCase(450d, 0.88d, 511.36f)]
        public void GetPriceForReturnToPlayer_RoundsToStep(double expectedValue, double targetReturnToPlayer, float expectedPrice)
        {
            var price = _calculator.GetPriceForReturnToPlayer(expectedValue, targetReturnToPlayer);

            Assert.That(price, Is.EqualTo(expectedPrice).Within(1e-5f));
        }

        [Test]
        public void GetPriceForReturnToPlayer_TinyValue_ReturnsOneStep()
        {
            var price = _calculator.GetPriceForReturnToPlayer(0.0001d, 0.9d);

            Assert.That(price, Is.EqualTo(0.01f).Within(1e-6f));
        }

        [Test]
        public void GetPriceForReturnToPlayer_ZeroTarget_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _calculator.GetPriceForReturnToPlayer(1d, 0d));
        }
    }
}

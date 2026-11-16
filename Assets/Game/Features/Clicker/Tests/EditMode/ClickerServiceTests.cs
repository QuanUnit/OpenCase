using Features.Clicker.Services;
using Features.Wallet.Services;
using NUnit.Framework;
using Shared.Tools.Mathematics;

namespace Features.Clicker.Tests.EditMode
{
    public class ClickerServiceTests
    {
        private const float Tolerance = 0.0001f;

        private SoftCurrencyWallet CreateWallet(float currency)
        {
            var wallet = new SoftCurrencyWallet();

            if (currency > 0f)
            {
                wallet.Add(currency);
            }

            return wallet;
        }

        private ClickerService CreateService(SoftCurrencyWallet wallet)
        {
            var config = new ClickerConfigBuilder()
                .WithRewardPerClick(0.01f)
                .WithParticles(4f, 3, 12)
                .Build();

            return new ClickerService(wallet, config);
        }

        [TestCase(0f)]
        [TestCase(2f)]
        [TestCase(5000f)]
        public void Earn_AnyBalance_AddsRewardAndReturnsIt(float currency)
        {
            var wallet = CreateWallet(currency);
            var service = CreateService(wallet);
            var changesCount = 0;
            wallet.Changed += (sender, delta, current) => changesCount++;

            var reward = service.Earn();

            Assert.AreEqual(1, MoneyMath.ToMinorUnits(reward));
            Assert.AreEqual(MoneyMath.RoundToMoney(currency + 0.01f), wallet.Currency);
            Assert.AreEqual(1, changesCount);
        }

        [Test]
        public void Earn_39TimesFromZero_AffordsPrice039()
        {
            var wallet = CreateWallet(0f);
            var service = CreateService(wallet);

            for (var index = 0; index < 39; index++)
            {
                service.Earn();
            }

            Assert.IsTrue(wallet.IsEnoughForSpend(0.39f) == true);
        }

        [Test]
        public void Earn_RepeatedFromZero_StaysOnCentGrid()
        {
            var wallet = CreateWallet(0f);
            var service = CreateService(wallet);

            for (var index = 1; index <= 300; index++)
            {
                service.Earn();

                Assert.AreEqual(index / 100f, wallet.Currency);
            }
        }

        [Test]
        public void Earn_OffCentBalance_SnapsBalanceToCentGrid()
        {
            var wallet = CreateWallet(0.3899999f);
            var service = CreateService(wallet);

            service.Earn();

            Assert.AreEqual(0.4f, wallet.Currency);
        }

        [TestCase(0.01f, 4)]
        [TestCase(0.02f, 8)]
        [TestCase(0.005f, 3)]
        [TestCase(1f, 12)]
        public void GetParticlesCount_ByAmount_ScalesAndClamps(float amount, int expected)
        {
            var service = CreateService(CreateWallet(0f));

            var count = service.GetParticlesCount(amount);

            Assert.AreEqual(expected, count);
        }

        [Test]
        public void HoldInterval_ReturnsConfigValue()
        {
            var service = CreateService(CreateWallet(0f));

            Assert.AreEqual(0.15f, service.HoldInterval, Tolerance);
        }
    }
}

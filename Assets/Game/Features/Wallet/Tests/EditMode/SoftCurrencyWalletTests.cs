using System;
using System.Collections.Generic;
using Features.Wallet.Services;
using NUnit.Framework;

namespace Features.Wallet.Tests.EditMode
{
    public class SoftCurrencyWalletTests
    {
        [Test]
        public void Add_PositiveValue_IncreasesCurrency()
        {
            var wallet = new SoftCurrencyWallet();

            wallet.Add(10f);

            Assert.AreEqual(10f, wallet.Currency);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void Add_NonPositiveValue_Throws(float value)
        {
            var wallet = new SoftCurrencyWallet();

            Assert.Throws<InvalidOperationException>(() => wallet.Add(value));
        }

        [Test]
        public void Spend_NotEnough_Throws()
        {
            var wallet = new SoftCurrencyWallet();
            wallet.Add(5f);

            Assert.Throws<InvalidOperationException>(() => wallet.Spend(10f));
            Assert.AreEqual(5f, wallet.Currency);
        }

        [Test]
        public void TrySpend_NotEnough_ReturnsFalseAndKeepsCurrency()
        {
            var wallet = new SoftCurrencyWallet();
            wallet.Add(5f);

            var result = wallet.TrySpend(10f);

            Assert.IsTrue(result == false);
            Assert.AreEqual(5f, wallet.Currency);
        }

        [Test]
        public void TrySpend_Enough_ReturnsTrueAndDecreases()
        {
            var wallet = new SoftCurrencyWallet();
            wallet.Add(10f);

            var result = wallet.TrySpend(4f);

            Assert.IsTrue(result == true);
            Assert.AreEqual(6f, wallet.Currency);
        }

        [Test]
        public void Changed_OnAddAndSpend_PassesDeltaAndCurrent()
        {
            var wallet = new SoftCurrencyWallet();
            var records = new List<(float Delta, float Current)>();
            wallet.Changed += (sender, delta, current) => records.Add((delta, current));

            wallet.Add(10f);
            wallet.Spend(3f);

            Assert.AreEqual(2, records.Count);
            Assert.AreEqual((10f, 10f), records[0]);
            Assert.AreEqual((-3f, 7f), records[1]);
        }
    }
}

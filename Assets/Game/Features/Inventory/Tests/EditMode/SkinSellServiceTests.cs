using System.Collections.Generic;
using Features.Inventory.Services;
using Features.Skins.Data;
using Features.Wallet.Services;
using NUnit.Framework;
using QCore.Tools.Identifiers;
using Shared.Tools.Mathematics;
using Shared.UI.Effects;

namespace Features.Inventory.Tests.EditMode
{
    public class SkinSellServiceTests
    {
        private const float Tolerance = 0.0001f;

        private InventoryService _inventory;
        private SoftCurrencyWallet _wallet;
        private SkinSellService _service;

        [SetUp]
        public void SetUp()
        {
            _inventory = new InventoryService();
            _wallet = new SoftCurrencyWallet();
            _service = new SkinSellService(_inventory, _wallet);
        }

        private Skin CreateSkin(float price)
        {
            return new Skin(QGuid.CreateNew(), QGuid.CreateNew(), 0.1f, price, false, false);
        }

        private int ParseDisplayedCents(string displayed)
        {
            var digits = displayed
                .Replace(NumberCounterFormatter.MoneyPostfix, string.Empty)
                .Replace(".", string.Empty);

            return int.Parse(digits);
        }

        private Skin AddSkin(float price)
        {
            var skin = CreateSkin(price);
            _inventory.Add(skin);

            return skin;
        }

        [Test]
        public void Sell_SkinInInventory_RemovesSkinAndAddsRoundedPrice()
        {
            var skin = AddSkin(12.346f);

            var amount = _service.Sell(skin);

            Assert.AreEqual(12.35f, amount, Tolerance);
            Assert.AreEqual(12.35f, _wallet.Currency, Tolerance);
            Assert.IsTrue(_inventory.TryGet(skin.Id, out _) == false);
        }

        [Test]
        public void Sell_SkinNotInInventory_ReturnsZeroAndKeepsWallet()
        {
            var skin = CreateSkin(5f);

            var amount = _service.Sell(skin);

            Assert.AreEqual(0f, amount, Tolerance);
            Assert.AreEqual(0f, _wallet.Currency, Tolerance);
        }

        [Test]
        public void Sell_SameSkinTwice_SecondReturnsZero()
        {
            var skin = AddSkin(5f);

            _service.Sell(skin);
            var secondAmount = _service.Sell(skin);

            Assert.AreEqual(0f, secondAmount, Tolerance);
            Assert.AreEqual(5f, _wallet.Currency, Tolerance);
        }

        [Test]
        public void Sell_ZeroPrice_RemovesSkinWithoutWalletChange()
        {
            var skin = AddSkin(0.001f);
            var changesCount = 0;
            _wallet.Changed += (sender, delta, current) => changesCount++;

            var amount = _service.Sell(skin);

            Assert.AreEqual(0f, amount, Tolerance);
            Assert.AreEqual(0, changesCount);
            Assert.IsTrue(_inventory.TryGet(skin.Id, out _) == false);
        }

        [Test]
        public void SellAll_SkinsInInventory_RemovesAllAndAddsRoundedSumOnce()
        {
            var skins = new List<Skin>()
            {
                AddSkin(1.004f),
                AddSkin(2.006f),
                AddSkin(0.114f),
            };
            var deltas = new List<float>();
            _wallet.Changed += (sender, delta, current) => deltas.Add(delta);

            var amount = _service.SellAll(skins);

            Assert.AreEqual(3.12f, amount, Tolerance);
            Assert.AreEqual(1, deltas.Count);
            Assert.AreEqual(3.12f, deltas[0], Tolerance);

            foreach (var skin in skins)
            {
                Assert.IsTrue(_inventory.TryGet(skin.Id, out _) == false);
            }
        }

        [Test]
        public void SellAll_SomeSkinsMissing_SellsOnlySkinsInInventory()
        {
            var soldBefore = AddSkin(4f);
            var kept = AddSkin(3f);
            var missing = CreateSkin(10f);
            _service.Sell(soldBefore);

            var amount = _service.SellAll(new List<Skin>() { soldBefore, kept, missing });

            Assert.AreEqual(3f, amount, Tolerance);
            Assert.AreEqual(7f, _wallet.Currency, Tolerance);
        }

        [Test]
        public void SellAll_DuplicateSkin_SellsItOnce()
        {
            var skin = AddSkin(2f);

            var amount = _service.SellAll(new List<Skin>() { skin, skin });

            Assert.AreEqual(2f, amount, Tolerance);
            Assert.AreEqual(2f, _wallet.Currency, Tolerance);
        }

        [Test]
        public void SellAll_Empty_ReturnsZeroWithoutWalletChange()
        {
            var changesCount = 0;
            _wallet.Changed += (sender, delta, current) => changesCount++;

            var amount = _service.SellAll(new List<Skin>());

            Assert.AreEqual(0f, amount, Tolerance);
            Assert.AreEqual(0, changesCount);
        }

        [TestCase(0.125f)]
        [TestCase(1.375f)]
        [TestCase(0.115f)]
        [TestCase(576.665f)]
        public void GetSellPrice_Midpoint_DisplayedEqualsCredited(float price)
        {
            var skin = AddSkin(price);
            var displayed = NumberCounterFormatter.FormatMoney(_service.GetSellPrice(skin));

            var amount = _service.Sell(skin);

            Assert.AreEqual(displayed, NumberCounterFormatter.FormatMoney(amount));
            Assert.AreEqual(displayed, NumberCounterFormatter.FormatMoney(_wallet.Currency));
        }

        [Test]
        public void GetSellPrice_TwentySkins_EqualsSellAllCreditAndSumOfDisplayedPrices()
        {
            var skins = new List<Skin>();
            var displayedSumInCents = 0;

            for (var index = 0; index < 20; index++)
            {
                var skin = AddSkin(0.105f + index * 0.0371f);
                skins.Add(skin);
                displayedSumInCents += ParseDisplayedCents(NumberCounterFormatter.FormatMoney(_service.GetSellPrice(skin)));
            }

            var displayedTotal = _service.GetSellPrice(skins);

            var amount = _service.SellAll(skins);

            Assert.AreEqual(NumberCounterFormatter.FormatMoney(displayedTotal), NumberCounterFormatter.FormatMoney(amount));
            Assert.AreEqual(displayedSumInCents, MoneyMath.ToMinorUnits(amount));
            Assert.AreEqual(displayedTotal, _wallet.Currency, Tolerance);
        }

        [Test]
        public void CanSell_InInventoryAndAfterSale_ReturnsTrueThenFalse()
        {
            var skin = AddSkin(1f);

            var before = _service.CanSell(skin);
            _service.Sell(skin);
            var after = _service.CanSell(skin);

            Assert.IsTrue(before == true);
            Assert.IsTrue(after == false);
        }
    }
}

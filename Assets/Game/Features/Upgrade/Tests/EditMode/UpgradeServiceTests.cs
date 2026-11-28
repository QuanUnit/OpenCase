using System.Linq;
using Features.Inventory.Services;
using Features.Skins.Configs;
using Features.Skins.Data;
using Features.Upgrade.Configs;
using Features.Upgrade.Data;
using Features.Upgrade.Services;
using Features.Wallet.Services;
using NUnit.Framework;
using Shared.Identifiers;
using UnityEngine;

namespace Features.Upgrade.Tests.EditMode
{
    public class UpgradeServiceTests
    {
        private const float Tolerance = 0.0001f;

        private FakeSkinsDatabaseService _skinsDatabase;
        private FakeSkinFactory _skinFactory;
        private FakeChanceRoller _chanceRoller;
        private InventoryService _inventory;
        private SoftCurrencyWallet _wallet;

        [SetUp]
        public void SetUp()
        {
            _skinsDatabase = new FakeSkinsDatabaseService();
            _skinFactory = new FakeSkinFactory();
            _chanceRoller = new FakeChanceRoller();
            _inventory = new InventoryService();
            _wallet = new SoftCurrencyWallet();
        }

        private UpgradeService CreateService(UpgradeConfig config)
        {
            return new UpgradeService(config, _skinsDatabase, _skinFactory, _inventory, _chanceRoller, _wallet);
        }

        private UpgradeService CreateService()
        {
            return CreateService(new UpgradeConfigBuilder().Build());
        }

        private SkinConfig AddSkinConfig(float minPrice, float maxPrice, params float[] factoryPrices)
        {
            var config = ScriptableObject.CreateInstance<SkinConfig>();
            config.Id = EntityIds.CreateUnique<SkinId>("TestSkin");
            config.Price = new PriceRange(minPrice, maxPrice);

            _skinsDatabase.Add(config);
            _skinFactory.SetPrices(config, factoryPrices);

            return config;
        }

        private Skin AddSourceSkin(float price)
        {
            var skin = new Skin(EntityIds.CreateUnique<SkinId>("TestSkin"), EntityIds.CreateUnique<SkinInstanceId>(SkinInstanceId.EntityName), 0.1f, price, false, false);
            _inventory.Add(skin);

            return skin;
        }

        private Skin CreateSkin(float price)
        {
            return new Skin(EntityIds.CreateUnique<SkinId>("TestSkin"), EntityIds.CreateUnique<SkinInstanceId>(SkinInstanceId.EntityName), 0.1f, price, false, false);
        }

        [Test]
        public void CreateTargets_SeveralConfigsInWindow_ReturnsDistinctSortedTargetsAboveSource()
        {
            AddSkinConfig(15f, 25f, 19f);
            AddSkinConfig(15f, 25f, 21f);
            AddSkinConfig(15f, 25f, 20f);
            AddSkinConfig(30f, 40f, 35f);
            var source = AddSourceSkin(10f);
            var service = CreateService();

            var targets = service.CreateTargets(source, 2f);

            Assert.AreEqual(3, targets.Count);
            Assert.AreEqual(19f, targets[0].Price, Tolerance);
            Assert.AreEqual(20f, targets[1].Price, Tolerance);
            Assert.AreEqual(21f, targets[2].Price, Tolerance);
            Assert.AreEqual(3, targets.Select(target => target.ConfigId).Distinct().Count());
        }

        [Test]
        public void CreateTargets_MoreCandidatesThanTargetsCount_ReturnsTargetsCount()
        {
            for (var index = 0; index < 5; index++)
            {
                AddSkinConfig(15f, 25f, 20f);
            }

            var source = AddSourceSkin(10f);
            var service = CreateService(new UpgradeConfigBuilder().WithTargetsCount(2).Build());

            var targets = service.CreateTargets(source, 2f);

            Assert.AreEqual(2, targets.Count);
        }

        [Test]
        public void CreateTargets_NoCandidates_ReturnsEmpty()
        {
            AddSkinConfig(1f, 5f, 3f);
            var source = AddSourceSkin(10f);
            var service = CreateService();

            var targets = service.CreateTargets(source, 2f);

            Assert.AreEqual(0, targets.Count);
        }

        [Test]
        public void CreateTargets_ZeroPriceSource_ReturnsEmpty()
        {
            AddSkinConfig(0f, 1f, 0f);
            var source = AddSourceSkin(0f);
            var service = CreateService();

            var targets = service.CreateTargets(source, 2f);

            Assert.AreEqual(0, targets.Count);
        }

        [Test]
        public void CreateTargets_TargetNotMoreExpensive_IsExcluded()
        {
            AddSkinConfig(9f, 11f, 10f);
            var source = AddSourceSkin(10f);
            var service = CreateService(new UpgradeConfigBuilder().WithPriceTolerance(0.1f).Build());

            var targets = service.CreateTargets(source, 1.05f);

            Assert.AreEqual(0, targets.Count);
        }

        [Test]
        public void CreateTargets_ManyCandidates_TriesNoMoreThanMaxCandidates()
        {
            for (var index = 0; index < 10; index++)
            {
                AddSkinConfig(15f, 25f, 30f);
            }

            var source = AddSourceSkin(10f);
            var service = CreateService(new UpgradeConfigBuilder().WithMaxCandidates(3).WithTargetAttempts(2).Build());

            service.CreateTargets(source, 2f);

            Assert.AreEqual(6, _skinFactory.CreatedCount);
        }

        [Test]
        public void GetChance_WithExtra_AddsExtraToSourcePrice()
        {
            var source = AddSourceSkin(10f);
            var target = CreateSkin(40f);
            var service = CreateService(new UpgradeConfigBuilder().Build());

            var chance = service.GetChance(source, target, 10f);

            Assert.AreEqual(0.5f, chance, Tolerance);
        }

        [TestCase(10f, 15f, 0.6666667f)]
        [TestCase(10f, 20f, 0.5f)]
        [TestCase(10f, 30f, 0.3333333f)]
        [TestCase(10f, 50f, 0.2f)]
        [TestCase(10f, 100f, 0.1f)]
        public void GetChance_FairMultipliers_EqualsPriceRatio(float sourcePrice, float targetPrice, float expected)
        {
            var source = AddSourceSkin(sourcePrice);
            var target = CreateSkin(targetPrice);
            var service = CreateService();

            var chance = service.GetChance(source, target, 0f);

            Assert.AreEqual(expected, chance, Tolerance);
        }

        [Test]
        public void GetChance_AboveMaxChance_ReturnsMaxChance()
        {
            var source = AddSourceSkin(10f);
            var target = CreateSkin(12f);
            var service = CreateService(new UpgradeConfigBuilder().WithMaxChance(0.8f).Build());

            var chance = service.GetChance(source, target, 5f);

            Assert.AreEqual(0.8f, chance, Tolerance);
        }

        [Test]
        public void GetMaxExtra_EnoughMoney_ReturnsAmountToReachMaxChance()
        {
            var source = AddSourceSkin(10f);
            var target = CreateSkin(40f);
            var service = CreateService(new UpgradeConfigBuilder().WithMaxChance(0.8f).Build());
            _wallet.Add(1000f);

            var maxExtra = service.GetMaxExtra(source, target);

            Assert.AreEqual(22f, maxExtra, Tolerance);
        }

        [Test]
        public void GetMaxExtra_LimitedByWallet_ReturnsBalance()
        {
            var source = AddSourceSkin(10f);
            var target = CreateSkin(40f);
            var service = CreateService();
            _wallet.Add(5f);

            var maxExtra = service.GetMaxExtra(source, target);

            Assert.AreEqual(5f, maxExtra, Tolerance);
        }

        [Test]
        public void GetMaxExtra_EmptyWallet_ReturnsZero()
        {
            var source = AddSourceSkin(10f);
            var target = CreateSkin(40f);
            var service = CreateService();

            var maxExtra = service.GetMaxExtra(source, target);

            Assert.AreEqual(0f, maxExtra, Tolerance);
        }

        [Test]
        public void GetMaxExtra_ChanceAlreadyCapped_ReturnsZero()
        {
            var source = AddSourceSkin(10f);
            var target = CreateSkin(11f);
            var service = CreateService();
            _wallet.Add(1000f);

            var maxExtra = service.GetMaxExtra(source, target);

            Assert.AreEqual(0f, maxExtra, Tolerance);
        }

        [Test]
        public void GetMaxExtra_RoundsDownToCents()
        {
            var source = AddSourceSkin(10f);
            var target = CreateSkin(40.01f);
            var service = CreateService(new UpgradeConfigBuilder().WithMaxChance(0.8f).Build());
            _wallet.Add(1000f);

            var maxExtra = service.GetMaxExtra(source, target);
            var uncappedChance = (10f + maxExtra) / 40.01f;

            Assert.AreEqual(22f, maxExtra, Tolerance);
            Assert.IsTrue(uncappedChance <= 0.8f + 0.000001f);
        }

        [Test]
        public void GetMaxExtra_FractionalWalletBalance_ReturnsBalance()
        {
            var source = AddSourceSkin(10f);
            var target = CreateSkin(40f);
            var service = CreateService();
            _wallet.Add(0.29f);

            var maxExtra = service.GetMaxExtra(source, target);

            Assert.AreEqual(0.29f, maxExtra);
        }

        [Test]
        public void Upgrade_MaxOfferWithAccumulatedFloatBalance_IsNotRejectedAndSpendsExtra()
        {
            var source = AddSourceSkin(10f);
            var target = CreateSkin(400f);
            var service = CreateService();
            _wallet.Add(5.299999f);
            var offer = service.CreateOffer(source, target, service.GetMaxExtra(source, target));
            var balanceBefore = _wallet.Currency;
            _chanceRoller.Result = true;

            var result = service.Upgrade(offer);

            Assert.AreNotEqual(UpgradeResult.Rejected, result);
            Assert.AreEqual(balanceBefore - offer.Extra, _wallet.Currency, Tolerance);
        }

        [Test]
        public void CreateOffer_ExtraAboveMax_ClampsToMax()
        {
            var source = AddSourceSkin(10f);
            var target = CreateSkin(40f);
            var service = CreateService();
            _wallet.Add(5f);

            var offer = service.CreateOffer(source, target, 100f);

            Assert.AreEqual(5f, offer.Extra, Tolerance);
            Assert.AreEqual(service.GetChance(source, target, 5f), offer.Chance, Tolerance);
        }

        [Test]
        public void Upgrade_WithExtra_SpendsExactExtra()
        {
            var source = AddSourceSkin(10f);
            var target = CreateSkin(40f);
            var service = CreateService();
            _wallet.Add(100f);
            var offer = service.CreateOffer(source, target, 12.34f);
            _chanceRoller.Result = true;

            var result = service.Upgrade(offer);

            Assert.AreEqual(UpgradeResult.Success, result);
            Assert.AreEqual(87.66f, _wallet.Currency, Tolerance);
            Assert.IsTrue(_inventory.TryGet(target.Id, out _) == true);
        }

        [Test]
        public void Upgrade_Lost_SpendsExtraAndRemovesSource()
        {
            var source = AddSourceSkin(10f);
            var target = CreateSkin(40f);
            var service = CreateService();
            _wallet.Add(100f);
            var offer = service.CreateOffer(source, target, 10f);
            _chanceRoller.Result = false;

            var result = service.Upgrade(offer);

            Assert.AreEqual(UpgradeResult.Lost, result);
            Assert.AreEqual(90f, _wallet.Currency, Tolerance);
            Assert.IsTrue(_inventory.TryGet(source.Id, out _) == false);
            Assert.IsTrue(_inventory.TryGet(target.Id, out _) == false);
        }

        [Test]
        public void Upgrade_ZeroExtra_DoesNotTouchWallet()
        {
            var source = AddSourceSkin(10f);
            var target = CreateSkin(40f);
            var service = CreateService();
            var offer = service.CreateOffer(source, target, 0f);
            _chanceRoller.Result = true;

            var result = service.Upgrade(offer);

            Assert.AreEqual(UpgradeResult.Success, result);
            Assert.AreEqual(0f, _wallet.Currency, Tolerance);
        }

        [Test]
        public void Upgrade_NotEnoughMoney_ReturnsRejectedWithoutChanges()
        {
            var source = AddSourceSkin(10f);
            var target = CreateSkin(40f);
            var service = CreateService();
            _wallet.Add(10f);
            var offer = service.CreateOffer(source, target, 10f);
            _wallet.Spend(5f);

            var result = service.Upgrade(offer);

            Assert.AreEqual(UpgradeResult.Rejected, result);
            Assert.IsTrue(_inventory.TryGet(source.Id, out _) == true);
            Assert.AreEqual(5f, _wallet.Currency, Tolerance);
            Assert.AreEqual(0, _chanceRoller.RollsCount);
        }

        [Test]
        public void Upgrade_SourceNotInInventory_ReturnsRejected()
        {
            var source = AddSourceSkin(10f);
            var target = CreateSkin(40f);
            var service = CreateService();
            var offer = service.CreateOffer(source, target, 0f);
            _inventory.Remove(source);
            _chanceRoller.Result = true;

            var result = service.Upgrade(offer);

            Assert.AreEqual(UpgradeResult.Rejected, result);
            Assert.AreEqual(0, _chanceRoller.RollsCount);
            Assert.IsTrue(_inventory.TryGet(target.Id, out _) == false);
        }

        [Test]
        public void Upgrade_ValidOffer_PassesOfferChanceToRoller()
        {
            var source = AddSourceSkin(10f);
            var target = CreateSkin(20f);
            var service = CreateService();
            var offer = service.CreateOffer(source, target, 0f);

            service.Upgrade(offer);

            Assert.AreEqual(offer.Chance, _chanceRoller.LastChance, Tolerance);
        }
    }
}

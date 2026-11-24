using Features.Inventory.Services;
using Features.Skins.Configs;
using Features.Skins.Data;
using Features.Upgrade.Configs;
using Features.Upgrade.Services;
using NUnit.Framework;
using UnityEngine;
using Shared.Identifiers;

namespace Features.Upgrade.Tests.EditMode
{
    public class UpgradeServiceTests
    {
        private const float Tolerance = 0.0001f;

        private FakeSkinsDatabaseService _skinsDatabase;
        private FakeSkinFactory _skinFactory;
        private FakeChanceRoller _chanceRoller;
        private InventoryService _inventory;

        [SetUp]
        public void SetUp()
        {
            _skinsDatabase = new FakeSkinsDatabaseService();
            _skinFactory = new FakeSkinFactory();
            _chanceRoller = new FakeChanceRoller();
            _inventory = new InventoryService();
        }

        private UpgradeService CreateService(UpgradeConfig config)
        {
            return new UpgradeService(config, _skinsDatabase, _skinFactory, _inventory, _chanceRoller);
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

        [Test]
        public void TryCreateOffer_CandidateInWindow_ReturnsTargetWithPriceInWindow()
        {
            AddSkinConfig(15f, 25f, 21f);
            var source = AddSourceSkin(10f);
            var service = CreateService();

            var created = service.TryCreateOffer(source, 2f, out var offer);

            Assert.IsTrue(created == true);
            Assert.AreEqual(21f, offer.Target.Price, Tolerance);
            Assert.AreSame(source, offer.Source);
            Assert.AreEqual(2f, offer.Multiplier, Tolerance);
        }

        [Test]
        public void TryCreateOffer_SeveralRollsInWindow_PicksClosestToTargetPrice()
        {
            AddSkinConfig(15f, 25f, 18.5f, 20.4f, 21.9f);
            var source = AddSourceSkin(10f);
            var service = CreateService(new UpgradeConfigBuilder().WithTargetAttempts(3).Build());

            service.TryCreateOffer(source, 2f, out var offer);

            Assert.AreEqual(20.4f, offer.Target.Price, Tolerance);
        }

        [Test]
        public void TryCreateOffer_ValidTarget_ChanceIsPriceRatioTimesReturnToPlayer()
        {
            AddSkinConfig(15f, 25f, 20f);
            var source = AddSourceSkin(10f);
            var service = CreateService(new UpgradeConfigBuilder().WithReturnToPlayer(0.9f).Build());

            service.TryCreateOffer(source, 2f, out var offer);

            Assert.AreEqual(0.45f, offer.Chance, Tolerance);
        }

        [Test]
        public void TryCreateOffer_NoCandidateInWindow_ReturnsFalse()
        {
            AddSkinConfig(1f, 5f, 3f);
            AddSkinConfig(50f, 100f, 70f);
            var source = AddSourceSkin(10f);
            var service = CreateService();

            var created = service.TryCreateOffer(source, 2f, out var offer);

            Assert.IsTrue(created == false);
            Assert.IsNull(offer);
        }

        [Test]
        public void TryCreateOffer_RollsOutsideWindow_ReturnsFalse()
        {
            AddSkinConfig(10f, 30f, 12f, 29f);
            var source = AddSourceSkin(10f);
            var service = CreateService(new UpgradeConfigBuilder().WithTargetAttempts(2).Build());

            var created = service.TryCreateOffer(source, 2f, out _);

            Assert.IsTrue(created == false);
        }

        [Test]
        public void TryCreateOffer_ZeroPriceSource_ReturnsFalse()
        {
            AddSkinConfig(0f, 1f, 0f);
            var source = AddSourceSkin(0f);
            var service = CreateService();

            var created = service.TryCreateOffer(source, 2f, out _);

            Assert.IsTrue(created == false);
        }

        [Test]
        public void TryCreateOffer_TargetNotMoreExpensive_IsRejected()
        {
            AddSkinConfig(9f, 11f, 10f);
            var source = AddSourceSkin(10f);
            var service = CreateService(new UpgradeConfigBuilder().WithPriceTolerance(0.1f).Build());

            var created = service.TryCreateOffer(source, 1.05f, out _);

            Assert.IsTrue(created == false);
        }

        [Test]
        public void TryCreateOffer_ManyCandidates_TriesNoMoreThanMaxCandidates()
        {
            for (var index = 0; index < 10; index++)
            {
                AddSkinConfig(15f, 25f, 30f);
            }

            var source = AddSourceSkin(10f);
            var service = CreateService(new UpgradeConfigBuilder().WithMaxCandidates(3).WithTargetAttempts(2).Build());

            service.TryCreateOffer(source, 2f, out _);

            Assert.AreEqual(6, _skinFactory.CreatedCount);
        }

        [Test]
        public void Upgrade_RollSuccess_RemovesSourceAndAddsTarget()
        {
            AddSkinConfig(15f, 25f, 20f);
            var source = AddSourceSkin(10f);
            var service = CreateService();
            service.TryCreateOffer(source, 2f, out var offer);
            _chanceRoller.Result = true;

            var isSuccess = service.Upgrade(offer);

            Assert.IsTrue(isSuccess == true);
            Assert.IsTrue(_inventory.TryGet(source.Id, out _) == false);
            Assert.IsTrue(_inventory.TryGet(offer.Target.Id, out _) == true);
        }

        [Test]
        public void Upgrade_RollFail_RemovesSourceOnly()
        {
            AddSkinConfig(15f, 25f, 20f);
            var source = AddSourceSkin(10f);
            var service = CreateService();
            service.TryCreateOffer(source, 2f, out var offer);
            _chanceRoller.Result = false;

            var isSuccess = service.Upgrade(offer);

            Assert.IsTrue(isSuccess == false);
            Assert.IsTrue(_inventory.TryGet(source.Id, out _) == false);
            Assert.IsTrue(_inventory.TryGet(offer.Target.Id, out _) == false);
        }

        [Test]
        public void Upgrade_SourceNotInInventory_ReturnsFalseAndDoesNotAddTarget()
        {
            AddSkinConfig(15f, 25f, 20f);
            var source = AddSourceSkin(10f);
            var service = CreateService();
            service.TryCreateOffer(source, 2f, out var offer);
            _inventory.Remove(source);
            _chanceRoller.Result = true;

            var isSuccess = service.Upgrade(offer);

            Assert.IsTrue(isSuccess == false);
            Assert.AreEqual(0, _chanceRoller.RollsCount);
            Assert.IsTrue(_inventory.TryGet(offer.Target.Id, out _) == false);
        }

        [Test]
        public void Upgrade_ValidOffer_PassesOfferChanceToRoller()
        {
            AddSkinConfig(15f, 25f, 20f);
            var source = AddSourceSkin(10f);
            var service = CreateService();
            service.TryCreateOffer(source, 2f, out var offer);

            service.Upgrade(offer);

            Assert.AreEqual(offer.Chance, _chanceRoller.LastChance, Tolerance);
        }
    }
}

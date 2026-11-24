using System.Collections.Generic;
using Features.Contracts.Services;
using Features.Inventory.Services;
using Features.Skins.Configs;
using Features.Skins.Data;
using NUnit.Framework;
using QCore.Tools.Identifiers;
using UnityEngine;

namespace Features.Contracts.Tests.EditMode
{
    public class ContractServiceTests
    {
        private const float Tolerance = 0.0001f;
        private const string CollectionA = "Collection A";
        private const string CollectionB = "Collection B";

        private FakeSkinsDatabaseService _skinsDatabase;
        private FakeSkinFactory _skinFactory;
        private FakeContractRandom _random;
        private InventoryService _inventory;
        private SkinConfig _milSpecA;
        private SkinConfig _milSpecB;
        private SkinConfig _restrictedA1;
        private SkinConfig _restrictedA2;
        private SkinConfig _restrictedB;

        [SetUp]
        public void SetUp()
        {
            _skinsDatabase = new FakeSkinsDatabaseService();
            _skinFactory = new FakeSkinFactory();
            _random = new FakeContractRandom();
            _inventory = new InventoryService();

            _milSpecA = AddConfig(Rarity.MilSpec, CollectionA);
            _milSpecB = AddConfig(Rarity.MilSpec, CollectionB);
            _restrictedA1 = AddConfig(Rarity.Restricted, CollectionA);
            _restrictedA2 = AddConfig(Rarity.Restricted, CollectionA);
            _restrictedB = AddConfig(Rarity.Restricted, CollectionB);
        }

        private ContractService CreateService()
        {
            return new ContractService(new ContractConfigBuilder().Build(), _skinsDatabase, _skinFactory, _inventory, _random);
        }

        private SkinConfig AddConfig(Rarity rarity, string collection)
        {
            var config = ScriptableObject.CreateInstance<SkinConfig>();
            config.CreateId();
            config.Rarity = rarity;
            config.Collection = collection;

            _skinsDatabase.Add(config);

            return config;
        }

        private Skin CreateSkin(SkinConfig config, float floatValue = 0.1f, bool isStatTrak = false, bool isSouvenir = false)
        {
            return new Skin(config.Id, QGuid.CreateNew(), floatValue, 1f, isStatTrak, isSouvenir);
        }

        private List<Skin> CreateInputs(SkinConfig configA, int countA, SkinConfig configB, int countB, float floatA = 0.1f, float floatB = 0.1f, bool isStatTrak = false)
        {
            var inputs = new List<Skin>();

            for (var index = 0; index < countA; index++)
            {
                inputs.Add(CreateSkin(configA, floatA, isStatTrak));
            }

            for (var index = 0; index < countB; index++)
            {
                inputs.Add(CreateSkin(configB, floatB, isStatTrak));
            }

            foreach (var input in inputs)
            {
                _inventory.Add(input);
            }

            return inputs;
        }

        [Test]
        public void IsEligible_ValidSkin_ReturnsTrue()
        {
            var service = CreateService();

            Assert.IsTrue(service.IsEligible(CreateSkin(_milSpecA)) == true);
        }

        [Test]
        public void IsEligible_Souvenir_ReturnsFalse()
        {
            var service = CreateService();

            Assert.IsTrue(service.IsEligible(CreateSkin(_milSpecA, isSouvenir: true)) == false);
        }

        [Test]
        public void IsEligible_CovertRarity_ReturnsFalse()
        {
            var covert = AddConfig(Rarity.Covert, CollectionA);
            var service = CreateService();

            Assert.IsTrue(service.IsEligible(CreateSkin(covert)) == false);
        }

        [Test]
        public void IsEligible_NoCollection_ReturnsFalse()
        {
            var config = AddConfig(Rarity.MilSpec, null);
            var service = CreateService();

            Assert.IsTrue(service.IsEligible(CreateSkin(config)) == false);
        }

        [Test]
        public void IsEligible_CollectionWithoutNextRarity_ReturnsFalse()
        {
            var lonely = AddConfig(Rarity.MilSpec, "Lonely Collection");
            var service = CreateService();

            Assert.IsTrue(service.IsEligible(CreateSkin(lonely)) == false);
        }

        [Test]
        public void CanAdd_DifferentRarity_ReturnsFalse()
        {
            var restrictedWithNext = AddConfig(Rarity.Restricted, CollectionA);
            AddConfig(Rarity.Classified, CollectionA);
            var service = CreateService();
            var inputs = new List<Skin>() { CreateSkin(_milSpecA) };

            Assert.IsTrue(service.CanAdd(inputs, CreateSkin(restrictedWithNext)) == false);
        }

        [Test]
        public void CanAdd_DifferentStatTrak_ReturnsFalse()
        {
            var service = CreateService();
            var inputs = new List<Skin>() { CreateSkin(_milSpecA) };

            Assert.IsTrue(service.CanAdd(inputs, CreateSkin(_milSpecA, isStatTrak: true)) == false);
        }

        [Test]
        public void CanAdd_Full_ReturnsFalse()
        {
            var service = CreateService();
            var inputs = CreateInputs(_milSpecA, 10, _milSpecB, 0);

            Assert.IsTrue(service.CanAdd(inputs, CreateSkin(_milSpecA)) == false);
        }

        [Test]
        public void CanAdd_SameSkinTwice_ReturnsFalse()
        {
            var service = CreateService();
            var skin = CreateSkin(_milSpecA);
            var inputs = new List<Skin>() { skin };

            Assert.IsTrue(service.CanAdd(inputs, skin) == false);
        }

        [Test]
        public void CanAdd_MatchingSkin_ReturnsTrue()
        {
            var service = CreateService();
            var inputs = new List<Skin>() { CreateSkin(_milSpecA) };

            Assert.IsTrue(service.CanAdd(inputs, CreateSkin(_milSpecB)) == true);
        }

        [Test]
        public void GetOutcomes_FiveFromAFiveFromB_SplitsByCollectionAndCandidates()
        {
            var service = CreateService();
            var inputs = CreateInputs(_milSpecA, 5, _milSpecB, 5);

            var outcomes = service.GetOutcomes(inputs);

            Assert.AreEqual(3, outcomes.Count);
            Assert.AreSame(_restrictedB, outcomes[0].Config);
            Assert.AreEqual(0.5f, outcomes[0].Chance, Tolerance);
            Assert.AreEqual(0.25f, outcomes[1].Chance, Tolerance);
            Assert.AreEqual(0.25f, outcomes[2].Chance, Tolerance);

            var sum = outcomes[0].Chance + outcomes[1].Chance + outcomes[2].Chance;

            Assert.AreEqual(1f, sum, Tolerance);
        }

        [Test]
        public void GetOutcomes_NineFromAOneFromB_WeightsByInputs()
        {
            var service = CreateService();
            var inputs = CreateInputs(_milSpecA, 9, _milSpecB, 1);

            var outcomes = service.GetOutcomes(inputs);

            Assert.AreEqual(3, outcomes.Count);
            Assert.AreEqual(0.45f, outcomes[0].Chance, Tolerance);
            Assert.AreEqual(0.45f, outcomes[1].Chance, Tolerance);
            Assert.AreSame(_restrictedB, outcomes[2].Config);
            Assert.AreEqual(0.1f, outcomes[2].Chance, Tolerance);
        }

        [Test]
        public void GetOutcomes_EmptyInputs_ReturnsEmpty()
        {
            var service = CreateService();

            Assert.AreEqual(0, service.GetOutcomes(new List<Skin>()).Count);
        }

        [Test]
        public void TryExecute_Valid_RemovesInputsAndAddsResult()
        {
            var service = CreateService();
            var inputs = CreateInputs(_milSpecA, 10, _milSpecB, 0);

            var executed = service.TryExecute(inputs, out var result);

            Assert.IsTrue(executed == true);
            Assert.AreEqual(1, new List<Skin>(_inventory.AllSkins).Count);
            Assert.IsTrue(_inventory.TryGet(result.Id, out _) == true);
            Assert.IsTrue(_inventory.TryGet(inputs[0].Id, out _) == false);
        }

        [Test]
        public void TryExecute_RemovedHandlerModifiesInputs_DoesNotThrow()
        {
            var service = CreateService();
            var inputs = CreateInputs(_milSpecA, 10, _milSpecB, 0);

            _inventory.Removed += skin => inputs.Remove(skin);

            var executed = service.TryExecute(inputs, out _);

            Assert.IsTrue(executed == true);
            Assert.AreEqual(1, new List<Skin>(_inventory.AllSkins).Count);
        }

        [Test]
        public void TryExecute_FloatIsAverageOfInputs()
        {
            var service = CreateService();
            var inputs = CreateInputs(_milSpecA, 5, _milSpecB, 5, 0.1f, 0.3f);

            service.TryExecute(inputs, out _);

            Assert.AreEqual(0.2f, _skinFactory.LastFloatValue, Tolerance);
        }

        [Test]
        public void TryExecute_StatTrakInputs_PassesStatTrak()
        {
            var service = CreateService();
            var inputs = CreateInputs(_milSpecA, 10, _milSpecB, 0, isStatTrak: true);

            service.TryExecute(inputs, out var result);

            Assert.IsTrue(_skinFactory.LastIsStatTrak == true);
            Assert.IsTrue(result.IsStatTrak == true);
        }

        [Test]
        public void TryExecute_RandomPicksByCumulativeChance()
        {
            var service = CreateService();
            var inputs = CreateInputs(_milSpecA, 5, _milSpecB, 5);
            _random.Value = 0.3f;

            service.TryExecute(inputs, out _);

            Assert.AreSame(_restrictedB, _skinFactory.LastConfig);
        }

        [Test]
        public void TryExecute_RandomAboveLargestChance_PicksFirstCandidateOfA()
        {
            var service = CreateService();
            var inputs = CreateInputs(_milSpecA, 5, _milSpecB, 5);
            _random.Value = 0.6f;

            service.TryExecute(inputs, out _);

            Assert.AreSame(_restrictedA1, _skinFactory.LastConfig);
        }

        [Test]
        public void TryExecute_DuplicateInput_ChangesNothing()
        {
            var service = CreateService();
            var inputs = CreateInputs(_milSpecA, 9, _milSpecB, 0);
            inputs.Add(inputs[0]);

            var executed = service.TryExecute(inputs, out var result);

            Assert.IsTrue(executed == false);
            Assert.IsNull(result);
            Assert.AreEqual(9, new List<Skin>(_inventory.AllSkins).Count);
        }

        [Test]
        public void TryExecute_MixedRarity_ChangesNothing()
        {
            AddConfig(Rarity.Classified, CollectionA);
            var service = CreateService();
            var inputs = CreateInputs(_milSpecA, 9, _restrictedA1, 1);

            var executed = service.TryExecute(inputs, out _);

            Assert.IsTrue(executed == false);
            Assert.AreEqual(10, new List<Skin>(_inventory.AllSkins).Count);
        }

        [Test]
        public void TryExecute_IneligibleInput_ChangesNothing()
        {
            var service = CreateService();
            var inputs = CreateInputs(_restrictedA1, 5, _restrictedB, 5);

            var executed = service.TryExecute(inputs, out var result);

            Assert.IsTrue(executed == false);
            Assert.IsNull(result);
            Assert.AreEqual(10, new List<Skin>(_inventory.AllSkins).Count);
        }

        [Test]
        public void TryExecute_RollJustBelowOne_PicksLastOutcome()
        {
            var service = CreateService();
            var inputs = CreateInputs(_milSpecA, 5, _milSpecB, 5);
            _random.Value = 0.99999f;

            service.TryExecute(inputs, out _);

            Assert.AreSame(_restrictedA2, _skinFactory.LastConfig);
        }

        [Test]
        public void TryExecute_InputMissingFromInventory_ChangesNothing()
        {
            var service = CreateService();
            var inputs = CreateInputs(_milSpecA, 10, _milSpecB, 0);
            _inventory.Remove(inputs[3]);

            var executed = service.TryExecute(inputs, out var result);

            Assert.IsTrue(executed == false);
            Assert.IsNull(result);
            Assert.AreEqual(9, new List<Skin>(_inventory.AllSkins).Count);
        }

        [Test]
        public void TryExecute_NotFull_ReturnsFalse()
        {
            var service = CreateService();
            var inputs = CreateInputs(_milSpecA, 9, _milSpecB, 0);

            var executed = service.TryExecute(inputs, out _);

            Assert.IsTrue(executed == false);
            Assert.AreEqual(9, new List<Skin>(_inventory.AllSkins).Count);
        }
    }
}

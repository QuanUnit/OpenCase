using System.Collections.Generic;
using Features.RemoteConfigs.Editor.Cases;
using Features.Skins.Data;
using NUnit.Framework;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class RarityOddsWeightingTests
    {
        private const float Tolerance = 1e-3f;

        private RarityOddsWeighting _weighting;

        [SetUp]
        public void SetUp()
        {
            var odds = new Dictionary<Rarity, float>
            {
                { Rarity.MilSpec, 80f },
                { Rarity.Restricted, 16f },
                { Rarity.Covert, 4f },
            };

            _weighting = new RarityOddsWeighting(odds, new Dictionary<Rarity, float>(), 100f);
        }

        [Test]
        public void Calculate_AbsentRarity_IsRenormalisedAway()
        {
            var result = _weighting.Calculate(new List<Rarity> { Rarity.MilSpec, Rarity.Covert, Rarity.MilSpec });

            Assert.That(result.Weights[0], Is.EqualTo(80f / 84f * 100f / 2f).Within(Tolerance));
            Assert.That(result.Weights[1], Is.EqualTo(4f / 84f * 100f).Within(Tolerance));
            Assert.That(result.Weights[2], Is.EqualTo(result.Weights[0]));
            Assert.That(result.RaritiesWithoutOdds, Is.Empty);
        }

        [Test]
        public void Calculate_AllRarities_TotalIsConfiguredWeight()
        {
            var result = _weighting.Calculate(new List<Rarity> { Rarity.MilSpec, Rarity.Restricted, Rarity.Restricted, Rarity.Covert });

            var total = 0f;

            foreach (var weight in result.Weights)
            {
                total += weight;
            }

            Assert.That(total, Is.EqualTo(100f).Within(Tolerance));
            Assert.That(result.Weights[1] + result.Weights[2], Is.EqualTo(16f).Within(Tolerance));
        }

        [Test]
        public void Calculate_RarityWithoutOdds_GetsZeroAndIsReported()
        {
            var result = _weighting.Calculate(new List<Rarity> { Rarity.MilSpec, Rarity.ConsumerGrade });

            Assert.That(result.Weights[0], Is.EqualTo(100f).Within(Tolerance));
            Assert.That(result.Weights[1], Is.EqualTo(0f));
            Assert.That(result.RaritiesWithoutOdds, Is.EqualTo(new[] { Rarity.ConsumerGrade }));
        }

        [Test]
        public void Calculate_FixedShareRarity_KeepsItsShareAndOthersRenormalise()
        {
            var odds = new Dictionary<Rarity, float> { { Rarity.Classified, 3.2f }, { Rarity.Covert, 0.64f }, { Rarity.RareSpecial, 0.26f } };
            var fixedShares = new Dictionary<Rarity, float> { { Rarity.RareSpecial, 0.0026f } };
            var weighting = new RarityOddsWeighting(odds, fixedShares, 100f);

            var result = weighting.Calculate(new List<Rarity> { Rarity.Classified, Rarity.Covert, Rarity.RareSpecial });

            Assert.That(result.Weights[2], Is.EqualTo(0.26f).Within(Tolerance));
            Assert.That(result.Weights[0], Is.EqualTo(3.2f / 3.84f * 99.74f).Within(Tolerance));
            Assert.That(result.Weights[1], Is.EqualTo(0.64f / 3.84f * 99.74f).Within(Tolerance));
        }

        [Test]
        public void Calculate_OnlyFixedShareRarity_TakesWholeTotal()
        {
            var fixedShares = new Dictionary<Rarity, float> { { Rarity.RareSpecial, 0.0026f } };
            var weighting = new RarityOddsWeighting(new Dictionary<Rarity, float>(), fixedShares, 100f);

            var result = weighting.Calculate(new List<Rarity> { Rarity.RareSpecial, Rarity.RareSpecial });

            Assert.That(result.Weights[0], Is.EqualTo(50f).Within(Tolerance));
            Assert.That(result.Weights[1], Is.EqualTo(50f).Within(Tolerance));
        }

        [Test]
        public void Calculate_EmptyCase_ReturnsNoWeights()
        {
            var result = _weighting.Calculate(new List<Rarity>());

            Assert.That(result.Weights, Is.Empty);
        }
    }
}

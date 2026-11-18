using System.Collections.Generic;
using Features.RemoteConfigs.Editor.Cases;
using Features.Skins.Configs;
using Features.Skins.Data;
using NUnit.Framework;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class CaseStatsCalculatorTests
    {
        private const double Tolerance = 1e-6d;
        private const float RewardPerClick = 0.01f;

        private CaseStatsCalculator _calculator;
        private CaseStatsSettings _settings;
        private SkinValueSampler _sampler;
        private SkinValueModel _model;

        [SetUp]
        public void SetUp()
        {
            _calculator = new CaseStatsCalculator();
            _settings = new CaseStatsSettings(new List<float> { 2f, 5f }, Rarity.Covert, 1, RewardPerClick, 0.4f, 0.1f);
            _sampler = new SkinValueSampler(16);
            _model = new SkinValueModel(AnimationCurve.Linear(0f, 0f, 1f, 1f), AnimationCurve.Linear(0f, 0f, 1f, 1f), 0f, 0f, 2f, 0.7f);
        }

        [Test]
        public void Calculate_TwoFixedSkins_ComputesEconomy()
        {
            var entries = new List<CaseStatsEntry> { CreateEntry("Cheap", Rarity.MilSpec, 3f, 1f), CreateEntry("Rare", Rarity.Covert, 1f, 10f) };

            var stats = _calculator.Calculate(entries, 4d, _settings);

            Assert.That(stats.IsValid, Is.True);
            Assert.That(stats.ExpectedValue, Is.EqualTo(3.25d).Within(Tolerance));
            Assert.That(stats.ReturnToPlayer, Is.EqualTo(0.8125d).Within(Tolerance));
            Assert.That(stats.HouseEdge, Is.EqualTo(0.1875d).Within(Tolerance));
            Assert.That(stats.ProfitChance, Is.EqualTo(0.25d).Within(Tolerance));
            Assert.That(stats.MultipleChances[0].Chance, Is.EqualTo(0.25d).Within(Tolerance));
            Assert.That(stats.MultipleChances[1].Chance, Is.EqualTo(0d).Within(Tolerance));
            Assert.That(stats.Median, Is.EqualTo(1d).Within(1e-4d));
            Assert.That(stats.Min, Is.EqualTo(1d).Within(Tolerance));
            Assert.That(stats.Max, Is.EqualTo(10d).Within(Tolerance));
            Assert.That(stats.StandardDeviation, Is.EqualTo(System.Math.Sqrt(15.1875d)).Within(1e-5d));
        }

        [Test]
        public void Calculate_TwoFixedSkins_ComputesRaritiesAndContributors()
        {
            var entries = new List<CaseStatsEntry> { CreateEntry("Cheap", Rarity.MilSpec, 3f, 1f), CreateEntry("Rare", Rarity.Covert, 1f, 10f) };

            var stats = _calculator.Calculate(entries, 4d, _settings);

            Assert.That(stats.RarityChances[Rarity.MilSpec], Is.EqualTo(0.75d).Within(Tolerance));
            Assert.That(stats.RarityChances[Rarity.Covert], Is.EqualTo(0.25d).Within(Tolerance));
            Assert.That(stats.ExpectedOpensUntilRare, Is.EqualTo(4d).Within(Tolerance));
            Assert.That(stats.TopContributors.Count, Is.EqualTo(1));
            Assert.That(stats.TopContributors[0].SkinId, Is.EqualTo("Rare"));
            Assert.That(stats.ClicksPerOpen, Is.EqualTo(400));
        }

        [Test]
        public void Calculate_NoRareSkins_OpensUntilRareIsInfinite()
        {
            var entries = new List<CaseStatsEntry> { CreateEntry("Cheap", Rarity.MilSpec, 1f, 1f) };

            var stats = _calculator.Calculate(entries, 1d, _settings);

            Assert.That(double.IsPositiveInfinity(stats.ExpectedOpensUntilRare), Is.True);
        }

        [Test]
        public void Calculate_EmptyCase_IsInvalid()
        {
            var stats = _calculator.Calculate(new List<CaseStatsEntry>(), 1d, _settings);

            Assert.That(stats.IsValid, Is.False);
        }

        [Test]
        public void Calculate_ZeroWeights_IsInvalid()
        {
            var entries = new List<CaseStatsEntry> { CreateEntry("Cheap", Rarity.MilSpec, 0f, 1f) };

            var stats = _calculator.Calculate(entries, 1d, _settings);

            Assert.That(stats.IsValid, Is.False);
        }

        [Test]
        public void Calculate_PriceNotMultipleOfReward_RoundsClicksUp()
        {
            var entries = new List<CaseStatsEntry> { CreateEntry("Cheap", Rarity.MilSpec, 1f, 1f) };

            var stats = _calculator.Calculate(entries, 0.305d, _settings);

            Assert.That(stats.ClicksPerOpen, Is.EqualTo(31));
        }

        [Test]
        public void Calculate_RareSkinGivesMostOfValue_IsDominant()
        {
            var entries = new List<CaseStatsEntry> { CreateEntry("Cheap", Rarity.MilSpec, 3f, 1f), CreateEntry("Rare", Rarity.Covert, 1f, 10f) };

            var stats = _calculator.Calculate(entries, 4d, _settings);

            Assert.That(stats.DominantEntries.Count, Is.EqualTo(1));
            Assert.That(stats.DominantEntries[0].SkinId, Is.EqualTo("Rare"));
        }

        [Test]
        public void Calculate_LowRaritySkinAbovePrice_IsOverpaying()
        {
            var entries = new List<CaseStatsEntry>
            {
                CreateEntry("Cheap", Rarity.MilSpec, 10f, 0.5f),
                CreateEntry("Jackpot", Rarity.MilSpec, 10f, 5f),
                CreateEntry("Top", Rarity.Covert, 1f, 50f),
            };

            var stats = _calculator.Calculate(entries, 4d, _settings);

            Assert.That(stats.OverpayingEntries.Count, Is.EqualTo(1));
            Assert.That(stats.OverpayingEntries[0].SkinId, Is.EqualTo("Jackpot"));
        }

        [Test]
        public void Calculate_RareMiddleTierAbovePrice_IsNotOverpaying()
        {
            var entries = new List<CaseStatsEntry>
            {
                CreateEntry("Cheap", Rarity.MilSpec, 95f, 0.5f),
                CreateEntry("RareMiddle", Rarity.Classified, 3f, 20f),
                CreateEntry("Top", Rarity.Covert, 2f, 50f),
            };

            var stats = _calculator.Calculate(entries, 4d, _settings);

            Assert.That(stats.OverpayingEntries, Is.Empty);
        }

        [Test]
        public void Calculate_TopRarityAbovePrice_IsNotOverpaying()
        {
            var entries = new List<CaseStatsEntry> { CreateEntry("Cheap", Rarity.MilSpec, 1f, 0.5f), CreateEntry("Top", Rarity.Covert, 1f, 50f) };

            var stats = _calculator.Calculate(entries, 4d, _settings);

            Assert.That(stats.OverpayingEntries, Is.Empty);
        }

        [Test]
        public void Calculate_ContinuousRange_MedianIsMiddle()
        {
            var sampler = new SkinValueSampler(512);
            var entries = new List<CaseStatsEntry> { new CaseStatsEntry("Range", "Range", Rarity.MilSpec, 1f, sampler.Sample(_model, new PriceRange(0f, 10f), false, false)) };

            var stats = _calculator.Calculate(entries, 5d, _settings);

            Assert.That(stats.Median, Is.EqualTo(5d).Within(0.02d));
            Assert.That(stats.ProfitChance, Is.EqualTo(0.5d).Within(0.005d));
        }

        private CaseStatsEntry CreateEntry(string id, Rarity rarity, float weight, float value)
        {
            return new CaseStatsEntry(id, id, rarity, weight, _sampler.Sample(_model, new PriceRange(value, value), false, false));
        }
    }
}

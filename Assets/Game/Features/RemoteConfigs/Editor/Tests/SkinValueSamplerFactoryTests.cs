using Features.RemoteConfigs.Editor.Cases;
using Features.Skins.Configs;
using Features.Skins.Services;
using Features.Skins.Tests.EditMode;
using NUnit.Framework;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class SkinValueSamplerFactoryTests
    {
        private const int OpensCount = 200000;
        private const int Seed = 12345;
        private const double RelativeTolerance = 0.01d;

        private SkinConfig _skinConfig;
        private SkinsGlobalSettingsConfig _globalConfig;

        [SetUp]
        public void SetUp()
        {
            var floatCurve = new AnimationCurve(new Keyframe(0f, 0f, 0.18f, 0.18f), new Keyframe(0.87f, 0.66f, 1.65f, 1.65f), new Keyframe(1f, 1f, 3.76f, 3.76f));

            _globalConfig = new SkinsGlobalSettingsConfigBuilder()
                .WithFloatValueCurve(floatCurve)
                .WithStatTrakChance(0.3f)
                .WithPriceMultiplierByStatTrak(2f)
                .WithSouvenirChance(0.2f)
                .WithPriceMultiplierBySouvenir(0.7f)
                .Build();

            _skinConfig = ScriptableObject.CreateInstance<SkinConfig>();
            _skinConfig.Price = new PriceRange(1f, 10f);
            _skinConfig.AvailableStatTrak = true;
            _skinConfig.AvailableSouvenir = true;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_skinConfig);
            Object.DestroyImmediate(_globalConfig);
        }

        [Test]
        public void Sample_MatchesSkinFactoryMonteCarloMean()
        {
            var distribution = new SkinValueSampler(512).Sample(new SkinValueModel(_globalConfig), _skinConfig.Price, true, true);
            var factory = new SkinFactory(_globalConfig);
            var previousState = Random.state;
            var total = 0d;

            Random.InitState(Seed);

            for (var index = 0; index < OpensCount; index++)
            {
                total += factory.CreateRandom(_skinConfig).Price;
            }

            Random.state = previousState;

            Assert.That(total / OpensCount, Is.EqualTo(distribution.Mean).Within(distribution.Mean * RelativeTolerance));
        }
    }
}

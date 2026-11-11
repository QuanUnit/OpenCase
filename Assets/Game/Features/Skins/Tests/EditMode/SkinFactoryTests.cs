using Features.Skins.Configs;
using Features.Skins.Services;
using NUnit.Framework;
using UnityEngine;

namespace Features.Skins.Tests.EditMode
{
    public class SkinFactoryTests
    {
        private SkinConfig CreateSkinConfig(bool availableStatTrak, bool availableSouvenir)
        {
            var config = ScriptableObject.CreateInstance<SkinConfig>();
            config.Price = new PriceRange(10f, 20f);
            config.AvailableStatTrak = availableStatTrak;
            config.AvailableSouvenir = availableSouvenir;

            return config;
        }

        [Test]
        public void CreateRandom_NoModifiers_PriceInsideRange()
        {
            var globalConfig = new SkinsGlobalSettingsConfigBuilder().Build();
            var factory = new SkinFactory(globalConfig);
            var config = CreateSkinConfig(true, true);

            for (var index = 0; index < 100; index++)
            {
                var skin = factory.CreateRandom(config);

                Assert.GreaterOrEqual(skin.Price, 10f);
                Assert.LessOrEqual(skin.Price, 20f);
                Assert.IsTrue(skin.IsStatTrak == false);
                Assert.IsTrue(skin.IsSouvenir == false);
            }
        }

        [Test]
        public void CreateRandom_StatTrakChanceOne_AppliesMultiplier()
        {
            var globalConfig = new SkinsGlobalSettingsConfigBuilder()
                .WithStatTrakChance(1f)
                .WithPriceMultiplierByStatTrak(2f)
                .WithFloatValueCurve(AnimationCurve.Constant(0f, 1f, 0f))
                .WithPriceByFloatValueCurve(AnimationCurve.Constant(0f, 1f, 0f))
                .Build();
            var factory = new SkinFactory(globalConfig);
            var config = CreateSkinConfig(true, false);

            var skin = factory.CreateRandom(config);

            Assert.IsTrue(skin.IsStatTrak == true);
            Assert.AreEqual(20f, skin.Price, 0.0001f);
        }

        [Test]
        public void CreateRandom_StatTrakNotAvailable_IgnoresChance()
        {
            var globalConfig = new SkinsGlobalSettingsConfigBuilder()
                .WithStatTrakChance(1f)
                .WithPriceMultiplierByStatTrak(2f)
                .WithFloatValueCurve(AnimationCurve.Constant(0f, 1f, 0f))
                .WithPriceByFloatValueCurve(AnimationCurve.Constant(0f, 1f, 0f))
                .Build();
            var factory = new SkinFactory(globalConfig);
            var config = CreateSkinConfig(false, false);

            var skin = factory.CreateRandom(config);

            Assert.IsTrue(skin.IsStatTrak == false);
            Assert.AreEqual(10f, skin.Price, 0.0001f);
        }

        [Test]
        public void CreateRandom_SouvenirChanceOne_AppliesMultiplier()
        {
            var globalConfig = new SkinsGlobalSettingsConfigBuilder()
                .WithSouvenirChance(1f)
                .WithPriceMultiplierBySouvenir(3f)
                .WithFloatValueCurve(AnimationCurve.Constant(0f, 1f, 0f))
                .WithPriceByFloatValueCurve(AnimationCurve.Constant(0f, 1f, 0f))
                .Build();
            var factory = new SkinFactory(globalConfig);
            var config = CreateSkinConfig(false, true);

            var skin = factory.CreateRandom(config);

            Assert.IsTrue(skin.IsSouvenir == true);
            Assert.AreEqual(30f, skin.Price, 0.0001f);
        }

        [Test]
        public void CreateRandom_KeepsSourceIdAndCreatesNewId()
        {
            var globalConfig = new SkinsGlobalSettingsConfigBuilder().Build();
            var factory = new SkinFactory(globalConfig);
            var config = CreateSkinConfig(false, false);

            var first = factory.CreateRandom(config);
            var second = factory.CreateRandom(config);

            Assert.AreEqual(config.Id, first.SkinSourceId);
            Assert.AreEqual(config.Id, second.SkinSourceId);
            Assert.AreNotEqual(first.Id, second.Id);
        }
    }
}

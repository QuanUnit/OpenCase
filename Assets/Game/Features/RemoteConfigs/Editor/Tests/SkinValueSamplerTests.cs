using Features.RemoteConfigs.Editor.Cases;
using Features.Skins.Configs;
using NUnit.Framework;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class SkinValueSamplerTests
    {
        private const int SamplesCount = 512;
        private const double Tolerance = 1e-4d;

        [Test]
        public void Sample_LinearCurves_MeanIsMiddleOfRange()
        {
            var sampler = new SkinValueSampler(SamplesCount);

            var distribution = sampler.Sample(CreateModel(0f, 0f), new PriceRange(2f, 6f), false, false);

            Assert.That(distribution.Mean, Is.EqualTo(4d).Within(Tolerance));
            Assert.That(distribution.Min, Is.EqualTo(2d).Within(Tolerance));
            Assert.That(distribution.Max, Is.EqualTo(6d).Within(Tolerance));
        }

        [Test]
        public void Sample_SquaredFloatCurve_MeanFollowsCurve()
        {
            var floatCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 2f, 2f));
            var model = new SkinValueModel(floatCurve, AnimationCurve.Linear(0f, 0f, 1f, 1f), 0f, 0f, 2f, 0.7f);
            var sampler = new SkinValueSampler(SamplesCount);

            var distribution = sampler.Sample(model, new PriceRange(0f, 3f), false, false);

            Assert.That(distribution.Mean, Is.EqualTo(1d).Within(1e-3d));
        }

        [Test]
        public void Sample_StatTrakChance_ScalesMeanWhenAvailable()
        {
            var sampler = new SkinValueSampler(SamplesCount);
            var model = CreateModel(0.5f, 0f);

            var available = sampler.Sample(model, new PriceRange(1f, 1f), true, false);
            var unavailable = sampler.Sample(model, new PriceRange(1f, 1f), false, false);

            Assert.That(available.Mean, Is.EqualTo(1.5d).Within(Tolerance));
            Assert.That(available.Max, Is.EqualTo(2d).Within(Tolerance));
            Assert.That(unavailable.Mean, Is.EqualTo(1d).Within(Tolerance));
        }

        [Test]
        public void Sample_StatTrakAndSouvenir_MixesFourVariants()
        {
            var sampler = new SkinValueSampler(SamplesCount);

            var distribution = sampler.Sample(CreateModel(0.5f, 0.5f), new PriceRange(1f, 1f), true, true);

            Assert.That(distribution.Mean, Is.EqualTo(0.25d * (1d + 2d + 0.7d + 1.4d)).Within(Tolerance));
            Assert.That(distribution.Min, Is.EqualTo(0.7d).Within(Tolerance));
            Assert.That(distribution.Max, Is.EqualTo(2d).Within(Tolerance));
            Assert.That(distribution.GetProbabilityAtLeast(1.3d), Is.EqualTo(0.5d).Within(Tolerance));
        }

        [Test]
        public void GetProbabilityAtLeast_LinearRange_ReturnsTailShare()
        {
            var sampler = new SkinValueSampler(SamplesCount);

            var distribution = sampler.Sample(CreateModel(0f, 0f), new PriceRange(0f, 10f), false, false);

            Assert.That(distribution.GetProbabilityAtLeast(7.5d), Is.EqualTo(0.25d).Within(0.01d));
            Assert.That(distribution.GetProbabilityAtLeast(-1d), Is.EqualTo(1d).Within(Tolerance));
            Assert.That(distribution.GetProbabilityAtLeast(11d), Is.EqualTo(0d));
        }

        private SkinValueModel CreateModel(float statTrakChance, float souvenirChance)
        {
            return new SkinValueModel(AnimationCurve.Linear(0f, 0f, 1f, 1f), AnimationCurve.Linear(0f, 0f, 1f, 1f), statTrakChance, souvenirChance, 2f, 0.7f);
        }
    }
}

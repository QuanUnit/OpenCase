using System;
using System.Collections.Generic;
using Features.Skins.Configs;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class SkinValueSampler
    {
        private readonly int _samplesCount;

        public SkinValueSampler(int samplesCount)
        {
            _samplesCount = samplesCount;
        }

        public SkinValueDistribution Sample(SkinValueModel model, PriceRange priceRange, bool isStatTrakAvailable, bool isSouvenirAvailable)
        {
            var statTrakChance = isStatTrakAvailable == true ? (double)model.StatTrakChance : 0d;
            var souvenirChance = isSouvenirAvailable == true ? (double)model.SouvenirChance : 0d;

            var variants = new List<PriceVariant>
            {
                new PriceVariant((1d - statTrakChance) * (1d - souvenirChance), 1d),
                new PriceVariant(statTrakChance * (1d - souvenirChance), model.StatTrakMultiplier),
                new PriceVariant((1d - statTrakChance) * souvenirChance, model.SouvenirMultiplier),
                new PriceVariant(statTrakChance * souvenirChance, (double)model.StatTrakMultiplier * model.SouvenirMultiplier),
            };

            variants.RemoveAll(variant => variant.Probability <= 0d);

            var baseValues = new double[_samplesCount];

            for (var index = 0; index < _samplesCount; index++)
            {
                var randomValue = (index + 0.5f) / _samplesCount;
                baseValues[index] = Evaluate(model, priceRange, randomValue);
            }

            var lowestBase = Evaluate(model, priceRange, 0f);
            var highestBase = Evaluate(model, priceRange, 1f);

            var values = new List<double>(_samplesCount * variants.Count);
            var probabilities = new List<double>(_samplesCount * variants.Count);
            var min = double.MaxValue;
            var max = double.MinValue;

            foreach (var variant in variants)
            {
                var sampleProbability = variant.Probability / _samplesCount;

                foreach (var baseValue in baseValues)
                {
                    values.Add(baseValue * variant.Multiplier);
                    probabilities.Add(sampleProbability);
                }

                min = Math.Min(min, Math.Min(lowestBase, highestBase) * variant.Multiplier);
                max = Math.Max(max, Math.Max(lowestBase, highestBase) * variant.Multiplier);
            }

            foreach (var value in values)
            {
                min = Math.Min(min, value);
                max = Math.Max(max, value);
            }

            return new SkinValueDistribution(values, probabilities, min, max);
        }

        private double Evaluate(SkinValueModel model, PriceRange priceRange, float randomValue)
        {
            var floatValue = model.FloatValueCurve.Evaluate(randomValue);
            var priceT = model.PriceByFloatValueCurve.Evaluate(floatValue);

            return priceRange.GetInRange(priceT);
        }

        private struct PriceVariant
        {
            public readonly double Probability;
            public readonly double Multiplier;

            public PriceVariant(double probability, double multiplier)
            {
                Probability = probability;
                Multiplier = multiplier;
            }
        }
    }
}

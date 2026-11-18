using System;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class CasePriceCalculator
    {
        private readonly decimal _priceStep;

        public CasePriceCalculator(float priceStep)
        {
            _priceStep = (decimal)priceStep;
        }

        public float GetPriceForReturnToPlayer(double expectedValue, double targetReturnToPlayer)
        {
            if (targetReturnToPlayer <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(targetReturnToPlayer), "Target RTP must be positive");
            }

            if (double.IsNaN(expectedValue) == true || double.IsInfinity(expectedValue) == true || expectedValue < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(expectedValue), "Expected value must be a finite non-negative number");
            }

            var rawPrice = (decimal)(expectedValue / targetReturnToPlayer);
            var steps = Math.Round(rawPrice / _priceStep, MidpointRounding.AwayFromZero);

            return (float)(Math.Max(1m, steps) * _priceStep);
        }
    }
}

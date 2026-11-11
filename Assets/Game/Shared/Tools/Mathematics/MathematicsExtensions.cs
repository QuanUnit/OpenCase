using System;
using Random = UnityEngine.Random;

namespace Shared.Tools.Mathematics
{
    public static class MathematicsExtensions
    {
        public static bool IsProke(this float normalizedChange)
        {
            if (normalizedChange < 0 || normalizedChange > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(normalizedChange), "Out of zero-one range");
            }
            
            var random = Random.value;

            return random <= normalizedChange;
        }
    }
}
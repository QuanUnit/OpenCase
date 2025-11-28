using System;
using Random = UnityEngine.Random;

namespace Tools.Mathematics
{
    public static class MathematicsExtensions
    {
        public static bool IsProke(this float normalizedChange)
        {
            if (normalizedChange < 0 || normalizedChange > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(normalizedChange), "Out of zero-one range");
            }
            
            float random = Random.value;

            return random <= normalizedChange;
        }
    }
}
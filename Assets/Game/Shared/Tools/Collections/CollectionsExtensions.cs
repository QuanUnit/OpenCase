using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Shared.Tools.Collections
{
    public static class CollectionsExtensions
    {
        public static T GetRandomReadOnly<T>(this IReadOnlyList<T> source)
        {
            var index = Random.Range(0, source.Count);
            return source[index];
        }

        public static void Shuffle<T>(this IList<T> source)
        {
            for (var index = source.Count - 1; index > 0; index--)
            {
                var swapIndex = Random.Range(0, index + 1);

                (source[index], source[swapIndex]) = (source[swapIndex], source[index]);
            }
        }

        public static int IndexOfMax<T>(this IReadOnlyList<T> source, Func<T, float> selector)
        {
            var maxIndex = -1;
            var maxValue = float.MinValue;

            for (var index = 0; index < source.Count; index++)
            {
                var value = selector.Invoke(source[index]);

                if (maxIndex == -1 || value > maxValue)
                {
                    maxIndex = index;
                    maxValue = value;
                }
            }

            return maxIndex;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace Shared.Tools.Collections
{
    public static class CollectionsExtensions
    {
        public static T GetRandomReadOnly<T>(this IReadOnlyList<T> source)
        {
            var index = Random.Range(0, source.Count);
            return source[index];
        }
    }
}
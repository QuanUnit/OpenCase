using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Shared.Tools.AssetsExtensions
{
    public static class AssetDatabaseTools
    {
#if UNITY_EDITOR
        
        public static List<T> FindAllAssets<T>() where T : UnityEngine.Object
        {
            var results = new List<T>();

            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);

                if (asset != null)
                {
                    results.Add(asset);
                }
            }

            return results;
        }
#endif

    }
}
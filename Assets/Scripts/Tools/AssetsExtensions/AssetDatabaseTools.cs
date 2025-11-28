using System.Collections.Generic;
using UnityEditor;

namespace Tools.AssetsExtensions
{
    public static class AssetDatabaseTools
    {
#if UNITY_EDITOR
        
        public static List<T> FindAllAssets<T>() where T : UnityEngine.Object
        {
            var results = new List<T>();

            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);

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
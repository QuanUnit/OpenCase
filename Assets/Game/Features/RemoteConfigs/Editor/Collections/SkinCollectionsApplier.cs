using System.Collections.Generic;
using Features.Skins.Configs;
using UnityEditor;

namespace Features.RemoteConfigs.Editor.Collections
{
    public class SkinCollectionsApplier
    {
        public SkinCollectionsReport Apply(IEnumerable<SkinConfig> skinConfigs, IReadOnlyDictionary<string, string> collections)
        {
            var report = new SkinCollectionsReport();

            foreach (var skinConfig in skinConfigs)
            {
                if (collections.TryGetValue(skinConfig.SkinStringId, out var collectionName) == false)
                {
                    report.Missing.Add(skinConfig.SkinStringId);
                    continue;
                }

                if (skinConfig.Collection == collectionName)
                {
                    report.Unchanged.Add(skinConfig.SkinStringId);
                    continue;
                }

                skinConfig.Collection = collectionName;
                EditorUtility.SetDirty(skinConfig);
                report.Assigned.Add(skinConfig.SkinStringId);
            }

            return report;
        }
    }
}

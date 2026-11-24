using System.Collections.Generic;
using System.Linq;
using Features.Skins.Configs;
using UnityEditor;

namespace Features.RemoteConfigs.Editor.Collections
{
    public class SkinCollectionsApplier
    {
        public SkinCollectionsReport Apply(IEnumerable<SkinConfig> skinConfigs, IReadOnlyDictionary<string, string> collections, SkinCollectionsConfig registry)
        {
            var report = new SkinCollectionsReport();

            foreach (var skinConfig in skinConfigs)
            {
                if (collections.TryGetValue(skinConfig.Id.Value, out var collectionName) == false)
                {
                    report.Missing.Add(skinConfig.Id.Value);
                    continue;
                }

                var collectionId = SkinCollectionNaming.ToId(collectionName);

                Register(registry, collectionName, report);

                if (skinConfig.Collection == collectionId)
                {
                    report.Unchanged.Add(skinConfig.Id.Value);
                    continue;
                }

                skinConfig.Collection = collectionId;
                EditorUtility.SetDirty(skinConfig);
                report.Assigned.Add(skinConfig.Id.Value);
            }

            return report;
        }

        private void Register(SkinCollectionsConfig registry, string collectionName, SkinCollectionsReport report)
        {
            var collectionId = SkinCollectionNaming.ToId(collectionName);

            if (registry.Collections.Any(entry => entry.Id == collectionId) == true)
            {
                return;
            }

            registry.Collections.Add(new SkinCollectionsConfig.Entry() { Id = collectionId, Name = collectionName });
            registry.Collections.Sort((first, second) => string.CompareOrdinal(first.Id.Value, second.Id.Value));
            EditorUtility.SetDirty(registry);
            report.Registered.Add(collectionId.Value);
        }
    }
}

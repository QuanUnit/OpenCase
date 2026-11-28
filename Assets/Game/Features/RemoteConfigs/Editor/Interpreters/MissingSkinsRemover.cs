using System.Collections.Generic;
using System.Linq;
using Features.Cases.Configs;
using Features.Skins.Configs;
using Shared.Tools.AssetsExtensions;
using UnityEditor;

namespace Features.RemoteConfigs.Editor.Interpreters
{
    public class MissingSkinsRemover
    {
        private readonly string _skinsFolder;

        public MissingSkinsRemover(string skinsFolder)
        {
            _skinsFolder = skinsFolder;
        }

        public List<SkinConfig> Select(IEnumerable<SkinConfig> skins, IEnumerable<string> missingIds)
        {
            var missingSet = new HashSet<string>(missingIds);

            return skins
                .Where(skin => missingSet.Contains(skin.Id.Value) == true)
                .ToList();
        }

        public HashSet<string> FindReferencedIds(IEnumerable<CaseConfig> cases)
        {
            var referenced = new HashSet<string>();

            foreach (var caseConfig in cases.Where(caseConfig => caseConfig.SkinsMapEntries != null))
            {
                foreach (var entry in caseConfig.SkinsMapEntries.Entries.Where(entry => entry.Key != null))
                {
                    referenced.Add(entry.Key.Id.Value);
                }
            }

            return referenced;
        }

        public HashSet<string> FindReferencedIds()
        {
            return FindReferencedIds(AssetDatabaseTools.FindAllAssets<CaseConfig>());
        }

        public MissingSkinsRemovalResult Remove(IEnumerable<string> missingIds)
        {
            var result = new MissingSkinsRemovalResult();
            var skins = Select(FindSkinsInFolder(), missingIds);
            var idsByPath = skins.ToDictionary(AssetDatabase.GetAssetPath, skin => skin.Id.Value);

            AssetDatabase.DeleteAssets(idsByPath.Keys.ToArray(), result.FailedPaths);

            var failed = new HashSet<string>(result.FailedPaths);
            result.DeletedIds.AddRange(idsByPath.Where(pair => failed.Contains(pair.Key) == false).Select(pair => pair.Value).Distinct());

            foreach (var allSkinsConfig in AssetDatabaseTools.FindAllAssets<AllSkinsConfig>())
            {
                allSkinsConfig.Collect();
                EditorUtility.SetDirty(allSkinsConfig);
                AssetDatabase.SaveAssetIfDirty(allSkinsConfig);
            }

            return result;
        }

        private List<SkinConfig> FindSkinsInFolder()
        {
            return AssetDatabase.FindAssets($"t:{nameof(SkinConfig)}", new[] { _skinsFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<SkinConfig>)
                .ToList();
        }
    }
}

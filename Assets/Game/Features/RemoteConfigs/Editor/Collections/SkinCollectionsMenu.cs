using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Features.RemoteConfigs.Editor.Exporters.Skins;
using Features.RemoteConfigs.Editor.Icons;
using Features.Skins.Configs;
using Shared.Tools.AssetsExtensions;
using Shared.Tools.Web;
using UnityEditor;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Collections
{
    public static class SkinCollectionsMenu
    {
        private const string ProgressTitle = "Skin Collections";

        [MenuItem("Tools/Skin Collections/Fill From CSGO-API")]
        public static async void FillFromSource()
        {
            try
            {
                var report = await Fill();

                Debug.Log($"Skin collections: assigned {report.Assigned.Count}, unchanged {report.Unchanged.Count}, missing {report.Missing.Count}, new collections {report.Registered.Count}");

                if (report.Missing.Count > 0)
                {
                    Debug.LogWarning("Skin collections missing for: " + string.Join(", ", report.Missing));
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static async Task<SkinCollectionsReport> Fill()
        {
            var config = AssetDatabaseTools.FindAllAssets<SkinIconsConfig>().First();

            var skinsJson = await DownloadTools.DownloadText(config.SkinsSourceUrl, config.SourceTimeoutSeconds, progress => EditorUtility.DisplayProgressBar(ProgressTitle, "Downloading skins source", progress));

            EditorUtility.DisplayProgressBar(ProgressTitle, "Applying collections", 1f);

            var skins = new SkinsSourcesParser().ParseSkins(skinsJson);
            var collections = new SkinCollectionsMapper().Map(skins);
            var skinConfigs = FindSkinConfigs(config.SkinConfigsPath);
            var registry = AssetDatabaseTools.FindAllAssets<SkinCollectionsConfig>().First();
            var report = new SkinCollectionsApplier().Apply(skinConfigs, collections, registry);

            AssetDatabase.SaveAssets();

            return report;
        }

        private static List<SkinConfig> FindSkinConfigs(string folderPath)
        {
            return AssetDatabase.FindAssets("t:SkinConfig", new[] { folderPath })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<SkinConfig>)
                .ToList();
        }
    }
}

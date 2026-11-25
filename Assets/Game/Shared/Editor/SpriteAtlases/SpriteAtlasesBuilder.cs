using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Shared.Editor.SpriteAtlases
{
    public static class SpriteAtlasesBuilder
    {
        private const string AtlasExtension = ".spriteatlasv2";

        public static List<SpriteAtlas> Rebuild(SpriteAtlasesConfig config)
        {
            var atlases = new List<SpriteAtlas>();

            foreach (var rule in config.Rules)
            {
                foreach (var sourceFolder in GetSourceFolders(rule))
                {
                    ApplySpriteSettings(rule, sourceFolder);

                    var atlas = BuildAtlas(rule, sourceFolder);
                    RegisterInAddressables(rule, atlas);
                    atlases.Add(atlas);
                }
            }

            AssetDatabase.SaveAssets();
            SpriteAtlasUtility.PackAtlases(atlases.ToArray(), EditorUserBuildSettings.activeBuildTarget);

            return atlases;
        }

        private static IEnumerable<string> GetSourceFolders(SpriteAtlasRule rule)
        {
            if (rule.AtlasPerSubfolder == false)
            {
                return new[] { rule.SourceFolderPath };
            }

            return AssetDatabase.GetSubFolders(rule.SourceFolderPath).OrderBy(path => path);
        }

        private static void ApplySpriteSettings(SpriteAtlasRule rule, string sourceFolder)
        {
            var texturePaths = AssetDatabase.FindAssets("t:Texture2D", new[] { sourceFolder }).Select(AssetDatabase.GUIDToAssetPath);

            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (var texturePath in texturePaths)
                {
                    var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

                    if (ApplySpriteSettings(rule, importer) == true)
                    {
                        importer.SaveAndReimport();
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        private static bool ApplySpriteSettings(SpriteAtlasRule rule, TextureImporter importer)
        {
            var isChanged = false;

            foreach (var platform in rule.Platforms)
            {
                var settings = importer.GetPlatformTextureSettings(platform.PlatformName);

                if (settings.overridden == true && settings.maxTextureSize == platform.SpriteMaxTextureSize)
                {
                    continue;
                }

                settings.overridden = true;
                settings.maxTextureSize = platform.SpriteMaxTextureSize;
                importer.SetPlatformTextureSettings(settings);
                isChanged = true;
            }

            return isChanged;
        }

        private static SpriteAtlas BuildAtlas(SpriteAtlasRule rule, string sourceFolder)
        {
            var atlasPath = $"{rule.OutputFolderPath}/{GetAtlasName(rule, sourceFolder)}{AtlasExtension}";

            var atlasAsset = new SpriteAtlasAsset();
            atlasAsset.Add(new[] { AssetDatabase.LoadAssetAtPath<Object>(sourceFolder) });
            SpriteAtlasAsset.Save(atlasAsset, atlasPath);
            AssetDatabase.ImportAsset(atlasPath);

            var importer = (SpriteAtlasImporter)AssetImporter.GetAtPath(atlasPath);
            importer.includeInBuild = true;
            importer.packingSettings = CreatePackingSettings(rule);
            importer.textureSettings = CreateTextureSettings();
            ApplyAtlasPlatformSettings(importer, rule);
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
        }

        private static string GetAtlasName(SpriteAtlasRule rule, string sourceFolder)
        {
            if (rule.AtlasPerSubfolder == true || string.IsNullOrEmpty(rule.AtlasName) == true)
            {
                return Path.GetFileName(sourceFolder);
            }

            return rule.AtlasName;
        }

        private static SpriteAtlasPackingSettings CreatePackingSettings(SpriteAtlasRule rule)
        {
            var settings = new SpriteAtlasPackingSettings();
            settings.enableRotation = false;
            settings.enableTightPacking = false;
            settings.enableAlphaDilation = true;
            settings.padding = rule.Padding;

            return settings;
        }

        private static SpriteAtlasTextureSettings CreateTextureSettings()
        {
            var settings = new SpriteAtlasTextureSettings();
            settings.readable = false;
            settings.generateMipMaps = false;
            settings.sRGB = true;
            settings.filterMode = FilterMode.Bilinear;
            settings.anisoLevel = 1;

            return settings;
        }

        private static void ApplyAtlasPlatformSettings(SpriteAtlasImporter importer, SpriteAtlasRule rule)
        {
            foreach (var platform in rule.Platforms)
            {
                var settings = importer.GetPlatformSettings(platform.PlatformName);
                settings.overridden = true;
                settings.format = platform.Format;
                settings.compressionQuality = platform.CompressionQuality;
                settings.crunchedCompression = IsCrunched(platform.Format);
                settings.maxTextureSize = platform.MaxTextureSize;
                importer.SetPlatformSettings(settings);
            }
        }

        private static bool IsCrunched(TextureImporterFormat format)
        {
            return format == TextureImporterFormat.DXT1Crunched
                || format == TextureImporterFormat.DXT5Crunched
                || format == TextureImporterFormat.ETC_RGB4Crunched
                || format == TextureImporterFormat.ETC2_RGBA8Crunched;
        }

        private static void RegisterInAddressables(SpriteAtlasRule rule, SpriteAtlas atlas)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var group = FindOrCreateGroup(settings, rule.AddressablesGroupName);
            var atlasPath = AssetDatabase.GetAssetPath(atlas);

            var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(atlasPath), group);
            entry.address = atlasPath;

            AssetDatabase.SaveAssetIfDirty(group);
            AssetDatabase.SaveAssetIfDirty(settings);
        }

        private static AddressableAssetGroup FindOrCreateGroup(AddressableAssetSettings settings, string groupName)
        {
            var group = settings.FindGroup(groupName);

            if (group != null)
            {
                return group;
            }

            return settings.CreateGroup(groupName, false, false, true, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
        }
    }
}

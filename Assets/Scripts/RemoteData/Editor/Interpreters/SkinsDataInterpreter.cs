using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using QCore.AddressablesWrapper.Containers;
using RemoteData.Editor.Models;
using Services.Skins.Configs;
using Tools.DebugExtensions;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace RemoteData.Editor.Interpreters
{
    [Serializable]
    public class SkinsDataInterpreter : IInterpreter
    {
        [SerializeField] private DefaultAsset _skinsFolder;
        [SerializeField] private DefaultAsset _skinsIconsSourceFolder;

        public void Interpret(List<IEntityModel> parsedData)
        {
            string skinsPath = AssetDatabase.GetAssetPath(_skinsFolder);
            string skinsIconsPath = AssetDatabase.GetAssetPath(_skinsIconsSourceFolder);

            SkinsConfigsIconsFinder skinsIconsFinder = new SkinsConfigsIconsFinder(skinsIconsPath);
            skinsIconsFinder.Warmup();

            Dictionary<string, SkinConfig> allSkinsMap = GetExistingSkins(skinsPath);
            List<SkinConfig> skinsWithoutIcons = new List<SkinConfig>();
            
            foreach (SkinDataModel skinDataModel in parsedData)
            {
                SkinConfig skinConfig = TryGetOrCreateNew(allSkinsMap, skinDataModel.Id, skinsPath);

                skinConfig.Price = new PriceRange(skinDataModel.Price.From, skinDataModel.Price.To);
                skinConfig.AvailableSouvenir = skinDataModel.AvailableSouvenir;
                skinConfig.AvailableStatTrak = skinDataModel.AvailableStatTrak;
                skinConfig.Rarity = skinDataModel.Rarity;
                skinConfig.Name = skinDataModel.Name;

                bool isIconsSet = skinsIconsFinder.TrySetIconToSkinConfig(skinConfig);

                if (isIconsSet == false)
                {
                    skinsWithoutIcons.Add(skinConfig);
                }
                
                EditorUtility.SetDirty(skinConfig);
                AssetDatabase.SaveAssetIfDirty(skinConfig);
            }

            LogSkinsConfigsWithoutIcons(skinsWithoutIcons);
        }

        private void LogSkinsConfigsWithoutIcons(IEnumerable<SkinConfig> skinsConfigs)
        {
            foreach (var skinConfig in skinsConfigs)
            {
                DebugColor.Log($"Icon not found for: {skinConfig.SkinStringId}", Color.yellow, skinConfig);
            }
        }

        private Dictionary<string, SkinConfig> GetExistingSkins(string path)
        {
            var skins = new Dictionary<string, SkinConfig>();
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(SkinConfig)}", new[] { path });

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                SkinConfig skin = AssetDatabase.LoadAssetAtPath<SkinConfig>(assetPath);

                if (skin != null && !skins.ContainsKey(skin.SkinStringId))
                {
                    skins.Add(skin.SkinStringId, skin);
                }
            }

            return skins;
        }

        private SkinConfig TryGetOrCreateNew(Dictionary<string, SkinConfig> skinsMap, string skinId, string skinsPath)
        {
            if (skinsMap.TryGetValue(skinId, out SkinConfig skinConfig))
            {
                return skinConfig;
            }

            SkinConfig newSkin = CrateNew(skinId, skinsPath);
            skinsMap.Add(skinId, newSkin);

            return newSkin;
        }

        private SkinConfig CrateNew(string skinId, string skinsPath)
        {
            SkinConfig newSkin = ScriptableObject.CreateInstance<SkinConfig>();
            newSkin.CreateId();
            newSkin.SkinStringId = skinId;

            string assetPath = $"{skinsPath}/SkinConfig_{skinId}.asset";

            AssetDatabase.CreateAsset(newSkin, assetPath);

            return newSkin;
        }
    }

    public class SkinsConfigsIconsFinder
    {
        private bool IsWarmup => _iconsPaths.Count > 0;

        private readonly string _iconsRootPath;
        private readonly List<string> _iconsPaths;
        private readonly char[] _splitters;

        public SkinsConfigsIconsFinder(string iconsRootPath)
        {
            _iconsRootPath = iconsRootPath;
            _iconsPaths = new List<string>();
            _splitters = new[] { '_', '-', ' ', '.' };
        }

        public void Warmup()
        {
            string[] rawIconsPaths = Directory.GetFiles(_iconsRootPath, "*.png", SearchOption.AllDirectories);

            foreach (var rawIconPath in rawIconsPaths)
            {
                string iconPath = rawIconPath.Replace("\\", "/");
                _iconsPaths.Add(iconPath);
            }
        }

        public bool TrySetIconToSkinConfig(SkinConfig skinConfig)
        {
            if (skinConfig == null)
            {
                return false;
            }

            int maxMatches = 0;
            string bestIconPath = null;
            string bestIconName = null;
            
            string[] skinNameParts = skinConfig.SkinStringId.Split(_splitters, StringSplitOptions.RemoveEmptyEntries);

            foreach (string iconPath in _iconsPaths)
            {
                string iconName = Path.GetFileNameWithoutExtension(iconPath);
                string[] iconNameParts = iconName.Split(_splitters, StringSplitOptions.RemoveEmptyEntries);
                if (iconNameParts.Length == 0 || skinNameParts.Length == 0) continue;
                
                int matches = iconNameParts.Count(iconPart => skinNameParts.Any(skinPart =>
                    string.Equals(iconPart, skinPart, StringComparison.OrdinalIgnoreCase)));
                
                if (matches > maxMatches)
                {
                    maxMatches = matches;
                    bestIconPath = iconPath;
                    bestIconName = iconName;
                }
            }

            if (bestIconPath != null && LastWordIsTheSame(bestIconName, skinConfig.SkinStringId))
            {
                LoadAndSetIconToSkinConfig(bestIconPath, skinConfig);
                return true;
            }

            ResetSkinConfigIcon(skinConfig);
            return false;
        }

        private void LoadAndSetIconToSkinConfig(string iconPath, SkinConfig skinConfig)
        {
            Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);

            if (icon != null)
            {
                string iconGuid = AssetDatabase.AssetPathToGUID(iconPath);

                skinConfig.ContextData = new SkinConfig.UIContextData()
                {
                    SpriteSource = new AddressableLoadContainerSprite(iconGuid)
                };
            }
        }

        private void ResetSkinConfigIcon(SkinConfig skinConfig)
        {
            skinConfig.ContextData = new SkinConfig.UIContextData()
            {
                SpriteSource = new AddressableLoadContainerSprite(),
            };
        }

        private bool LastWordIsTheSame(string skinName, string iconName)
        {
            return skinName.Split(_splitters, StringSplitOptions.RemoveEmptyEntries)[^1] ==
                   iconName.Split(_splitters, StringSplitOptions.RemoveEmptyEntries)[^1];
        }
    }
}
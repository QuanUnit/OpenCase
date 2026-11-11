using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using QCore.AddressablesWrapper.Containers;
using Features.RemoteConfigs.Editor.Models;
using Features.Skins.Configs;
using Shared.Tools.DebugExtensions;
using UnityEditor;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Interpreters
{
    [Serializable]
    public class SkinsDataInterpreter : IInterpreter
    {
        [SerializeField] private DefaultAsset _skinsFolder;
        [SerializeField] private DefaultAsset _skinsIconsSourceFolder;

        public void Interpret(List<IEntityModel> parsedData)
        {
            var skinsPath = AssetDatabase.GetAssetPath(_skinsFolder);
            var skinsIconsPath = AssetDatabase.GetAssetPath(_skinsIconsSourceFolder);

            var skinsIconsFinder = new SkinsConfigsIconsFinder(skinsIconsPath);
            skinsIconsFinder.Warmup();

            var allSkinsMap = GetExistingSkins(skinsPath);
            var skinsWithoutIcons = new List<SkinConfig>();
            
            foreach (SkinDataModel skinDataModel in parsedData)
            {
                var skinConfig = TryGetOrCreateNew(allSkinsMap, skinDataModel.Id, skinsPath);

                skinConfig.Price = new PriceRange(skinDataModel.Price.From, skinDataModel.Price.To);
                skinConfig.AvailableSouvenir = skinDataModel.AvailableSouvenir;
                skinConfig.AvailableStatTrak = skinDataModel.AvailableStatTrak;
                skinConfig.Rarity = skinDataModel.Rarity;
                skinConfig.Name = skinDataModel.Name;

                var isIconsSet = skinsIconsFinder.TrySetIconToSkinConfig(skinConfig);

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
            var guids = AssetDatabase.FindAssets($"t:{nameof(SkinConfig)}", new[] { path });

            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var skin = AssetDatabase.LoadAssetAtPath<SkinConfig>(assetPath);

                if (skin != null && !skins.ContainsKey(skin.SkinStringId))
                {
                    skins.Add(skin.SkinStringId, skin);
                }
            }

            return skins;
        }

        private SkinConfig TryGetOrCreateNew(Dictionary<string, SkinConfig> skinsMap, string skinId, string skinsPath)
        {
            if (skinsMap.TryGetValue(skinId, out var skinConfig))
            {
                return skinConfig;
            }

            var newSkin = CrateNew(skinId, skinsPath);
            skinsMap.Add(skinId, newSkin);

            return newSkin;
        }

        private SkinConfig CrateNew(string skinId, string skinsPath)
        {
            var newSkin = ScriptableObject.CreateInstance<SkinConfig>();
            newSkin.CreateId();
            newSkin.SkinStringId = skinId;

            var assetPath = $"{skinsPath}/SkinConfig_{skinId}.asset";

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
            var rawIconsPaths = Directory.GetFiles(_iconsRootPath, "*.png", SearchOption.AllDirectories);

            foreach (var rawIconPath in rawIconsPaths)
            {
                var iconPath = rawIconPath.Replace("\\", "/");
                _iconsPaths.Add(iconPath);
            }
        }

        public bool TrySetIconToSkinConfig(SkinConfig skinConfig)
        {
            if (skinConfig == null)
            {
                return false;
            }

            var maxMatches = 0;
            string bestIconPath = null;
            string bestIconName = null;
            
            var skinNameParts = skinConfig.SkinStringId.Split(_splitters, StringSplitOptions.RemoveEmptyEntries);

            foreach (var iconPath in _iconsPaths)
            {
                var iconName = Path.GetFileNameWithoutExtension(iconPath);
                var iconNameParts = iconName.Split(_splitters, StringSplitOptions.RemoveEmptyEntries);
                if (iconNameParts.Length == 0 || skinNameParts.Length == 0) continue;
                
                var matches = iconNameParts.Count(iconPart => skinNameParts.Any(skinPart =>
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
            var icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);

            if (icon != null)
            {
                var iconGuid = AssetDatabase.AssetPathToGUID(iconPath);

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
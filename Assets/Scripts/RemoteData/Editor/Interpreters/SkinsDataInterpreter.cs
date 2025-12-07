using System;
using System.Collections.Generic;
using System.IO;
using QCore.AddressablesWrapper.Containers;
using RemoteData.Editor.Models;
using Services.Skins.Configs;
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
            
            Dictionary<string, SkinConfig> allSkinsMap = GetExistingSkins(skinsPath);
            
            foreach (SkinDataModel skinDataModel in parsedData)
            {
                SkinConfig skinConfig = TryGetOrCreateNew(allSkinsMap, skinDataModel.Id, skinsPath);

                skinConfig.Price = new PriceRange(skinDataModel.Price.Min, skinDataModel.Price.Max);
                skinConfig.AvailableSouvenir = skinDataModel.AvailableSouvenir;
                skinConfig.AvailableStatTrak = skinDataModel.AvailableStatTrak;
                skinConfig.Rarity = skinDataModel.Rarity;

                TrySetIconToSkin(skinConfig, skinsIconsPath);
                
                EditorUtility.SetDirty(skinConfig);
                AssetDatabase.SaveAssetIfDirty(skinConfig);
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

        private bool TrySetIconToSkin(SkinConfig skinConfig, string iconsRootPath)
        {
            if (skinConfig == null || string.IsNullOrEmpty(iconsRootPath))
                return false;

            string[] files = Directory.GetFiles(iconsRootPath, "*.png", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                string assetPath = file.Replace("\\", "/");
                string fileName = Path.GetFileNameWithoutExtension(assetPath);

                string[] fileParts = fileName.Split('_', StringSplitOptions.RemoveEmptyEntries);
                string[] skinParts = skinConfig.name.Split('_', StringSplitOptions.RemoveEmptyEntries);

                if (fileParts.Length > 0 && skinParts.Length > 0)
                {
                    string lastFileWord = fileParts[^1];
                    string lastSkinWord = skinParts[^1];

                    if (string.Equals(lastFileWord, lastSkinWord, StringComparison.OrdinalIgnoreCase))
                    {
                        Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                        if (icon != null)
                        {
                            string iconGuid = AssetDatabase.AssetPathToGUID(assetPath);

                            skinConfig.ContextData = new SkinConfig.UIContextData()
                            {
                                SpriteSource = new AddressableLoadContainerSprite(iconGuid)
                            };

                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
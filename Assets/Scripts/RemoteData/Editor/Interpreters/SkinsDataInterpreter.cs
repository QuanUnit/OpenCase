using System;
using System.Collections.Generic;
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
        
        public void Interpret(List<IEntityModel> parsedData)
        {
            string path = AssetDatabase.GetAssetPath(_skinsFolder);
            Dictionary<string, SkinConfig> allSkinsMap = GetExistingSkins(path);
            
            foreach (SkinDataModel skinDataModel in parsedData)
            {
                SkinConfig skinConfig = TryGetOrCreateNew(allSkinsMap, skinDataModel.Id, path);

                skinConfig.Price = new PriceRange(skinDataModel.Price.Min, skinDataModel.Price.Max);
                skinConfig.AvailableSouvenir = skinDataModel.AvailableSouvenir;
                skinConfig.AvailableStatTrak = skinDataModel.AvailableStatTrak;
                skinConfig.Rarity = skinDataModel.Rarity;
                
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
            newSkin.name = $"SkinConfig_{skinId}";
            newSkin.CreateId();
            newSkin.SkinStringId = skinId;

            string assetPath = $"{skinsPath}/{skinId}.asset";

            AssetDatabase.CreateAsset(newSkin, assetPath);
            EditorUtility.SetDirty(newSkin);
            AssetDatabase.SaveAssetIfDirty(newSkin);
            
            return newSkin;
        }
    }
}
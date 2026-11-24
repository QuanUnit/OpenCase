using System;
using System.Collections.Generic;
using System.Linq;
using QCore.AddressablesWrapper.Containers;
using Features.RemoteConfigs.Editor.Models;
using Features.Skins.Configs;
using UnityEditor;
using UnityEngine;
using Features.Skins.Data;

namespace Features.RemoteConfigs.Editor.Interpreters
{
    [Serializable]
    public class SkinsDataInterpreter : IInterpreter
    {
        [SerializeField] private DefaultAsset _skinsFolder;

        public ImportReport Interpret(List<IEntityModel> parsedData)
        {
            var report = new ImportReport();
            var skinsPath = AssetDatabase.GetAssetPath(_skinsFolder);
            var allSkinsMap = GetExistingSkins(skinsPath);
            var existingIds = allSkinsMap.Keys.ToList();
            var skinDataModels = parsedData.Cast<SkinDataModel>().ToList();

            var changedConfigs = new List<SkinConfig>();

            AssetDatabase.StartAssetEditing();

            try
            {
                foreach (var skinDataModel in skinDataModels)
                {
                    try
                    {
                        changedConfigs.Add(Apply(skinDataModel, allSkinsMap, skinsPath, report));
                    }
                    catch (Exception exception)
                    {
                        report.Errors.Add($"{skinDataModel.Id}: {exception.Message}");
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                Save(changedConfigs);
            }

            var idsComparer = new ImportIdsComparer();
            report.Missing.AddRange(idsComparer.FindMissing(existingIds, skinDataModels.Select(model => model.Id)));

            return report;
        }

        private SkinConfig Apply(SkinDataModel skinDataModel, Dictionary<string, SkinConfig> allSkinsMap, string skinsPath, ImportReport report)
        {
            var skinConfig = GetOrCreate(allSkinsMap, skinDataModel.Id, skinsPath, report);

            skinConfig.Price = new PriceRange(skinDataModel.Price.From, skinDataModel.Price.To);
            skinConfig.AvailableSouvenir = skinDataModel.AvailableSouvenir;
            skinConfig.AvailableStatTrak = skinDataModel.AvailableStatTrak;
            skinConfig.Rarity = skinDataModel.Rarity;
            skinConfig.Name = skinDataModel.Name;

            EditorUtility.SetDirty(skinConfig);

            return skinConfig;
        }

        private void Save(List<SkinConfig> changedConfigs)
        {
            foreach (var skinConfig in changedConfigs)
            {
                AssetDatabase.SaveAssetIfDirty(skinConfig);
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

                if (skin != null && skins.ContainsKey(skin.Id.Value) == false)
                {
                    skins.Add(skin.Id.Value, skin);
                }
            }

            return skins;
        }

        private SkinConfig GetOrCreate(Dictionary<string, SkinConfig> skinsMap, string skinId, string skinsPath, ImportReport report)
        {
            if (skinsMap.TryGetValue(skinId, out var skinConfig) == true)
            {
                AddUnique(report.Updated, skinId);
                return skinConfig;
            }

            var newSkin = CreateNew(skinId, skinsPath);
            skinsMap.Add(skinId, newSkin);
            report.Created.Add(skinId);

            return newSkin;
        }

        private SkinConfig CreateNew(string skinId, string skinsPath)
        {
            var newSkin = ScriptableObject.CreateInstance<SkinConfig>();
            newSkin.Id = new SkinId(skinId);
            newSkin.ContextData = new SkinConfig.UIContextData
            {
                SpriteSource = new AddressableLoadContainerSprite(),
            };

            var assetPath = $"{skinsPath}/SkinConfig_{skinId}.asset";

            AssetDatabase.CreateAsset(newSkin, assetPath);

            return newSkin;
        }

        private void AddUnique(List<string> list, string value)
        {
            if (list.Contains(value) == false)
            {
                list.Add(value);
            }
        }
    }
}

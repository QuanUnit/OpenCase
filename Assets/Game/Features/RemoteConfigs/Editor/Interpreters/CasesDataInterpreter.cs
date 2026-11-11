using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QCore.AddressablesWrapper.Containers;
using QCore.Tools.Collections;
using Features.RemoteConfigs.Editor.Models;
using Features.Cases.Configs;
using Features.Skins.Configs;
using Shared.Tools.AssetsExtensions;
using UnityEditor;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Interpreters
{
    [Serializable]
    public class CasesDataInterpreter : IInterpreter
    {
        [SerializeField] private DefaultAsset _casesFolder;
        
        public void Interpret(List<IEntityModel> parsedData)
        {
            var skinsPath = AssetDatabase.GetAssetPath(_casesFolder);
            
            var allCasesMap = GetExistingCases(skinsPath);
            var allSkinsMap = GetExistingSkins();
            
            foreach (CaseDataModel caseDataModel in parsedData)
            {
                var caseConfig = TryGetOrCreateNew(allCasesMap, caseDataModel.Id, skinsPath);

                caseConfig.Name = caseDataModel.Name;
                caseConfig.Price = caseDataModel.Price;

                var skinsMap = new WeightedMap<SkinConfig>();

                foreach (var pair in caseDataModel.SkinsMap)
                {
                    var skinConfig = allSkinsMap[pair.Key];
                    var weight = pair.Value;
                    skinsMap.Add(skinConfig, weight);
                }

                caseConfig.SkinsMapEntries = skinsMap;
                
                EditorUtility.SetDirty(caseConfig);
                AssetDatabase.SaveAssetIfDirty(caseConfig);
            }
        }

        private Dictionary<string, CaseConfig> GetExistingCases(string path)
        {
            var skins = new Dictionary<string, CaseConfig>();
            var guids = AssetDatabase.FindAssets($"t:{nameof(CaseConfig)}", new[] { path });

            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var caseConfig = AssetDatabase.LoadAssetAtPath<CaseConfig>(assetPath);

                if (caseConfig != null && !skins.ContainsKey(caseConfig.CaseStringId))
                {
                    skins.Add(caseConfig.CaseStringId, caseConfig);
                }
            }

            return skins;
        }

        private Dictionary<string, SkinConfig> GetExistingSkins()
        {
            var skins = AssetDatabaseTools.FindAllAssets<SkinConfig>();

            return skins.ToDictionary(a => a.SkinStringId, b => b);
        }

        private CaseConfig TryGetOrCreateNew(Dictionary<string, CaseConfig> casesMap, string caseId, string casesPath)
        {
            if (casesMap.TryGetValue(caseId, out var caseConfig))
            {
                return caseConfig;
            }

            var newCase = CrateNew(caseId, casesPath);
            casesMap.Add(caseId, newCase);

            return newCase;
        }

        private CaseConfig CrateNew(string caseId, string casesPath)
        {
            var caseConfig = ScriptableObject.CreateInstance<CaseConfig>();
            caseConfig.CreateId();
            caseConfig.CaseStringId = caseId;

            var assetPath = $"{casesPath}/CaseConfig_{caseId}.asset";

            AssetDatabase.CreateAsset(caseConfig, assetPath);
            
            return caseConfig;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QCore.AddressablesWrapper.Containers;
using QCore.Tools.Collections;
using RemoteData.Editor.Models;
using Services.Cases.Configs;
using Services.Skins.Configs;
using Tools.AssetsExtensions;
using UnityEditor;
using UnityEngine;

namespace RemoteData.Editor.Interpreters
{
    [Serializable]
    public class CasesDataInterpreter : IInterpreter
    {
        [SerializeField] private DefaultAsset _casesFolder;
        
        public void Interpret(List<IEntityModel> parsedData)
        {
            string skinsPath = AssetDatabase.GetAssetPath(_casesFolder);
            
            Dictionary<string, CaseConfig> allCasesMap = GetExistingCases(skinsPath);
            Dictionary<string, SkinConfig> allSkinsMap = GetExistingSkins();
            
            foreach (CaseDataModel caseDataModel in parsedData)
            {
                CaseConfig caseConfig = TryGetOrCreateNew(allCasesMap, caseDataModel.Id, skinsPath);

                caseConfig.Name = caseDataModel.Name;
                caseConfig.Price = caseDataModel.Price;

                WeightedMap<SkinConfig> skinsMap = new WeightedMap<SkinConfig>();

                foreach (var pair in caseDataModel.SkinsMap)
                {
                    SkinConfig skinConfig = allSkinsMap[pair.Key];
                    float weight = pair.Value;
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
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(CaseConfig)}", new[] { path });

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                CaseConfig caseConfig = AssetDatabase.LoadAssetAtPath<CaseConfig>(assetPath);

                if (caseConfig != null && !skins.ContainsKey(caseConfig.CaseStringId))
                {
                    skins.Add(caseConfig.CaseStringId, caseConfig);
                }
            }

            return skins;
        }

        private Dictionary<string, SkinConfig> GetExistingSkins()
        {
            List<SkinConfig> skins = AssetDatabaseTools.FindAllAssets<SkinConfig>();

            return skins.ToDictionary(a => a.SkinStringId, b => b);
        }

        private CaseConfig TryGetOrCreateNew(Dictionary<string, CaseConfig> casesMap, string caseId, string casesPath)
        {
            if (casesMap.TryGetValue(caseId, out CaseConfig caseConfig))
            {
                return caseConfig;
            }

            CaseConfig newCase = CrateNew(caseId, casesPath);
            casesMap.Add(caseId, newCase);

            return newCase;
        }

        private CaseConfig CrateNew(string caseId, string casesPath)
        {
            CaseConfig caseConfig = ScriptableObject.CreateInstance<CaseConfig>();
            caseConfig.CreateId();
            caseConfig.CaseStringId = caseId;

            string assetPath = $"{casesPath}/CaseConfig_{caseId}.asset";

            AssetDatabase.CreateAsset(caseConfig, assetPath);
            
            return caseConfig;
        }
    }
}
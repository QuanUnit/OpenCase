using System;
using System.Collections.Generic;
using System.Linq;
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

        public ImportReport Interpret(List<IEntityModel> parsedData)
        {
            var report = new ImportReport();
            var casesPath = AssetDatabase.GetAssetPath(_casesFolder);

            var allCasesMap = GetExistingCases(casesPath);
            var existingIds = allCasesMap.Keys.ToList();
            var allSkinsMap = GetExistingSkins();
            var caseDataModels = parsedData.Cast<CaseDataModel>().ToList();

            var changedConfigs = new List<CaseConfig>();

            AssetDatabase.StartAssetEditing();

            try
            {
                foreach (var caseDataModel in caseDataModels)
                {
                    try
                    {
                        changedConfigs.Add(Apply(caseDataModel, allCasesMap, allSkinsMap, casesPath, report));
                    }
                    catch (Exception exception)
                    {
                        report.Errors.Add($"{caseDataModel.Id}: {exception.Message}");
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                Save(changedConfigs);
            }

            var idsComparer = new ImportIdsComparer();
            report.Missing.AddRange(idsComparer.FindMissing(existingIds, caseDataModels.Select(model => model.Id)));

            return report;
        }

        private CaseConfig Apply(CaseDataModel caseDataModel, Dictionary<string, CaseConfig> allCasesMap, Dictionary<string, SkinConfig> allSkinsMap, string casesPath, ImportReport report)
        {
            var caseConfig = GetOrCreate(allCasesMap, caseDataModel.Id, casesPath, report);

            caseConfig.Name = caseDataModel.Name;
            caseConfig.Price = caseDataModel.Price;
            caseConfig.SkinsMapEntries = BuildSkinsMap(caseDataModel, allSkinsMap, report);

            EditorUtility.SetDirty(caseConfig);

            return caseConfig;
        }

        private void Save(List<CaseConfig> changedConfigs)
        {
            foreach (var caseConfig in changedConfigs)
            {
                AssetDatabase.SaveAssetIfDirty(caseConfig);
            }
        }

        public WeightedMap<SkinConfig> BuildSkinsMap(CaseDataModel caseDataModel, IReadOnlyDictionary<string, SkinConfig> skinsById, ImportReport report)
        {
            var skinsMap = new WeightedMap<SkinConfig>();

            foreach (var pair in caseDataModel.SkinsMap)
            {
                if (skinsById.TryGetValue(pair.Key, out var skinConfig) == false)
                {
                    report.UnknownReferences.Add($"{caseDataModel.Id}: {pair.Key}");
                    continue;
                }

                skinsMap.Add(skinConfig, pair.Value);
            }

            return skinsMap;
        }

        private Dictionary<string, CaseConfig> GetExistingCases(string path)
        {
            var cases = new Dictionary<string, CaseConfig>();
            var guids = AssetDatabase.FindAssets($"t:{nameof(CaseConfig)}", new[] { path });

            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var caseConfig = AssetDatabase.LoadAssetAtPath<CaseConfig>(assetPath);

                if (caseConfig != null && cases.ContainsKey(caseConfig.CaseStringId) == false)
                {
                    cases.Add(caseConfig.CaseStringId, caseConfig);
                }
            }

            return cases;
        }

        private Dictionary<string, SkinConfig> GetExistingSkins()
        {
            var skins = AssetDatabaseTools.FindAllAssets<SkinConfig>();

            return skins.ToDictionary(skin => skin.SkinStringId, skin => skin);
        }

        private CaseConfig GetOrCreate(Dictionary<string, CaseConfig> casesMap, string caseId, string casesPath, ImportReport report)
        {
            if (casesMap.TryGetValue(caseId, out var caseConfig) == true)
            {
                report.Updated.Add(caseId);
                return caseConfig;
            }

            var newCase = CreateNew(caseId, casesPath);
            casesMap.Add(caseId, newCase);
            report.Created.Add(caseId);

            return newCase;
        }

        private CaseConfig CreateNew(string caseId, string casesPath)
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

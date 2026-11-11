using System.Linq;
using Features.Cases.Configs;
using Features.Skins.Data;
using Features.Skins.Services;
using Features.Skins.Configs;
using Shared.Tools.AssetsExtensions;
using UnityEditor;
using UnityEngine;

namespace Features.RemoteConfigs.Editor
{
    public class CasesStatisticEditorWindow : EditorWindow
    {
        private CaseConfig _caseConfig;
        private int _opensCount;
        private string _statisticsText = "";

        [MenuItem("Tools/Cases Statistic")]
        public static void ShowWindow()
        {
            var window = GetWindow<CasesStatisticEditorWindow>("Cases Statistic");
            window._caseConfig = AssetDatabaseTools.FindAllAssets<CaseConfig>().First();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Main settings:", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            _caseConfig =
                (CaseConfig)EditorGUILayout.ObjectField("Case config:", _caseConfig, typeof(CaseConfig), false);
            _opensCount = EditorGUILayout.IntField("Opens count:", _opensCount);
            
            EditorGUILayout.Space();
            GUI.enabled = _caseConfig != null;
            if (GUILayout.Button("Run", GUILayout.Height(30)))
            {
                CalculateStatistic(_opensCount);
            }

            GUI.enabled = true;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Statistics:", EditorStyles.boldLabel);
            _statisticsText = EditorGUILayout.TextArea(_statisticsText, GUILayout.Height(150));
        }

        private void CalculateStatistic(int opensCount)
        {
            SkinsGlobalSettingsConfig skinsGlobalSettingsConfig =
                AssetDatabaseTools.FindAllAssets<SkinsGlobalSettingsConfig>().First();
            var skinFactory = new SkinFactory(skinsGlobalSettingsConfig);
            var totalSpent = 0f;
            var totalEarn = 0f;
            for (var i = 0; i < opensCount; i++)
            {
                var casePrice = _caseConfig.Price;
                totalSpent += casePrice;
                var skinConfig = _caseConfig.SkinsMapEntries.GetRandom();
                var skin = skinFactory.CreateRandom(skinConfig);
                totalEarn += skin.Price;
            }

            var totalDelta = totalEarn - totalSpent;
            var oneOpenDelta = totalDelta / opensCount;
            _statisticsText = $"Count: {opensCount}\n" + $"Spent: {totalSpent}\n" + $"Earn: {totalEarn}\n" +
                              $"Delta: {totalDelta}\n" + $"x1_Delta: {oneOpenDelta}";
            Debug.Log(_statisticsText);
        }
    }
}
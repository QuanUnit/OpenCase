using System.Linq;
using Services.Cases.Configs;
using Services.Skins;
using Services.Skins.Configs;
using Tools.AssetsExtensions;
using UnityEditor;
using UnityEngine;

namespace RemoteData.Editor
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
            ISkinFactory skinFactory = new SkinFactory(skinsGlobalSettingsConfig);
            float totalSpent = 0;
            float totalEarn = 0;
            for (int i = 0; i < opensCount; i++)
            {
                float casePrice = _caseConfig.Price;
                totalSpent += casePrice;
                SkinConfig skinConfig = _caseConfig.SkinsMapEntries.GetRandom();
                Skin skin = skinFactory.CreateRandom(skinConfig);
                totalEarn += skin.Price;
            }

            float totalDelta = totalEarn - totalSpent;
            float oneOpenDelta = totalDelta / opensCount;
            _statisticsText = $"Count: {opensCount}\n" + $"Spent: {totalSpent}\n" + $"Earn: {totalEarn}\n" +
                              $"Delta: {totalDelta}\n" + $"x1_Delta: {oneOpenDelta}";
            Debug.Log(_statisticsText);
        }
    }
}
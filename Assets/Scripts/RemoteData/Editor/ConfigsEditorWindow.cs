using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RemoteData.Editor.Models;
using Tools.AssetsExtensions;
using Tools.DebugExtensions;
using UnityEditor;
using UnityEngine;

namespace RemoteData.Editor
{
    public class ConfigsEditorWindow : EditorWindow
    {
        private EditorRemoteConfigsData _remoteConfigsData;
        
        [MenuItem("Tools/Remotes Configs Importer")]
        public static void ShowWindow()
        {
            var window = GetWindow<ConfigsEditorWindow>("Remote Configs Importer");
            window._remoteConfigsData = AssetDatabaseTools.FindAllAssets<EditorRemoteConfigsData>().First();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Main settings:", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            _remoteConfigsData = (EditorRemoteConfigsData)EditorGUILayout.ObjectField(
                "CSV Files:",
                _remoteConfigsData,
                typeof(EditorRemoteConfigsData),
                false
            );

            EditorGUILayout.Space();

            GUI.enabled = _remoteConfigsData != null;
            
            if (GUILayout.Button("Import Remote Data", GUILayout.Height(30)))
            {
                ImportCSVData().Forget();
            }
            
            GUI.enabled = true;
        }

        private async UniTask ImportCSVData()
        {
            List<UniTask> tasks = new List<UniTask>();
        
            foreach (var page in _remoteConfigsData.Pages)
            {
                UniTask task = ParseAndInterpretPage(_remoteConfigsData.SheetId, page);
                tasks.Add(task);
            }

            await UniTask.WhenAll(tasks);
        }

        private async UniTask ParseAndInterpretPage(string sheetId, EditorRemoteConfigsData.Page page)
        {
            List<IEntityModel> parsedEntities = await page.Parser.ParseData(sheetId, page.Id);
            page.Interpreter.Interpret(parsedEntities);
            
            DebugColor.Log($"Page {page.Name} parsed and interpreted successfully", Color.green);
        }
    }
}
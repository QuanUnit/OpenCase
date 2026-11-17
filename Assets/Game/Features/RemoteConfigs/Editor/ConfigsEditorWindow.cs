using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Features.RemoteConfigs.Editor.Icons;
using Features.RemoteConfigs.Editor.Interpreters;
using Shared.Tools.AssetsExtensions;
using UnityEditor;
using UnityEngine;

namespace Features.RemoteConfigs.Editor
{
    public class ConfigsEditorWindow : EditorWindow
    {
        private const string Title = "Remote Configs Importer";
        private const float ButtonHeight = 30f;
        private const string DownloadIconsPrefsKey = "OpenCase.RemoteConfigsImporter.DownloadMissingIcons";

        private EditorRemoteConfigsData _remoteConfigsData;
        private bool _isBusy;
        private bool _isDownloadingIcons;

        [MenuItem("Tools/Remotes Configs Importer")]
        public static void ShowWindow()
        {
            var window = GetWindow<ConfigsEditorWindow>(Title);
            window._remoteConfigsData = AssetDatabaseTools.FindAllAssets<EditorRemoteConfigsData>().First();
        }

        private void OnEnable()
        {
            _isDownloadingIcons = EditorPrefs.GetBool(DownloadIconsPrefsKey, true);
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

            var isDownloadingIcons = EditorGUILayout.Toggle("Download missing icons after import", _isDownloadingIcons);

            if (isDownloadingIcons != _isDownloadingIcons)
            {
                _isDownloadingIcons = isDownloadingIcons;
                EditorPrefs.SetBool(DownloadIconsPrefsKey, _isDownloadingIcons);
            }

            EditorGUILayout.Space();

            GUI.enabled = _remoteConfigsData != null && _isBusy == false;

            if (GUILayout.Button("Import Remote Data", GUILayout.Height(ButtonHeight)) == true)
            {
                ImportCsvData().Forget();
            }

            GUI.enabled = true;
        }

        private async UniTask ImportCsvData()
        {
            _isBusy = true;

            try
            {
                var importer = new RemoteConfigsImporter();
                var pages = _remoteConfigsData.Pages;

                var reports = await importer.Import(_remoteConfigsData.SheetId, pages, (pageName, index) => PageStartedHandle(pageName, index, pages.Count));

                EditorUtility.ClearProgressBar();
                ShowSummary(reports);

                if (_isDownloadingIcons == true && reports.Any(report => report.Errors.Count > 0) == true)
                {
                    Debug.LogWarning("Missing icons were not downloaded because the import reported errors");
                }
                else if (_isDownloadingIcons == true)
                {
                    await DownloadMissingIcons();
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                _isBusy = false;
                Repaint();
            }
        }

        private async UniTask DownloadMissingIcons()
        {
            var config = AssetDatabaseTools.FindAllAssets<SkinIconsConfig>().First();
            var runner = new SkinIconsRunner(config);

            var report = await runner.Run(SkinIconsMode.OnlyMissing, false, IconsProgressChangedHandle);

            EditorUtility.ClearProgressBar();
            runner.Log(report);
            EditorUtility.DisplayDialog(Title, $"{report.GetShortSummary()}\n\nDetails are in the console.", "OK");
        }

        private bool IconsProgressChangedHandle(string stage, float progress)
        {
            return EditorUtility.DisplayCancelableProgressBar(Title, stage, progress);
        }

        private void PageStartedHandle(string pageName, int index, int pagesCount)
        {
            EditorUtility.DisplayProgressBar(Title, $"Importing page {pageName}", (float)index / pagesCount);
        }

        private void ShowSummary(List<ImportReport> reports)
        {
            var dialogText = new StringBuilder();

            foreach (var report in reports)
            {
                dialogText.AppendLine(report.GetShortSummary());

                if (report.Errors.Count > 0)
                {
                    Debug.LogError(report.GetFullSummary());
                }
                else if (report.HasProblems == true)
                {
                    Debug.LogWarning(report.GetFullSummary());
                }
                else
                {
                    Debug.Log(report.GetFullSummary());
                }
            }

            dialogText.AppendLine();
            dialogText.Append("Details are in the console.");

            EditorUtility.DisplayDialog(Title, dialogText.ToString(), "OK");
        }
    }
}

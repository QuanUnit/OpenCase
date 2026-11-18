using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Features.RemoteConfigs.Editor.GoogleSheets;
using Shared.Tools.AssetsExtensions;
using Shared.Tools.DebugExtensions;
using UnityEditor;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Exporters.Skins
{
    public class SkinsSheetExporterWindow : EditorWindow
    {
        private const string Title = "Skins Sheet Exporter";
        private const float ButtonHeight = 30f;
        private const float BrowseButtonWidth = 70f;
        private const string KeyInsideProjectMessage = "The service account key must be stored outside the project folder so it is never imported or committed. Move it (for example to your home folder) and choose it again.";

        private const string HelpText =
            "Writes all CS2 weapon skins (ByMykel CSGO-API + market.csgo.com prices) into the Skins page of the sheet.\n" +
            "Existing rows are updated in place, new skins are appended, rows missing in the source are kept.\n\n" +
            "Upload uses the Google Sheets API with a service account:\n" +
            "1. Google Cloud: enable the Google Sheets API, create a service account and download its JSON key.\n" +
            "2. Share the sheet with the service account client_email as Editor.\n" +
            "3. Choose the key file below. Only its path is stored, in EditorPrefs.";

        private readonly ServiceAccountKeyPathValidator _keyPathValidator = new ServiceAccountKeyPathValidator(Directory.GetParent(Application.dataPath).FullName);

        private SkinsSheetExportConfig _config;
        private string _keyPath;
        private bool _isBusy;

        [MenuItem("Tools/Skins Sheet Exporter")]
        public static void ShowWindow()
        {
            GetWindow<SkinsSheetExporterWindow>(Title);
        }

        private void OnEnable()
        {
            _keyPath = EditorPrefs.GetString(GoogleSheetsPrefs.ServiceAccountKeyPathKey, string.Empty);

            if (_config == null)
            {
                _config = AssetDatabaseTools.FindAllAssets<SkinsSheetExportConfig>().FirstOrDefault();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(HelpText, MessageType.Info);
            EditorGUILayout.Space();

            _config = (SkinsSheetExportConfig)EditorGUILayout.ObjectField("Config", _config, typeof(SkinsSheetExportConfig), false);

            DrawKeyPathField();

            EditorGUILayout.Space();

            GUI.enabled = _config != null && _isBusy == false;

            if (GUILayout.Button("Build && Save CSV", GUILayout.Height(ButtonHeight)) == true)
            {
                BuildAndSaveCsv().Forget();
            }

            GUI.enabled = _config != null && _isBusy == false && IsKeyUsable() == true;

            if (GUILayout.Button("Build && Upload", GUILayout.Height(ButtonHeight)) == true)
            {
                BuildAndUpload().Forget();
            }

            GUI.enabled = true;
        }

        private void DrawKeyPathField()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Service account key (JSON)", _keyPath);

            if (GUILayout.Button("Browse", GUILayout.Width(BrowseButtonWidth)) == true)
            {
                SelectKeyFile();
            }

            EditorGUILayout.EndHorizontal();

            if (string.IsNullOrEmpty(_keyPath) == false && File.Exists(_keyPath) == false)
            {
                EditorGUILayout.HelpBox("Key file not found", MessageType.Warning);
            }
        }

        private void SelectKeyFile()
        {
            var directory = string.IsNullOrEmpty(_keyPath) == true ? string.Empty : Path.GetDirectoryName(_keyPath);
            var path = EditorUtility.OpenFilePanel("Service account key (JSON)", directory, "json");

            if (string.IsNullOrEmpty(path) == true)
            {
                return;
            }

            if (_keyPathValidator.IsInsideProject(path) == true)
            {
                EditorUtility.DisplayDialog(Title, KeyInsideProjectMessage, "OK");
                return;
            }

            _keyPath = path;
            EditorPrefs.SetString(GoogleSheetsPrefs.ServiceAccountKeyPathKey, _keyPath);
        }

        private bool IsKeyUsable()
        {
            return string.IsNullOrEmpty(_keyPath) == false && File.Exists(_keyPath) == true && _keyPathValidator.IsInsideProject(_keyPath) == false;
        }

        private async UniTask BuildAndSaveCsv()
        {
            var path = EditorUtility.SaveFilePanel("Save skins sheet CSV", string.Empty, "skins.csv", "csv");

            if (string.IsNullOrEmpty(path) == true)
            {
                return;
            }

            _isBusy = true;

            try
            {
                var exporter = new SkinsSheetExporter(_config);
                SkinsSheetExportResult result;

                if (IsKeyUsable() == true)
                {
                    var client = CreateClient();
                    var connection = await ConnectAndRead(client, exporter);

                    result = await exporter.Build(ProgressChangedHandle, connection.ExistingValues);
                }
                else
                {
                    result = await exporter.BuildFromPublicCsv(ProgressChangedHandle);
                }

                LogResult(result);

                File.WriteAllText(path, result.Csv);
                DebugColor.Log($"Skins sheet CSV saved: {path}", Color.green);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                FinishOperation();
            }
        }

        private async UniTask BuildAndUpload()
        {
            _isBusy = true;

            try
            {
                var exporter = new SkinsSheetExporter(_config);
                var client = CreateClient();
                var connection = await ConnectAndRead(client, exporter);

                var result = await exporter.Build(ProgressChangedHandle, connection.ExistingValues);
                LogResult(result);

                EditorUtility.DisplayProgressBar(Title, "Writing to the sheet", 1f);
                var writtenRows = await client.WriteValues(connection.Session, result.Values, result.BooleanColumns);

                DebugColor.Log($"Skins sheet uploaded: {writtenRows} rows written", Color.green);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                FinishOperation();
            }
        }

        private GoogleSheetsClient CreateClient()
        {
            var sender = new UnityWebRequestSender(_config.RequestTimeoutSeconds);
            return new GoogleSheetsClient(sender, _config.GetGoogleSheetsClientSettings(), _keyPathValidator);
        }

        private async UniTask<SheetConnection> ConnectAndRead(GoogleSheetsClient client, SkinsSheetExporter exporter)
        {
            EditorUtility.DisplayProgressBar(Title, "Reading the current sheet", 0f);

            var session = await client.Connect(_keyPath, _config.SheetId, _config.PageId);
            var existingValues = await client.ReadValues(session, exporter.ColumnsCount);

            return new SheetConnection(session, existingValues);
        }

        private void FinishOperation()
        {
            EditorUtility.ClearProgressBar();
            _isBusy = false;
            Repaint();
        }

        private void ProgressChangedHandle(string stage, float progress)
        {
            EditorUtility.DisplayProgressBar(Title, stage, progress);
        }

        private class SheetConnection
        {
            public readonly GoogleSheetSession Session;
            public readonly List<List<string>> ExistingValues;

            public SheetConnection(GoogleSheetSession session, List<List<string>> existingValues)
            {
                Session = session;
                ExistingValues = existingValues;
            }
        }

        private void LogResult(SkinsSheetExportResult result)
        {
            Debug.Log(result.GetSummary());

            foreach (var skinName in result.BuildResult.SkippedWithoutPrice)
            {
                Debug.LogWarning($"Skin skipped, no price found: {skinName}");
            }

            foreach (var skinName in result.BuildResult.SkippedUnknownRarity)
            {
                Debug.LogWarning($"Skin skipped, unknown rarity: {skinName}");
            }
        }
    }
}

using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Shared.Tools.AssetsExtensions;
using UnityEditor;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Icons
{
    public class SkinIconsWindow : EditorWindow
    {
        private const string Title = "Skin Icons";
        private const float ButtonHeight = 30f;
        private const string DryRunPrefsKey = "OpenCase.SkinIcons.DryRun";

        private const string HelpText =
            "Downloads skin icons from the CSGO-API source (Steam CDN) into the weapon folders and assigns them to SkinConfigs.\n" +
            "Only missing: configs without a valid icon.\n" +
            "Repair mismatches: also configs whose icon file is named after another skin or is shared with another config.\n" +
            "Replace all: every config is pointed to its own <Id>.png.\n" +
            "Existing files are never overwritten or deleted: an existing <Id>.png is reused.";

        private SkinIconsConfig _config;
        private SkinIconsMode _mode = SkinIconsMode.OnlyMissing;
        private bool _isDryRun;
        private bool _isBusy;
        private string _summary = string.Empty;
        private Vector2 _summaryScroll;

        [MenuItem("Tools/Skin Icons")]
        public static void ShowWindow()
        {
            GetWindow<SkinIconsWindow>(Title);
        }

        private void OnEnable()
        {
            _isDryRun = EditorPrefs.GetBool(DryRunPrefsKey, true);

            if (_config == null)
            {
                _config = AssetDatabaseTools.FindAllAssets<SkinIconsConfig>().FirstOrDefault();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(HelpText, MessageType.Info);
            EditorGUILayout.Space();

            _config = (SkinIconsConfig)EditorGUILayout.ObjectField("Config", _config, typeof(SkinIconsConfig), false);
            _mode = (SkinIconsMode)EditorGUILayout.EnumPopup("Mode", _mode);
            var isDryRun = EditorGUILayout.Toggle("Dry run", _isDryRun);

            if (isDryRun != _isDryRun)
            {
                _isDryRun = isDryRun;
                EditorPrefs.SetBool(DryRunPrefsKey, _isDryRun);
            }

            EditorGUILayout.Space();

            GUI.enabled = _config != null && _isBusy == false;

            if (GUILayout.Button("Run", GUILayout.Height(ButtonHeight)) == true)
            {
                RunTool().Forget();
            }

            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Summary", EditorStyles.boldLabel);

            _summaryScroll = EditorGUILayout.BeginScrollView(_summaryScroll);
            EditorGUILayout.TextArea(_summary, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private async UniTask RunTool()
        {
            _isBusy = true;

            try
            {
                var runner = new SkinIconsRunner(_config);
                var preparation = await runner.Prepare(_mode, ProgressChangedHandle);
                var plannedReport = runner.CreateDryRunReport(preparation);

                EditorUtility.ClearProgressBar();

                if (_isDryRun == true)
                {
                    ShowReport(runner, plannedReport);
                    return;
                }

                if (preparation.Plan.Items.Count == 0)
                {
                    ShowReport(runner, plannedReport);
                    EditorUtility.DisplayDialog(Title, "Nothing to change.", "OK");
                    return;
                }

                var isConfirmed = EditorUtility.DisplayDialog(Title, $"{plannedReport.GetShortSummary()}\n\nApply these changes?", "Apply", "Cancel");

                if (isConfirmed == false)
                {
                    _summary = "Cancelled before any change.";
                    return;
                }

                var report = await runner.Apply(preparation, ProgressChangedHandle);
                ShowReport(runner, report);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                _summary = exception.Message;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                _isBusy = false;
                Repaint();
            }
        }

        private void ShowReport(SkinIconsRunner runner, SkinIconsReport report)
        {
            _summary = report.GetFullSummary();
            runner.Log(report);
        }

        private bool ProgressChangedHandle(string stage, float progress)
        {
            return EditorUtility.DisplayCancelableProgressBar(Title, stage, progress);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Features.Cases.Configs;
using Features.Skins.Configs;
using QCore.Tools.Collections;
using Shared.Tools.AssetsExtensions;
using Shared.Tools.DebugExtensions;
using UnityEditor;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class CaseEditorWindow : EditorWindow
    {
        private const string Title = "Case Editor";
        private const float ListWidth = 250f;
        private const float StatsWidth = 360f;
        private const float IconPreviewSize = 64f;
        private const float TableHeightShare = 0.42f;
        private const float PickerHeightShare = 0.28f;
        private const float MinListHeight = 120f;
        private const float ListReservedHeight = 230f;
        private const float ToolbarButtonWidth = 150f;
        private const string WithoutCategoryTitle = "Without category";
        private const string NoCategoryOption = "(none)";
        private const float RemoveButtonWidth = 22f;

        private CaseEditorConfig _config;
        private CaseConfigEditing _editing;
        private SkinCatalog _catalog;
        private CaseStatsBuilder _statsBuilder;
        private CaseSkinsTableView _tableView;
        private CaseSkinPickerView _pickerView;
        private CaseStatsView _statsView;
        private CasesSheetPusher _pusher;

        private CaseConfig _selectedCase;
        private int _selectedCategoryIndex = -1;
        private Vector2 _listScroll;
        private Vector2 _statsScroll;
        private float _targetReturnToPlayer;
        private int _simulationOpens;
        private CaseSimulationResult _simulationResult;
        private CaseConfig _simulatedCase;
        private string _message = string.Empty;
        private MessageType _messageType = MessageType.None;
        private bool _isBusy;

        [MenuItem("Tools/Case Editor")]
        public static void ShowWindow()
        {
            GetWindow<CaseEditorWindow>(Title);
        }

        public void Select(CaseConfig caseConfig)
        {
            _selectedCase = caseConfig;
            _selectedCategoryIndex = _editing.FindCategoryIndex(caseConfig);
            _simulationResult = null;
            Repaint();
        }

        private void OnEnable()
        {
            if (_config == null)
            {
                _config = AssetDatabaseTools.FindAllAssets<CaseEditorConfig>().FirstOrDefault();
            }

            Undo.undoRedoPerformed += UndoRedoPerformedHandle;
            EditorApplication.projectChanged += ProjectChangedHandle;

            Initialize();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformedHandle;
            EditorApplication.projectChanged -= ProjectChangedHandle;
        }

        private void Initialize()
        {
            if (_config == null)
            {
                return;
            }

            _editing = new CaseConfigEditing(_config);
            _catalog = new SkinCatalog(_config.SkinsGlobalSettingsConfig, _config.ValueSamplesCount);
            _catalog.Reload();
            _statsBuilder = new CaseStatsBuilder(_catalog);
            _tableView = new CaseSkinsTableView(_config, _catalog);
            _pickerView = new CaseSkinPickerView(_config, _catalog);
            _statsView = new CaseStatsView(_config, _catalog);
            _pusher = new CasesSheetPusher(_config);
            _targetReturnToPlayer = _config.DefaultTargetReturnToPlayer;
            _simulationOpens = _config.DefaultSimulationOpens;
        }

        private void UndoRedoPerformedHandle()
        {
            if (_editing == null)
            {
                return;
            }

            _editing.InvalidateCases();
            _editing.SaveAllDirty();
            Repaint();
        }

        private void ProjectChangedHandle()
        {
            if (_catalog == null)
            {
                return;
            }

            _catalog.Reload();
            _editing.InvalidateCases();
            _pickerView.MarkDirty();
            Repaint();
        }

        private void OnGUI()
        {
            if (_config == null || _editing == null)
            {
                DrawMissingConfig();
                return;
            }

            var configErrors = _config.GetValidationErrors();

            if (configErrors.Count > 0)
            {
                EditorGUILayout.HelpBox($"{nameof(CaseEditorConfig)} is misconfigured:\n{string.Join("\n", configErrors)}", MessageType.Error);
                EditorGUILayout.ObjectField("Config", _config, typeof(CaseEditorConfig), false);
                return;
            }

            DrawToolbar();

            if (string.IsNullOrEmpty(_message) == false)
            {
                EditorGUILayout.HelpBox(_message, _messageType);
            }

            EditorGUILayout.BeginHorizontal();

            DrawCasesList();

            if (_selectedCase != null)
            {
                DrawSelectedCase();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a case on the left", MessageType.Info);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawMissingConfig()
        {
            EditorGUILayout.HelpBox($"Create a {nameof(CaseEditorConfig)} asset (Configs/Editor/{nameof(CaseEditorConfig)}) and assign it here", MessageType.Warning);

            var config = (CaseEditorConfig)EditorGUILayout.ObjectField("Config", _config, typeof(CaseEditorConfig), false);

            if (config != _config)
            {
                _config = config;
                Initialize();
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUI.enabled = _isBusy == false && _pusher.IsKeyUsable() == true;

            if (GUILayout.Button("Push cases to sheet", EditorStyles.toolbarButton, GUILayout.Width(ToolbarButtonWidth)) == true)
            {
                PushToSheet().Forget();
            }

            GUI.enabled = _isBusy == false;

            if (GUILayout.Button("Pull from sheet", EditorStyles.toolbarButton, GUILayout.Width(ToolbarButtonWidth)) == true)
            {
                PullFromSheet().Forget();
            }

            if (GUILayout.Button("Reload skins", EditorStyles.toolbarButton, GUILayout.Width(ToolbarButtonWidth)) == true)
            {
                _catalog.Reload();
                _pickerView.MarkDirty();
            }

            GUI.enabled = true;
            GUILayout.FlexibleSpace();

            var keyStatus = _pusher.IsKeyUsable() == true ? "Service account key: set" : "Service account key: not set (Tools/Skins Sheet Exporter)";
            GUILayout.Label(keyStatus, EditorStyles.miniLabel);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawCasesList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(ListWidth));

            _listScroll = EditorGUILayout.BeginScrollView(_listScroll, GUILayout.Height(Mathf.Max(MinListHeight, position.height - ListReservedHeight)));

            var categories = _editing.Categories;

            for (var categoryIndex = 0; categoryIndex < categories.Count; categoryIndex++)
            {
                DrawCategory(categoryIndex, categories[categoryIndex].Key.Name, categories[categoryIndex].Value);
            }

            var withoutCategory = _editing.GetCasesWithoutCategory();

            if (withoutCategory.Count > 0)
            {
                DrawCategory(-1, WithoutCategoryTitle, withoutCategory);
            }

            EditorGUILayout.EndScrollView();

            DrawListButtons();

            EditorGUILayout.EndVertical();
        }

        private void DrawCategory(int categoryIndex, string name, IReadOnlyList<CaseConfig> cases)
        {
            EditorGUILayout.Space();

            var isSelectedCategory = categoryIndex >= 0 && categoryIndex == _selectedCategoryIndex && _selectedCase == null;
            var headerStyle = isSelectedCategory == true ? EditorStyles.boldLabel : EditorStyles.miniBoldLabel;

            if (GUILayout.Button($"{name} ({cases.Count})", headerStyle) == true && categoryIndex >= 0)
            {
                _selectedCategoryIndex = categoryIndex;
                _selectedCase = null;
            }

            foreach (var caseConfig in cases)
            {
                if (caseConfig == null)
                {
                    DrawMissingCase(categoryIndex);
                    continue;
                }

                var label = $"{caseConfig.Name}  {CaseEditorGuiTools.FormatMoney(caseConfig.Price)}";
                var isSelected = caseConfig == _selectedCase;

                if (GUILayout.Toggle(isSelected, label, EditorStyles.toolbarButton) == true && isSelected == false)
                {
                    Select(caseConfig);
                    GUI.FocusControl(null);
                }
            }
        }

        private void DrawMissingCase(int categoryIndex)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Missing case reference", CaseEditorGuiTools.CreateColoredLabel(EditorStyles.miniLabel, _config.ErrorColor));

            if (GUILayout.Button("x", GUILayout.Width(RemoveButtonWidth)) == true)
            {
                _editing.RemoveMissingCases(categoryIndex);
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawListButtons()
        {
            if (GUILayout.Button("New case") == true)
            {
                var categoryIndex = _selectedCase != null ? _editing.FindCategoryIndex(_selectedCase) : _selectedCategoryIndex;

                if (categoryIndex < 0 && _editing.Categories.Count > 0)
                {
                    categoryIndex = 0;
                }

                Select(_editing.CreateCase(categoryIndex));
            }

            GUI.enabled = _selectedCase != null;

            if (GUILayout.Button("Duplicate") == true)
            {
                Select(_editing.DuplicateCase(_selectedCase));
            }

            GUI.enabled = _selectedCase != null && _editing.FindCategoryIndex(_selectedCase) >= 0;

            if (GUILayout.Button("Remove from category") == true)
            {
                _editing.RemoveFromCategory(_selectedCase);
                _selectedCategoryIndex = -1;
            }

            GUI.enabled = true;

            EditorGUILayout.Space();

            if (GUILayout.Button("Add category") == true)
            {
                _editing.AddCategory(_config.NewCategoryName);
                _selectedCategoryIndex = _editing.Categories.Count - 1;
                _selectedCase = null;
            }

            if (_selectedCase == null && _selectedCategoryIndex >= 0 && _selectedCategoryIndex < _editing.Categories.Count)
            {
                var category = _editing.Categories[_selectedCategoryIndex];
                var name = EditorGUILayout.DelayedTextField("Category name", category.Key.Name);

                if (name != category.Key.Name && string.IsNullOrWhiteSpace(name) == false)
                {
                    _editing.RenameCategory(_selectedCategoryIndex, name.Trim());
                }

                GUI.enabled = category.Value.Count == 0;

                if (GUILayout.Button("Remove empty category") == true)
                {
                    _editing.RemoveEmptyCategory(_selectedCategoryIndex);
                    _selectedCategoryIndex = -1;
                }

                GUI.enabled = true;
            }
        }

        private void DrawSelectedCase()
        {
            var entries = _editing.GetEntries(_selectedCase);
            var validEntries = entries.Where(entry => entry.Key != null).ToList();
            var stats = _statsBuilder.Calculate(validEntries, _selectedCase.Price, _config.GetStatsSettings());

            EditorGUILayout.BeginVertical();

            DrawProperties();

            if (validEntries.Count < entries.Count)
            {
                EditorGUILayout.HelpBox($"{entries.Count - validEntries.Count} entries reference missing skins, they are dropped on the next edit", MessageType.Warning);
            }

            var tableEdit = _tableView.Draw(_selectedCase, validEntries, stats, position.height * TableHeightShare);

            if (tableEdit != null)
            {
                ApplyTableEdit(tableEdit);
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.Space();

            var added = _pickerView.Draw(validEntries.Select(entry => entry.Key).ToList(), position.height * PickerHeightShare);

            if (added != null && added.Count > 0)
            {
                AddSkins(validEntries, added);
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(StatsWidth));
            _statsScroll = EditorGUILayout.BeginScrollView(_statsScroll);

            DrawHelpers(validEntries, stats);
            EditorGUILayout.Space();
            _statsView.Draw(stats, validEntries.Select(entry => entry.Key).ToList(), _simulatedCase == _selectedCase ? _simulationResult : null);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawProperties()
        {
            EditorGUILayout.BeginHorizontal();

            var icon = _editing.GetIcon(_selectedCase);
            var iconRect = GUILayoutUtility.GetRect(IconPreviewSize, IconPreviewSize, GUILayout.Width(IconPreviewSize));
            CaseEditorGuiTools.DrawSprite(iconRect, icon);

            EditorGUILayout.BeginVertical();

            var id = EditorGUILayout.DelayedTextField("Id", _selectedCase.Id.Value);

            if (id != _selectedCase.Id.Value)
            {
                ChangeId(id.Trim());
            }

            var name = EditorGUILayout.DelayedTextField("Name", _selectedCase.Name);

            if (name != _selectedCase.Name)
            {
                _editing.SetName(_selectedCase, name);
            }

            var price = EditorGUILayout.DelayedFloatField("Price", _selectedCase.Price);

            if (Mathf.Approximately(price, _selectedCase.Price) == false)
            {
                _editing.SetPrice(_selectedCase, Mathf.Max(0f, price));
            }

            var newIcon = (Sprite)EditorGUILayout.ObjectField("Icon", icon, typeof(Sprite), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));

            if (newIcon != icon && newIcon == null)
            {
                ShowMessage("A case must have an icon: the previous icon was kept", MessageType.Warning);
            }
            else if (newIcon != icon)
            {
                ChangeIcon(newIcon);
            }

            DrawCategoryPopup();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            if (icon == null)
            {
                EditorGUILayout.HelpBox("The case has no icon", MessageType.Error);
            }
            else if (_editing.IsIconAddressable(_selectedCase) == false)
            {
                EditorGUILayout.HelpBox("The icon is not addressable, reassign it to register it", MessageType.Error);
            }
        }

        private void DrawCategoryPopup()
        {
            var categories = _editing.Categories;
            var options = new[] { NoCategoryOption }.Concat(categories.Select(category => category.Key.Name)).ToArray();
            var currentIndex = _editing.FindCategoryIndex(_selectedCase);
            var newIndex = EditorGUILayout.Popup("Category", currentIndex + 1, options) - 1;

            if (newIndex != currentIndex)
            {
                _editing.MoveToCategory(_selectedCase, newIndex);
                _selectedCategoryIndex = newIndex;
            }
        }

        private void DrawHelpers(List<KeyValue<SkinConfig, float>> entries, CaseStats stats)
        {
            EditorGUILayout.LabelField("Helpers", EditorStyles.boldLabel);

            GUI.enabled = entries.Count > 0;

            if (GUILayout.Button("Apply rarity odds") == true)
            {
                ApplyRarityOdds(entries);
            }

            EditorGUILayout.BeginHorizontal();
            _targetReturnToPlayer = EditorGUILayout.Slider("Target RTP", _targetReturnToPlayer, _config.MinReturnToPlayer, _config.MaxReturnToPlayer);

            GUI.enabled = stats.IsValid == true;

            if (GUILayout.Button("Set price") == true)
            {
                SetPriceForTarget(stats);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _simulationOpens = Mathf.Max(1, EditorGUILayout.IntField("Monte-Carlo opens", _simulationOpens));

            if (GUILayout.Button("Run") == true)
            {
                _simulationResult = new CaseMonteCarloSimulator(_config.SkinsGlobalSettingsConfig).Run(_selectedCase, _simulationOpens);
                _simulatedCase = _selectedCase;
            }

            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        private void SetPriceForTarget(CaseStats stats)
        {
            try
            {
                var price = new CasePriceCalculator(_config.PriceStep).GetPriceForReturnToPlayer(stats.ExpectedValue, _targetReturnToPlayer);
                _editing.SetPrice(_selectedCase, price);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                ShowMessage($"Price not set: {exception.Message}", MessageType.Error);
            }
        }

        private void ChangeId(string id)
        {
            var error = _editing.ValidateId(_selectedCase, id);

            if (error != null)
            {
                ShowMessage(error, MessageType.Error);
                return;
            }

            _editing.SetId(_selectedCase, id);
            ShowMessage(string.Empty, MessageType.None);
        }

        private void ChangeIcon(Sprite sprite)
        {
            try
            {
                _editing.SetIcon(_selectedCase, sprite);
            }
            catch (InvalidOperationException exception)
            {
                ShowMessage(exception.Message, MessageType.Error);
            }
        }

        private void ApplyTableEdit(CaseSkinsTableEdit edit)
        {
            var entries = _editing.GetEntries(edit.Owner).Where(entry => entry.Key != null).ToList();
            var index = entries.FindIndex(entry => entry.Key == edit.Skin);

            if (index < 0)
            {
                return;
            }

            if (edit.IsRemoval == true)
            {
                entries.RemoveAt(index);
                _editing.SetEntries(edit.Owner, entries, "Remove skin from case");
                return;
            }

            if (Mathf.Approximately(entries[index].Value, edit.Weight) == true)
            {
                return;
            }

            entries[index] = new KeyValue<SkinConfig, float>(edit.Skin, edit.Weight);
            _editing.SetEntries(edit.Owner, entries, "Change skin weight");
        }

        private void AddSkins(List<KeyValue<SkinConfig, float>> entries, List<SkinConfig> skins)
        {
            var existing = new HashSet<SkinConfig>(entries.Select(entry => entry.Key));

            foreach (var skin in skins)
            {
                if (existing.Add(skin) == true)
                {
                    entries.Add(new KeyValue<SkinConfig, float>(skin, _config.DefaultSkinWeight));
                }
            }

            _editing.SetEntries(_selectedCase, entries, "Add skins to case");
        }

        private void ApplyRarityOdds(List<KeyValue<SkinConfig, float>> entries)
        {
            var weighting = _config.CreateRarityOddsWeighting();
            var result = weighting.Calculate(CaseEditorGuiTools.GetRarities(entries));

            for (var index = 0; index < entries.Count; index++)
            {
                entries[index] = new KeyValue<SkinConfig, float>(entries[index].Key, result.Weights[index]);
            }

            _editing.SetEntries(_selectedCase, entries, "Apply rarity odds");

            if (result.RaritiesWithoutOdds.Count > 0)
            {
                ShowMessage($"No odds configured for {string.Join(", ", result.RaritiesWithoutOdds)}: their weights were set to 0", MessageType.Warning);
            }
        }

        private async UniTask PushToSheet()
        {
            _isBusy = true;

            try
            {
                var cases = _editing.GetCategorizedCasesInOrder();
                EditorUtility.DisplayProgressBar(Title, "Pushing cases to the sheet", 0.5f);

                var writtenRows = await _pusher.Push(cases, ConfirmUnknownIds);

                if (writtenRows > 0)
                {
                    ShowMessage($"Pushed {cases.Count} cases ({writtenRows} rows) to the {_config.CasesPageName} page", MessageType.Info);
                    DebugColor.Log($"Cases pushed to sheet: {cases.Count} cases, {writtenRows} rows", Color.green);
                }
                else
                {
                    ShowMessage("Push cancelled", MessageType.Warning);
                }
            }
            catch (Exception exception)
            {
                ShowMessage($"Push failed: {exception.Message}", MessageType.Error);
                Debug.LogException(exception);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                _isBusy = false;
                Repaint();
            }
        }

        private bool ConfirmUnknownIds(IReadOnlyList<string> unknownIds)
        {
            EditorUtility.ClearProgressBar();

            var text = $"The {_config.CasesPageName} page has cases that are not in {nameof(AllCasesConfig)}:\n{string.Join("\n", unknownIds)}\n\nPushing removes them from the sheet.";

            return EditorUtility.DisplayDialog(Title, text, "Push anyway", "Cancel");
        }

        private async UniTask PullFromSheet()
        {
            if (EditorUtility.DisplayDialog(Title, $"Import the {_config.CasesPageName} page from the sheet? Local case edits that were not pushed are overwritten.", "Pull", "Cancel") == false)
            {
                return;
            }

            _isBusy = true;

            try
            {
                EditorUtility.DisplayProgressBar(Title, "Pulling cases from the sheet", 0.5f);

                var pages = new List<EditorRemoteConfigsData.Page> { _config.GetCasesPage() };
                var reports = await new RemoteConfigsImporter().Import(_config.RemoteConfigsData.SheetId, pages, PageStartedHandle);
                var summary = new StringBuilder();

                foreach (var report in reports)
                {
                    summary.AppendLine(report.GetShortSummary());
                    Debug.Log(report.GetFullSummary());
                }

                var hasErrors = reports.Any(report => report.Errors.Count > 0);
                ShowMessage(summary.ToString().Trim(), hasErrors == true ? MessageType.Error : MessageType.Info);
            }
            catch (Exception exception)
            {
                ShowMessage($"Pull failed: {exception.Message}", MessageType.Error);
                Debug.LogException(exception);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                _isBusy = false;
                _pickerView.MarkDirty();
                Repaint();
            }
        }

        private void PageStartedHandle(string pageName, int index)
        {
            EditorUtility.DisplayProgressBar(Title, $"Importing page {pageName}", 0.5f);
        }

        private void ShowMessage(string message, MessageType messageType)
        {
            _message = message;
            _messageType = messageType;
        }
    }
}

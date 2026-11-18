using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Features.Cases.Configs;
using Features.Skins.Configs;
using QCore.Tools.Collections;
using UnityEditor;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class CaseSkinsTableView
    {
        private const float RowHeight = 34f;
        private const float IconWidth = 44f;
        private const float RarityWidth = 100f;
        private const float PriceWidth = 130f;
        private const float WeightWidth = 70f;
        private const float ChanceWidth = 70f;
        private const float ContributionWidth = 80f;
        private const float RemoveWidth = 22f;
        private const float Padding = 4f;
        private const float MinNameWidth = 120f;
        private const float SortLabelWidth = 30f;
        private const float SortPopupWidth = 100f;
        private const float DescendingToggleWidth = 45f;
        private const string WeightControlPrefix = "CaseSkinWeight_";
        private const string WeightFormat = "0.#####";

        private readonly CaseEditorConfig _config;
        private readonly SkinCatalog _catalog;

        private CaseSkinsSortMode _sortMode = CaseSkinsSortMode.CaseOrder;
        private bool _isDescending = true;
        private Vector2 _scroll;
        private CaseConfig _owner;
        private CaseConfig _editingOwner;
        private SkinConfig _editingSkin;
        private string _editingText;
        private CaseSkinsTableEdit _pendingEdit;

        public CaseSkinsTableView(CaseEditorConfig config, SkinCatalog catalog)
        {
            _config = config;
            _catalog = catalog;
        }

        public CaseSkinsTableEdit Draw(CaseConfig owner, IReadOnlyList<KeyValue<SkinConfig, float>> entries, CaseStats stats, float height)
        {
            CaseSkinsTableEdit edit = null;
            _owner = owner;

            HandleCommitKeys();

            if (Event.current.type == EventType.Layout)
            {
                edit = CommitIfFocusLeft();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Skins ({entries.Count})", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Sort", GUILayout.Width(SortLabelWidth));
            var sortMode = (CaseSkinsSortMode)EditorGUILayout.EnumPopup(_sortMode, GUILayout.Width(SortPopupWidth));
            var isDescending = GUILayout.Toggle(_isDescending, "Desc", EditorStyles.miniButton, GUILayout.Width(DescendingToggleWidth));
            EditorGUILayout.EndHorizontal();

            if (sortMode != _sortMode || isDescending != _isDescending)
            {
                _sortMode = sortMode;
                _isDescending = isDescending;
                ReleaseFocus();
            }

            DrawHeader();

            var order = GetOrder(entries, stats);
            var previousScroll = _scroll;

            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(height));

            if (_scroll != previousScroll)
            {
                ReleaseFocus();
            }

            var firstVisible = Mathf.Max(0, Mathf.FloorToInt(_scroll.y / RowHeight));
            var visibleCount = Mathf.CeilToInt(height / RowHeight) + 1;
            var lastVisible = Mathf.Min(order.Count, firstVisible + visibleCount);

            GUILayout.Space(firstVisible * RowHeight);

            for (var row = firstVisible; row < lastVisible; row++)
            {
                var rowEdit = DrawRow(entries, stats, order[row]);

                if (rowEdit != null)
                {
                    edit = rowEdit;
                }
            }

            GUILayout.Space((order.Count - lastVisible) * RowHeight);

            EditorGUILayout.EndScrollView();

            if (edit == null && _pendingEdit != null)
            {
                edit = _pendingEdit;
                _pendingEdit = null;
            }

            return edit;
        }

        private void HandleCommitKeys()
        {
            var currentEvent = Event.current;

            if (_editingSkin == null || currentEvent.type != EventType.KeyDown)
            {
                return;
            }

            if (currentEvent.keyCode == KeyCode.Return || currentEvent.keyCode == KeyCode.KeypadEnter)
            {
                ReleaseFocus();
                currentEvent.Use();
            }
        }

        private CaseSkinsTableEdit CommitIfFocusLeft()
        {
            if (_editingSkin == null || GUI.GetNameOfFocusedControl() == GetControlName(_editingSkin))
            {
                return null;
            }

            var edit = CreateWeightEdit(_editingOwner, _editingSkin, _editingText);
            _editingSkin = null;
            _editingText = null;

            return edit;
        }

        private CaseSkinsTableEdit CreateWeightEdit(CaseConfig owner, SkinConfig skin, string text)
        {
            var normalized = text.Trim().Replace(',', '.');

            if (float.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var weight) == false)
            {
                return null;
            }

            return new CaseSkinsTableEdit(owner, skin, false, Mathf.Max(0f, weight));
        }

        private void ReleaseFocus()
        {
            if (_editingSkin == null)
            {
                return;
            }

            GUI.FocusControl(null);
            EditorGUIUtility.editingTextField = false;
        }

        private string GetControlName(SkinConfig skin)
        {
            return WeightControlPrefix + skin.SkinStringId;
        }

        private void DrawHeader()
        {
            var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            var columns = GetColumns(rect);

            EditorGUI.LabelField(columns.Name, "Skin", EditorStyles.miniBoldLabel);
            EditorGUI.LabelField(columns.Rarity, "Rarity", EditorStyles.miniBoldLabel);
            EditorGUI.LabelField(columns.Price, "Price range", EditorStyles.miniBoldLabel);
            EditorGUI.LabelField(columns.Weight, "Weight", EditorStyles.miniBoldLabel);
            EditorGUI.LabelField(columns.Chance, "Chance", EditorStyles.miniBoldLabel);
            EditorGUI.LabelField(columns.Contribution, "EV part", EditorStyles.miniBoldLabel);
        }

        private CaseSkinsTableEdit DrawRow(IReadOnlyList<KeyValue<SkinConfig, float>> entries, CaseStats stats, int index)
        {
            var entry = entries[index];
            var skin = entry.Key;
            var info = _catalog.GetInfo(skin);
            var rect = EditorGUILayout.GetControlRect(false, RowHeight - EditorGUIUtility.standardVerticalSpacing);
            var columns = GetColumns(rect);
            var lineHeight = EditorGUIUtility.singleLineHeight;

            CaseEditorGuiTools.DrawSprite(columns.Icon, _catalog.GetIcon(skin));

            var nameRect = new Rect(columns.Name.x, columns.Name.y, columns.Name.width, lineHeight);
            var weaponRect = new Rect(columns.Name.x, columns.Name.y + lineHeight, columns.Name.width, lineHeight);
            EditorGUI.LabelField(nameRect, skin.Name);
            EditorGUI.LabelField(weaponRect, info.Weapon, EditorStyles.miniLabel);

            var rarityStyle = CaseEditorGuiTools.CreateColoredLabel(EditorStyles.label, CaseEditorGuiTools.GetRarityColor(_config.SkinsUIContextDataConfig, skin.Rarity));
            EditorGUI.LabelField(Center(columns.Rarity), skin.Rarity.ToString(), rarityStyle);
            EditorGUI.LabelField(Center(columns.Price), CaseEditorGuiTools.FormatPriceRange(skin.Price));

            DrawWeightField(Center(columns.Weight), skin, entry.Value);

            if (stats.IsValid == true && index < stats.Entries.Count)
            {
                EditorGUI.LabelField(Center(columns.Chance), CaseEditorGuiTools.FormatPercent(stats.Entries[index].DropChance));
                EditorGUI.LabelField(Center(columns.Contribution), CaseEditorGuiTools.FormatMoney(stats.Entries[index].ExpectedValueContribution));
            }

            if (GUI.Button(Center(columns.Remove), "x") == true)
            {
                return new CaseSkinsTableEdit(_owner, skin, true, 0f);
            }

            return null;
        }

        private void DrawWeightField(Rect rect, SkinConfig skin, float weight)
        {
            var controlName = GetControlName(skin);
            var isEditing = _editingSkin == skin && _editingOwner == _owner;
            var shownText = isEditing == true ? _editingText : weight.ToString(WeightFormat, CultureInfo.InvariantCulture);

            GUI.SetNextControlName(controlName);
            var text = EditorGUI.TextField(rect, shownText);

            if (GUI.GetNameOfFocusedControl() != controlName)
            {
                return;
            }

            if (_editingSkin != null && (_editingSkin != skin || _editingOwner != _owner))
            {
                _pendingEdit = CreateWeightEdit(_editingOwner, _editingSkin, _editingText);
            }

            _editingOwner = _owner;
            _editingSkin = skin;
            _editingText = text;
        }

        private List<int> GetOrder(IReadOnlyList<KeyValue<SkinConfig, float>> entries, CaseStats stats)
        {
            var indexes = Enumerable.Range(0, entries.Count);

            switch (_sortMode)
            {
                case CaseSkinsSortMode.Rarity:
                    indexes = Sort(indexes, index => (double)entries[index].Key.Rarity);
                    break;
                case CaseSkinsSortMode.Price:
                    indexes = Sort(indexes, index => _catalog.GetDistribution(entries[index].Key).Mean);
                    break;
                case CaseSkinsSortMode.Chance:
                    indexes = Sort(indexes, index => (double)entries[index].Value);
                    break;
            }

            return indexes.ToList();
        }

        private IEnumerable<int> Sort(IEnumerable<int> indexes, Func<int, double> key)
        {
            if (_isDescending == true)
            {
                return indexes.OrderByDescending(key).ThenBy(index => index);
            }

            return indexes.OrderBy(key).ThenBy(index => index);
        }

        private Rect Center(Rect rect)
        {
            var lineHeight = EditorGUIUtility.singleLineHeight;
            return new Rect(rect.x, rect.y + (rect.height - lineHeight) * 0.5f, rect.width, lineHeight);
        }

        private Columns GetColumns(Rect rect)
        {
            var fixedWidth = IconWidth + RarityWidth + PriceWidth + WeightWidth + ChanceWidth + ContributionWidth + RemoveWidth + Padding * 7f;
            var nameWidth = Mathf.Max(MinNameWidth, rect.width - fixedWidth);
            var x = rect.x;

            var columns = new Columns();
            columns.Icon = Take(ref x, rect, IconWidth);
            columns.Name = Take(ref x, rect, nameWidth);
            columns.Rarity = Take(ref x, rect, RarityWidth);
            columns.Price = Take(ref x, rect, PriceWidth);
            columns.Weight = Take(ref x, rect, WeightWidth);
            columns.Chance = Take(ref x, rect, ChanceWidth);
            columns.Contribution = Take(ref x, rect, ContributionWidth);
            columns.Remove = Take(ref x, rect, RemoveWidth);

            return columns;
        }

        private Rect Take(ref float x, Rect rect, float width)
        {
            var result = new Rect(x, rect.y, width, rect.height);
            x += width + Padding;

            return result;
        }

        private struct Columns
        {
            public Rect Icon;
            public Rect Name;
            public Rect Rarity;
            public Rect Price;
            public Rect Weight;
            public Rect Chance;
            public Rect Contribution;
            public Rect Remove;
        }
    }
}

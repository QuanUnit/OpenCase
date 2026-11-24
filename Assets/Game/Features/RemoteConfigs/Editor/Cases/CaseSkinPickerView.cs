using System;
using System.Collections.Generic;
using System.Linq;
using Features.Skins.Configs;
using Features.Skins.Data;
using UnityEditor;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class CaseSkinPickerView
    {
        private const float RowHeight = 26f;
        private const float ToggleWidth = 18f;
        private const float IconWidth = 34f;
        private const float RarityWidth = 100f;
        private const float ValueWidth = 80f;
        private const float Padding = 4f;
        private const string AllOption = "All";
        private const float IconToggleWidth = 110f;
        private const float CountLabelWidth = 180f;

        private readonly CaseEditorConfig _config;
        private readonly SkinCatalog _catalog;
        private readonly HashSet<SkinConfig> _selected = new HashSet<SkinConfig>();
        private readonly List<CaseSkinInfo> _filtered = new List<CaseSkinInfo>();

        private string _search = string.Empty;
        private int _rarityIndex;
        private int _weaponIndex;
        private float _minValue;
        private float _maxValue;
        private bool _isOnlyWithIcon = true;
        private bool _isFilterDirty = true;
        private HashSet<SkinConfig> _caseSkins = new HashSet<SkinConfig>();
        private Vector2 _scroll;

        public CaseSkinPickerView(CaseEditorConfig config, SkinCatalog catalog)
        {
            _config = config;
            _catalog = catalog;
        }

        public void MarkDirty()
        {
            _isFilterDirty = true;
        }

        public List<SkinConfig> Draw(ICollection<SkinConfig> caseSkins, float height)
        {
            List<SkinConfig> added = null;

            EditorGUILayout.LabelField("Add skins", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            _search = EditorGUILayout.TextField("Search", _search);
            _isOnlyWithIcon = EditorGUILayout.ToggleLeft("Only with icon", _isOnlyWithIcon, GUILayout.Width(IconToggleWidth));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            var rarityOptions = new[] { AllOption }.Concat(Enum.GetNames(typeof(Rarity))).ToArray();
            _rarityIndex = EditorGUILayout.Popup("Rarity", _rarityIndex, rarityOptions);
            var weaponOptions = new[] { AllOption }.Concat(_catalog.Weapons).ToArray();
            _weaponIndex = EditorGUILayout.Popup("Weapon", Mathf.Min(_weaponIndex, weaponOptions.Length - 1), weaponOptions);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _minValue = Mathf.Max(0f, EditorGUILayout.FloatField("Mean drop from", _minValue));
            _maxValue = Mathf.Max(0f, EditorGUILayout.FloatField("to (0 = any)", _maxValue));
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck() == true || _caseSkins.SetEquals(caseSkins) == false)
            {
                _isFilterDirty = true;
            }

            if (_isFilterDirty == true)
            {
                _caseSkins = new HashSet<SkinConfig>(caseSkins);
                Filter(weaponOptions);
            }

            var selectedCount = _selected.Count;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{_filtered.Count} found, {selectedCount} selected", GUILayout.Width(CountLabelWidth));

            GUI.enabled = selectedCount > 0;

            if (GUILayout.Button($"Add selected ({selectedCount})") == true)
            {
                added = _selected.Where(skin => _caseSkins.Contains(skin) == false).OrderBy(skin => skin.Id.Value, StringComparer.Ordinal).ToList();
                _selected.Clear();
            }

            if (GUILayout.Button("Clear selection") == true)
            {
                _selected.Clear();
            }

            GUI.enabled = _filtered.Count > 0;

            if (GUILayout.Button($"Add all filtered ({_filtered.Count})") == true)
            {
                added = _filtered.Select(info => info.Config).ToList();
                _selected.Clear();
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            DrawList(height);

            if (added != null)
            {
                _isFilterDirty = true;
            }

            return added;
        }

        private void DrawList(float height)
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(height));

            var firstVisible = Mathf.Max(0, Mathf.FloorToInt(_scroll.y / RowHeight));
            var visibleCount = Mathf.CeilToInt(height / RowHeight) + 1;
            var lastVisible = Mathf.Min(_filtered.Count, firstVisible + visibleCount);

            GUILayout.Space(firstVisible * RowHeight);

            for (var row = firstVisible; row < lastVisible; row++)
            {
                DrawRow(_filtered[row]);
            }

            GUILayout.Space((_filtered.Count - lastVisible) * RowHeight);

            EditorGUILayout.EndScrollView();
        }

        private void DrawRow(CaseSkinInfo info)
        {
            var rect = EditorGUILayout.GetControlRect(false, RowHeight - EditorGUIUtility.standardVerticalSpacing);
            var x = rect.x;
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var centerY = rect.y + (rect.height - lineHeight) * 0.5f;

            var isSelected = _selected.Contains(info.Config);
            var toggled = EditorGUI.Toggle(new Rect(x, centerY, ToggleWidth, lineHeight), isSelected);
            x += ToggleWidth + Padding;

            if (toggled != isSelected)
            {
                if (toggled == true)
                {
                    _selected.Add(info.Config);
                }
                else
                {
                    _selected.Remove(info.Config);
                }
            }

            CaseEditorGuiTools.DrawSprite(new Rect(x, rect.y, IconWidth, rect.height), _catalog.GetIcon(info.Config));
            x += IconWidth + Padding;

            var nameWidth = Mathf.Max(0f, rect.xMax - x - RarityWidth - ValueWidth - Padding * 2f);
            EditorGUI.LabelField(new Rect(x, centerY, nameWidth, lineHeight), info.DisplayName);
            x += nameWidth + Padding;

            var rarityStyle = CaseEditorGuiTools.CreateColoredLabel(EditorStyles.label, CaseEditorGuiTools.GetRarityColor(_config.SkinsUIContextDataConfig, info.Config.Rarity));
            EditorGUI.LabelField(new Rect(x, centerY, RarityWidth, lineHeight), info.Config.Rarity.ToString(), rarityStyle);
            x += RarityWidth + Padding;

            EditorGUI.LabelField(new Rect(x, centerY, ValueWidth, lineHeight), CaseEditorGuiTools.FormatMoney(_catalog.GetDistribution(info.Config).Mean));
        }

        private void Filter(string[] weaponOptions)
        {
            _isFilterDirty = false;
            _filtered.Clear();
            _catalog.RefreshValueModel();

            var search = _search.Trim().ToLowerInvariant();
            var weapon = _weaponIndex > 0 && _weaponIndex < weaponOptions.Length ? weaponOptions[_weaponIndex] : null;

            foreach (var info in _catalog.Skins)
            {
                if (IsMatch(info, search, weapon) == true)
                {
                    _filtered.Add(info);
                }
            }

            _selected.IntersectWith(_filtered.Select(info => info.Config));
        }

        private bool IsMatch(CaseSkinInfo info, string search, string weapon)
        {
            if (_caseSkins.Contains(info.Config) == true)
            {
                return false;
            }

            if (search.Length > 0 && info.SearchText.Contains(search) == false)
            {
                return false;
            }

            if (_rarityIndex > 0 && (int)info.Config.Rarity != _rarityIndex - 1)
            {
                return false;
            }

            if (weapon != null && info.Weapon != weapon)
            {
                return false;
            }

            if (_isOnlyWithIcon == true && _catalog.HasIcon(info.Config) == false)
            {
                return false;
            }

            var mean = _catalog.GetDistribution(info.Config).Mean;

            if (mean < _minValue)
            {
                return false;
            }

            if (_maxValue > 0f && mean > _maxValue)
            {
                return false;
            }

            return true;
        }
    }
}

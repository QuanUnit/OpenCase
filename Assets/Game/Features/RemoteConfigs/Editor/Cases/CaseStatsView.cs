using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Features.Skins.Configs;
using UnityEditor;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class CaseStatsView
    {
        private const float LabelWidth = 190f;
        private const string OpensFormat = "0.#";
        private const double PriceStepTolerance = 1e-3d;

        private readonly CaseEditorConfig _config;
        private readonly SkinCatalog _catalog;

        private GUIStyle _errorStyle;
        private GUIStyle _warningStyle;
        private GUIStyle _goodStyle;

        public CaseStatsView(CaseEditorConfig config, SkinCatalog catalog)
        {
            _config = config;
            _catalog = catalog;
        }

        public void Draw(CaseStats stats, IReadOnlyList<SkinConfig> skins, CaseSimulationResult simulationResult)
        {
            EnsureStyles();

            var previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = LabelWidth;

            EditorGUILayout.LabelField("Live stats", EditorStyles.boldLabel);

            DrawWarnings(stats, skins);

            if (stats.IsValid == false)
            {
                EditorGUIUtility.labelWidth = previousLabelWidth;
                return;
            }

            EditorGUILayout.LabelField("Expected drop value (EV)", CaseEditorGuiTools.FormatMoney(stats.ExpectedValue));
            EditorGUILayout.LabelField("RTP (EV / price)", CaseEditorGuiTools.FormatPercent(stats.ReturnToPlayer), GetReturnToPlayerStyle(stats.ReturnToPlayer));
            EditorGUILayout.LabelField("House edge", CaseEditorGuiTools.FormatPercent(stats.HouseEdge));
            EditorGUILayout.LabelField("Median drop", CaseEditorGuiTools.FormatMoney(stats.Median));
            EditorGUILayout.LabelField("Profit chance (drop >= price)", CaseEditorGuiTools.FormatPercent(stats.ProfitChance));

            foreach (var multipleChance in stats.MultipleChances)
            {
                var label = $"Drop >= {multipleChance.Multiplier.ToString(CultureInfo.InvariantCulture)}x price";
                EditorGUILayout.LabelField(label, CaseEditorGuiTools.FormatPercent(multipleChance.Chance));
            }

            EditorGUILayout.LabelField("Min / max possible drop", $"{CaseEditorGuiTools.FormatMoney(stats.Min)} / {CaseEditorGuiTools.FormatMoney(stats.Max)}");
            EditorGUILayout.LabelField("Standard deviation", CaseEditorGuiTools.FormatMoney(stats.StandardDeviation));

            var opensUntilRare = double.IsInfinity(stats.ExpectedOpensUntilRare) == true ? "never" : stats.ExpectedOpensUntilRare.ToString(OpensFormat, CultureInfo.InvariantCulture);
            EditorGUILayout.LabelField($"Expected opens until {_config.RareRarity}+", opensUntilRare);
            EditorGUILayout.LabelField("Clicker clicks per open", stats.ClicksPerOpen.ToString(CultureInfo.InvariantCulture));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Chance per rarity", EditorStyles.boldLabel);

            foreach (var pair in stats.RarityChances)
            {
                var style = CaseEditorGuiTools.CreateColoredLabel(EditorStyles.label, CaseEditorGuiTools.GetRarityColor(_config.SkinsUIContextDataConfig, pair.Key));
                EditorGUILayout.LabelField(pair.Key.ToString(), CaseEditorGuiTools.FormatPercent(pair.Value), style);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Top {_config.TopContributorsCount} EV contributors", EditorStyles.boldLabel);

            foreach (var entry in stats.TopContributors)
            {
                var share = stats.ExpectedValue > 0d ? entry.ExpectedValueContribution / stats.ExpectedValue : 0d;
                EditorGUILayout.LabelField(entry.DisplayName, $"{CaseEditorGuiTools.FormatMoney(entry.ExpectedValueContribution)} ({CaseEditorGuiTools.FormatPercent(share)})");
            }

            if (simulationResult != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Monte-Carlo check", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Opens", simulationResult.OpensCount.ToString(CultureInfo.InvariantCulture));
                EditorGUILayout.LabelField("Simulated mean / analytic EV", $"{CaseEditorGuiTools.FormatMoney(simulationResult.MeanValue)} / {CaseEditorGuiTools.FormatMoney(stats.ExpectedValue)}");

                var difference = stats.ExpectedValue > 0d ? simulationResult.MeanValue / stats.ExpectedValue - 1d : 0d;
                EditorGUILayout.LabelField("Difference", CaseEditorGuiTools.FormatPercent(difference));
                EditorGUILayout.LabelField("Simulated profit chance", CaseEditorGuiTools.FormatPercent(simulationResult.ProfitChance));
            }

            EditorGUIUtility.labelWidth = previousLabelWidth;
        }

        private void DrawWarnings(CaseStats stats, IReadOnlyList<SkinConfig> skins)
        {
            if (skins.Count == 0)
            {
                EditorGUILayout.HelpBox("The case has no skins", MessageType.Error);
            }
            else if (stats.IsValid == false)
            {
                EditorGUILayout.HelpBox("All weights are zero", MessageType.Error);
            }

            if (stats.Price <= 0d)
            {
                EditorGUILayout.HelpBox("Price must be positive", MessageType.Error);
            }

            if (stats.IsValid == true && stats.ReturnToPlayer > _config.MaxReturnToPlayer)
            {
                EditorGUILayout.HelpBox($"RTP {CaseEditorGuiTools.FormatPercent(stats.ReturnToPlayer)}: the player profits on average", MessageType.Error);
            }
            else if (stats.IsValid == true && stats.ReturnToPlayer < _config.MinReturnToPlayer)
            {
                EditorGUILayout.HelpBox($"RTP {CaseEditorGuiTools.FormatPercent(stats.ReturnToPlayer)} is below {CaseEditorGuiTools.FormatPercent(_config.MinReturnToPlayer)}: the case feels like a rip-off", MessageType.Warning);
            }

            if (stats.Price > 0d && IsMultipleOfStep(stats.Price) == false)
            {
                EditorGUILayout.HelpBox($"Price {stats.Price.ToString(CultureInfo.InvariantCulture)} is finer than the price step {_config.PriceStep.ToString(CultureInfo.InvariantCulture)}: the sheet keeps 2 decimals, so a push and pull changes it", MessageType.Warning);
            }

            if (stats.DominantEntries.Count > 0)
            {
                var names = string.Join(", ", stats.DominantEntries.Select(entry => $"{entry.DisplayName} ({CaseEditorGuiTools.FormatPercent(entry.ExpectedValueContribution / stats.ExpectedValue)})"));
                EditorGUILayout.HelpBox($"One skin gives more than {CaseEditorGuiTools.FormatPercent(_config.DominantShare)} of EV: {names}", MessageType.Warning);
            }

            if (stats.OverpayingEntries.Count > 0)
            {
                var names = string.Join(", ", stats.OverpayingEntries.Select(entry => $"{entry.DisplayName} ({entry.Rarity}, mean {CaseEditorGuiTools.FormatMoney(entry.MeanValue)})"));
                EditorGUILayout.HelpBox($"Low-rarity skin pays more than the case (a common tier, at least {CaseEditorGuiTools.FormatPercent(_config.CommonRarityChance)} chance, below the top rarity): {names}", MessageType.Warning);
            }

            var skinsWithoutIcon = skins.Where(skin => _catalog.HasIcon(skin) == false).Select(skin => skin.Id.Value).ToList();

            if (skinsWithoutIcon.Count > 0)
            {
                EditorGUILayout.HelpBox($"Skins without icon: {string.Join(", ", skinsWithoutIcon)}", MessageType.Warning);
            }
        }

        private bool IsMultipleOfStep(double price)
        {
            var steps = price / _config.PriceStep;

            return Math.Abs(steps - Math.Round(steps)) < PriceStepTolerance;
        }

        private GUIStyle GetReturnToPlayerStyle(double returnToPlayer)
        {
            if (returnToPlayer > _config.MaxReturnToPlayer)
            {
                return _errorStyle;
            }

            if (returnToPlayer < _config.MinReturnToPlayer)
            {
                return _warningStyle;
            }

            return _goodStyle;
        }

        private void EnsureStyles()
        {
            if (_errorStyle != null)
            {
                return;
            }

            _errorStyle = CaseEditorGuiTools.CreateColoredLabel(EditorStyles.boldLabel, _config.ErrorColor);
            _warningStyle = CaseEditorGuiTools.CreateColoredLabel(EditorStyles.boldLabel, _config.WarningColor);
            _goodStyle = CaseEditorGuiTools.CreateColoredLabel(EditorStyles.boldLabel, _config.GoodColor);
        }
    }
}

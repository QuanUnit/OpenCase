using System;
using System.Collections.Generic;
using System.Globalization;
using Features.Skins.Configs;
using Features.Skins.Data;
using QCore.Tools.Collections;
using UnityEditor;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Cases
{
    public static class CaseEditorGuiTools
    {
        private const string MoneyFormat = "0.00";
        private const string PercentFormat = "0.###";
        private const float SmallMoney = 0.01f;
        private const string SmallMoneyFormat = "0.####";

        private static readonly Color EmptyIconColor = new Color(0f, 0f, 0f, 0.2f);

        public static void DrawSprite(Rect rect, Sprite sprite)
        {
            if (sprite == null)
            {
                EditorGUI.DrawRect(rect, EmptyIconColor);
                return;
            }

            var texture = sprite.texture;
            var textureRect = sprite.textureRect;
            var coordinates = new Rect(textureRect.x / texture.width, textureRect.y / texture.height, textureRect.width / texture.width, textureRect.height / texture.height);

            var aspect = textureRect.width / textureRect.height;
            var fitted = rect;

            if (aspect > rect.width / rect.height)
            {
                fitted.height = rect.width / aspect;
                fitted.y += (rect.height - fitted.height) * 0.5f;
            }
            else
            {
                fitted.width = rect.height * aspect;
                fitted.x += (rect.width - fitted.width) * 0.5f;
            }

            GUI.DrawTextureWithTexCoords(fitted, texture, coordinates);
        }

        public static Color GetRarityColor(SkinsUIContextDataConfig config, Rarity rarity)
        {
            foreach (var visual in config.RarityVisuals)
            {
                if (visual.Key == rarity)
                {
                    return visual.Value.RarityMainColor;
                }
            }

            return Color.white;
        }

        public static GUIStyle CreateColoredLabel(GUIStyle source, Color color)
        {
            var style = new GUIStyle(source);
            style.normal.textColor = color;
            style.hover.textColor = color;

            return style;
        }

        public static string FormatMoney(double value)
        {
            if (double.IsInfinity(value) == true || double.IsNaN(value) == true)
            {
                return "-";
            }

            if (value != 0d && Math.Abs(value) < SmallMoney)
            {
                return value.ToString(SmallMoneyFormat, CultureInfo.InvariantCulture);
            }

            return value.ToString(MoneyFormat, CultureInfo.InvariantCulture);
        }

        public static string FormatPercent(double share)
        {
            if (double.IsInfinity(share) == true || double.IsNaN(share) == true)
            {
                return "-";
            }

            return (share * 100d).ToString(PercentFormat, CultureInfo.InvariantCulture) + "%";
        }

        public static string FormatPriceRange(PriceRange priceRange)
        {
            return $"{FormatMoney(priceRange.MinPrice)} - {FormatMoney(priceRange.MaxPrice)}";
        }

        public static List<Rarity> GetRarities(IReadOnlyList<KeyValue<SkinConfig, float>> entries)
        {
            var rarities = new List<Rarity>(entries.Count);

            foreach (var entry in entries)
            {
                rarities.Add(entry.Key.Rarity);
            }

            return rarities;
        }
    }
}

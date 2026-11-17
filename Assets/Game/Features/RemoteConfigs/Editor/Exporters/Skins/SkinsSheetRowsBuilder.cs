using System;
using System.Collections.Generic;
using Features.RemoteConfigs.Editor.Models;
using Features.Skins.Data;
using QCore.Tools.Mathematics;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Exporters.Skins
{
    public class SkinsSheetRowsBuilder
    {
        private readonly HashSet<string> _excludedCategories;
        private readonly IReadOnlyDictionary<string, Rarity> _rarityMap;
        private readonly string _souvenirCrateMarker;

        public SkinsSheetRowsBuilder(IEnumerable<string> excludedCategories, IReadOnlyDictionary<string, Rarity> rarityMap, string souvenirCrateMarker)
        {
            _excludedCategories = new HashSet<string>(excludedCategories);
            _rarityMap = rarityMap;
            _souvenirCrateMarker = souvenirCrateMarker;
        }

        public SkinsSheetBuildResult Build(IReadOnlyList<SourceSkin> skins, IReadOnlyDictionary<string, float> prices)
        {
            var result = new SkinsSheetBuildResult();
            var rowsById = new Dictionary<string, SkinDataModel>();

            foreach (var skin in skins)
            {
                if (_excludedCategories.Contains(skin.category.name) == true)
                {
                    continue;
                }

                var name = SkinIds.GetName(skin.name);
                var id = SkinIds.Create(skin.weapon.name, name);

                if (TryGetRarity(skin.rarity.name, out var rarity) == false)
                {
                    AddUnique(result.SkippedUnknownRarity, $"{skin.name} ({skin.rarity.name})");
                    result.SkippedIds.Add(id);
                    continue;
                }

                if (TryGetPrice(skin, name, prices, out var price) == false)
                {
                    AddUnique(result.SkippedWithoutPrice, skin.name);
                    result.SkippedIds.Add(id);
                    continue;
                }

                if (rowsById.TryGetValue(id, out var existingRow) == true)
                {
                    MergeVariant(existingRow, skin, price);
                    continue;
                }

                var row = new SkinDataModel
                {
                    Id = id,
                    Name = name,
                    Rarity = rarity,
                    Price = price,
                    AvailableStatTrak = skin.stattrak,
                    AvailableSouvenir = IsSouvenirAvailable(skin),
                };

                rowsById.Add(id, row);
                result.Rows.Add(row);
            }

            return result;
        }

        private bool TryGetRarity(string rarityName, out Rarity rarity)
        {
            rarity = default;

            if (string.IsNullOrEmpty(rarityName) == true)
            {
                return false;
            }

            return _rarityMap.TryGetValue(rarityName, out rarity);
        }

        private bool TryGetPrice(SourceSkin skin, string name, IReadOnlyDictionary<string, float> prices, out FloatRange price)
        {
            price = default;

            var min = float.MaxValue;
            var max = float.MinValue;
            var isFound = false;

            foreach (var wear in skin.wears)
            {
                if (prices.TryGetValue(SkinIds.GetMarketHashName(skin.weapon.name, name, wear.name), out var wearPrice) == false)
                {
                    continue;
                }

                min = Mathf.Min(min, wearPrice);
                max = Mathf.Max(max, wearPrice);
                isFound = true;
            }

            if (isFound == false)
            {
                return false;
            }

            price = new FloatRange(min, max);
            return true;
        }

        private void MergeVariant(SkinDataModel row, SourceSkin skin, FloatRange price)
        {
            row.Price = new FloatRange(Mathf.Min(row.Price.From, price.From), Mathf.Max(row.Price.To, price.To));
            row.AvailableStatTrak = row.AvailableStatTrak == true || skin.stattrak == true;
            row.AvailableSouvenir = row.AvailableSouvenir == true || IsSouvenirAvailable(skin) == true;
        }

        private bool IsSouvenirAvailable(SourceSkin skin)
        {
            if (skin.souvenir == false)
            {
                return false;
            }

            foreach (var crate in skin.crates)
            {
                if (crate.name.Contains(_souvenirCrateMarker, StringComparison.Ordinal) == true)
                {
                    return true;
                }
            }

            return false;
        }

        private void AddUnique(List<string> list, string value)
        {
            if (list.Contains(value) == false)
            {
                list.Add(value);
            }
        }
    }
}

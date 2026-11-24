using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Exporters.Skins
{
    public class SkinsSourcesParser
    {
        public List<SourceSkin> ParseSkins(string skinsJson)
        {
            var wrappedJson = "{\"Items\":" + skinsJson + "}";
            var skinsList = JsonUtility.FromJson<SourceSkinsList>(wrappedJson);

            foreach (var skin in skinsList.Items)
            {
                FillMissingFields(skin);
            }

            return new List<SourceSkin>(skinsList.Items);
        }

        public Dictionary<string, float> ParsePrices(string pricesJson)
        {
            var response = JsonUtility.FromJson<MarketPricesResponse>(pricesJson);

            if (response.success == false || response.items == null)
            {
                throw new InvalidOperationException("Prices source returned an unsuccessful response");
            }

            var prices = new Dictionary<string, float>(response.items.Length);

            foreach (var marketPrice in response.items)
            {
                var isParsed = float.TryParse(marketPrice.price, NumberStyles.Float, CultureInfo.InvariantCulture, out var price);

                if (isParsed == true)
                {
                    prices[marketPrice.market_hash_name] = price;
                }
            }

            return prices;
        }

        private void FillMissingFields(SourceSkin skin)
        {
            skin.rarity ??= new SourceNamedEntity();
            skin.wears ??= Array.Empty<SourceNamedEntity>();
            skin.crates ??= Array.Empty<SourceNamedEntity>();
            skin.collections ??= Array.Empty<SourceNamedEntity>();
        }
    }
}

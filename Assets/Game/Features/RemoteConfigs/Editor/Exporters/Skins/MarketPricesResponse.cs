using System;

namespace Features.RemoteConfigs.Editor.Exporters.Skins
{
    [Serializable]
    public class MarketPricesResponse
    {
        public bool success;
        public MarketPrice[] items;
    }

    [Serializable]
    public class MarketPrice
    {
        public string market_hash_name;
        public string price;
    }
}

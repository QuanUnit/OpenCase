using System.Collections.Generic;
using Features.Skins.Data;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class CaseStats
    {
        public bool IsValid;
        public double Price;
        public double ExpectedValue;
        public double ReturnToPlayer;
        public double HouseEdge;
        public double Median;
        public double ProfitChance;
        public double Min;
        public double Max;
        public double StandardDeviation;
        public double ExpectedOpensUntilRare;
        public long ClicksPerOpen;
        public readonly List<PriceMultipleChance> MultipleChances = new List<PriceMultipleChance>();
        public readonly SortedDictionary<Rarity, double> RarityChances = new SortedDictionary<Rarity, double>();
        public readonly List<EntryStats> Entries = new List<EntryStats>();
        public readonly List<EntryStats> TopContributors = new List<EntryStats>();
        public readonly List<EntryStats> DominantEntries = new List<EntryStats>();
        public readonly List<EntryStats> OverpayingEntries = new List<EntryStats>();

        public class PriceMultipleChance
        {
            public float Multiplier;
            public double Chance;
        }

        public class EntryStats
        {
            public int Index;
            public string SkinId;
            public string DisplayName;
            public Rarity Rarity;
            public double DropChance;
            public double MeanValue;
            public double ExpectedValueContribution;
        }
    }
}

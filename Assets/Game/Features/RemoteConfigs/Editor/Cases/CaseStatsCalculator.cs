using System;
using System.Collections.Generic;
using System.Linq;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class CaseStatsCalculator
    {
        private const int MedianIterations = 64;
        private const double MedianProbability = 0.5d;
        private const int ClicksRoundingDigits = 4;

        public CaseStats Calculate(IReadOnlyList<CaseStatsEntry> entries, double price, CaseStatsSettings settings)
        {
            var stats = new CaseStats { Price = price };
            var totalWeight = entries.Sum(entry => (double)Math.Max(0f, entry.Weight));

            stats.ClicksPerOpen = CalculateClicks(price, settings.RewardPerClick);

            if (entries.Count == 0 || totalWeight <= 0d)
            {
                return stats;
            }

            stats.IsValid = true;

            FillEntries(stats, entries, totalWeight);
            FillMoments(stats, entries);
            FillRange(stats, entries);
            FillRarities(stats, settings);

            stats.ReturnToPlayer = price > 0d ? stats.ExpectedValue / price : double.PositiveInfinity;
            stats.HouseEdge = 1d - stats.ReturnToPlayer;
            stats.Median = CalculateMedian(stats, entries);
            stats.ProfitChance = GetProbabilityAtLeast(stats, entries, price);

            foreach (var multiplier in settings.PriceMultipliers)
            {
                stats.MultipleChances.Add(new CaseStats.PriceMultipleChance
                {
                    Multiplier = multiplier,
                    Chance = GetProbabilityAtLeast(stats, entries, price * multiplier),
                });
            }

            stats.TopContributors.AddRange(stats.Entries
                .Where(entry => entry.DropChance > 0d)
                .OrderByDescending(entry => entry.ExpectedValueContribution)
                .Take(settings.TopContributorsCount));

            FillProblems(stats, settings);

            return stats;
        }

        private void FillProblems(CaseStats stats, CaseStatsSettings settings)
        {
            var possibleEntries = stats.Entries.Where(entry => entry.DropChance > 0d).ToList();
            var topRarity = possibleEntries.Max(entry => entry.Rarity);

            foreach (var entry in possibleEntries)
            {
                if (stats.ExpectedValue > 0d && entry.ExpectedValueContribution / stats.ExpectedValue > settings.DominantShare)
                {
                    stats.DominantEntries.Add(entry);
                }

                var isCommonRarity = entry.Rarity < topRarity && stats.RarityChances[entry.Rarity] >= settings.CommonRarityChance;

                if (isCommonRarity == true && entry.MeanValue > stats.Price)
                {
                    stats.OverpayingEntries.Add(entry);
                }
            }
        }

        private void FillEntries(CaseStats stats, IReadOnlyList<CaseStatsEntry> entries, double totalWeight)
        {
            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                var dropChance = Math.Max(0f, entry.Weight) / totalWeight;

                stats.Entries.Add(new CaseStats.EntryStats
                {
                    Index = index,
                    SkinId = entry.SkinId,
                    DisplayName = entry.DisplayName,
                    Rarity = entry.Rarity,
                    DropChance = dropChance,
                    MeanValue = entry.Distribution.Mean,
                    ExpectedValueContribution = dropChance * entry.Distribution.Mean,
                });
            }
        }

        private void FillMoments(CaseStats stats, IReadOnlyList<CaseStatsEntry> entries)
        {
            var meanOfSquares = 0d;

            foreach (var entry in stats.Entries)
            {
                stats.ExpectedValue += entry.ExpectedValueContribution;
            }

            for (var index = 0; index < stats.Entries.Count; index++)
            {
                meanOfSquares += stats.Entries[index].DropChance * entries[index].Distribution.MeanOfSquares;
            }

            var variance = Math.Max(0d, meanOfSquares - stats.ExpectedValue * stats.ExpectedValue);
            stats.StandardDeviation = Math.Sqrt(variance);
        }

        private void FillRange(CaseStats stats, IReadOnlyList<CaseStatsEntry> entries)
        {
            stats.Min = double.MaxValue;
            stats.Max = double.MinValue;

            for (var index = 0; index < stats.Entries.Count; index++)
            {
                if (stats.Entries[index].DropChance <= 0d)
                {
                    continue;
                }

                stats.Min = Math.Min(stats.Min, entries[index].Distribution.Min);
                stats.Max = Math.Max(stats.Max, entries[index].Distribution.Max);
            }
        }

        private void FillRarities(CaseStats stats, CaseStatsSettings settings)
        {
            var rareChance = 0d;

            foreach (var entry in stats.Entries)
            {
                stats.RarityChances.TryGetValue(entry.Rarity, out var chance);
                stats.RarityChances[entry.Rarity] = chance + entry.DropChance;

                if (entry.Rarity >= settings.RareRarity)
                {
                    rareChance += entry.DropChance;
                }
            }

            stats.ExpectedOpensUntilRare = rareChance > 0d ? 1d / rareChance : double.PositiveInfinity;
        }

        private double CalculateMedian(CaseStats stats, IReadOnlyList<CaseStatsEntry> entries)
        {
            var low = stats.Min;
            var high = stats.Max;

            for (var iteration = 0; iteration < MedianIterations; iteration++)
            {
                var middle = (low + high) * 0.5d;

                if (GetProbabilityAtLeast(stats, entries, middle) >= MedianProbability)
                {
                    low = middle;
                }
                else
                {
                    high = middle;
                }
            }

            return low;
        }

        private double GetProbabilityAtLeast(CaseStats stats, IReadOnlyList<CaseStatsEntry> entries, double value)
        {
            var probability = 0d;

            for (var index = 0; index < entries.Count; index++)
            {
                probability += stats.Entries[index].DropChance * entries[index].Distribution.GetProbabilityAtLeast(value);
            }

            return probability;
        }

        private long CalculateClicks(double price, float rewardPerClick)
        {
            if (rewardPerClick <= 0f || price <= 0d)
            {
                return 0;
            }

            return (long)Math.Ceiling(Math.Round(price / rewardPerClick, ClicksRoundingDigits));
        }
    }
}

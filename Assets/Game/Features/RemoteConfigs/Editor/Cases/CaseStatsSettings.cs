using System.Collections.Generic;
using Features.Skins.Data;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class CaseStatsSettings
    {
        public readonly IReadOnlyList<float> PriceMultipliers;
        public readonly Rarity RareRarity;
        public readonly int TopContributorsCount;
        public readonly float RewardPerClick;
        public readonly float DominantShare;
        public readonly float CommonRarityChance;

        public CaseStatsSettings(IReadOnlyList<float> priceMultipliers, Rarity rareRarity, int topContributorsCount, float rewardPerClick, float dominantShare, float commonRarityChance)
        {
            PriceMultipliers = priceMultipliers;
            RareRarity = rareRarity;
            TopContributorsCount = topContributorsCount;
            RewardPerClick = rewardPerClick;
            DominantShare = dominantShare;
            CommonRarityChance = commonRarityChance;
        }
    }
}

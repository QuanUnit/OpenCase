using Features.Skins.Data;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class CaseStatsEntry
    {
        public readonly string SkinId;
        public readonly string DisplayName;
        public readonly Rarity Rarity;
        public readonly float Weight;
        public readonly SkinValueDistribution Distribution;

        public CaseStatsEntry(string skinId, string displayName, Rarity rarity, float weight, SkinValueDistribution distribution)
        {
            SkinId = skinId;
            DisplayName = displayName;
            Rarity = rarity;
            Weight = weight;
            Distribution = distribution;
        }
    }
}

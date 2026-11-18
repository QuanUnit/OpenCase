using System.Collections.Generic;
using Features.Skins.Data;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class RarityWeightsResult
    {
        public readonly List<float> Weights = new List<float>();
        public readonly List<Rarity> RaritiesWithoutOdds = new List<Rarity>();
    }
}

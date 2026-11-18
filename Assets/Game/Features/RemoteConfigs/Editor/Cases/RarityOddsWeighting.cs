using System.Collections.Generic;
using System.Linq;
using Features.Skins.Data;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class RarityOddsWeighting
    {
        private readonly IReadOnlyDictionary<Rarity, float> _odds;
        private readonly IReadOnlyDictionary<Rarity, float> _fixedShares;
        private readonly float _totalWeight;

        public RarityOddsWeighting(IReadOnlyDictionary<Rarity, float> odds, IReadOnlyDictionary<Rarity, float> fixedShares, float totalWeight)
        {
            _odds = odds;
            _fixedShares = fixedShares;
            _totalWeight = totalWeight;
        }

        public RarityWeightsResult Calculate(IReadOnlyList<Rarity> rarities)
        {
            var result = new RarityWeightsResult();
            var countsByRarity = rarities.GroupBy(rarity => rarity).ToDictionary(group => group.Key, group => group.Count());
            var shares = CalculateShares(countsByRarity.Keys, result);

            foreach (var rarity in rarities)
            {
                shares.TryGetValue(rarity, out var share);
                result.Weights.Add((float)(share * _totalWeight / countsByRarity[rarity]));
            }

            result.RaritiesWithoutOdds.Sort();

            return result;
        }

        private Dictionary<Rarity, double> CalculateShares(IEnumerable<Rarity> presentRarities, RarityWeightsResult result)
        {
            var shares = new Dictionary<Rarity, double>();
            var fixedShares = new Dictionary<Rarity, double>();
            var relativeOdds = new Dictionary<Rarity, double>();

            foreach (var rarity in presentRarities)
            {
                if (_fixedShares.TryGetValue(rarity, out var fixedShare) == true && fixedShare > 0f)
                {
                    fixedShares[rarity] = fixedShare;
                }
                else if (GetOdds(rarity) > 0d)
                {
                    relativeOdds[rarity] = GetOdds(rarity);
                }
                else
                {
                    result.RaritiesWithoutOdds.Add(rarity);
                }
            }

            var fixedTotal = fixedShares.Values.Sum();
            var oddsTotal = relativeOdds.Values.Sum();

            if (oddsTotal <= 0d || fixedTotal >= 1d)
            {
                foreach (var pair in fixedShares)
                {
                    shares[pair.Key] = pair.Value / fixedTotal;
                }

                return shares;
            }

            foreach (var pair in fixedShares)
            {
                shares[pair.Key] = pair.Value;
            }

            foreach (var pair in relativeOdds)
            {
                shares[pair.Key] = pair.Value / oddsTotal * (1d - fixedTotal);
            }

            return shares;
        }

        private double GetOdds(Rarity rarity)
        {
            if (_odds.TryGetValue(rarity, out var odds) == true)
            {
                return odds;
            }

            return 0d;
        }
    }
}

using System.Collections.Generic;
using Features.Skins.Configs;
using QCore.Tools.Collections;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class CaseStatsBuilder
    {
        private readonly SkinCatalog _catalog;
        private readonly CaseStatsCalculator _calculator = new CaseStatsCalculator();

        public CaseStatsBuilder(SkinCatalog catalog)
        {
            _catalog = catalog;
        }

        public List<CaseStatsEntry> BuildEntries(IReadOnlyList<KeyValue<SkinConfig, float>> entries)
        {
            _catalog.RefreshValueModel();

            var result = new List<CaseStatsEntry>(entries.Count);

            foreach (var entry in entries)
            {
                if (entry.Key == null)
                {
                    continue;
                }

                var info = _catalog.GetInfo(entry.Key);
                result.Add(new CaseStatsEntry(entry.Key.SkinStringId, info.DisplayName, entry.Key.Rarity, entry.Value, _catalog.GetDistribution(entry.Key)));
            }

            return result;
        }

        public CaseStats Calculate(IReadOnlyList<KeyValue<SkinConfig, float>> entries, float price, CaseStatsSettings settings)
        {
            return _calculator.Calculate(BuildEntries(entries), price, settings);
        }
    }
}

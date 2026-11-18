using Features.Cases.Configs;
using Features.Skins.Configs;
using Features.Skins.Services;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class CaseMonteCarloSimulator
    {
        private readonly SkinFactory _skinFactory;

        public CaseMonteCarloSimulator(SkinsGlobalSettingsConfig globalSettingsConfig)
        {
            _skinFactory = new SkinFactory(globalSettingsConfig);
        }

        public CaseSimulationResult Run(CaseConfig caseConfig, int opensCount)
        {
            var totalValue = 0d;
            var profitableCount = 0;

            for (var index = 0; index < opensCount; index++)
            {
                var skinConfig = caseConfig.SkinsMapEntries.GetRandom();
                var skin = _skinFactory.CreateRandom(skinConfig);

                totalValue += skin.Price;

                if (skin.Price >= caseConfig.Price)
                {
                    profitableCount++;
                }
            }

            return new CaseSimulationResult(opensCount, totalValue / opensCount, (double)profitableCount / opensCount);
        }
    }
}

namespace Features.RemoteConfigs.Editor.Cases
{
    public class CaseSimulationResult
    {
        public readonly int OpensCount;
        public readonly double MeanValue;
        public readonly double ProfitChance;

        public CaseSimulationResult(int opensCount, double meanValue, double profitChance)
        {
            OpensCount = opensCount;
            MeanValue = meanValue;
            ProfitChance = profitChance;
        }
    }
}

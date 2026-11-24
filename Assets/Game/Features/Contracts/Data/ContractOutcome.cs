using Features.Skins.Configs;

namespace Features.Contracts.Data
{
    public class ContractOutcome
    {
        public SkinConfig Config { get; }
        public float Chance { get; }

        public ContractOutcome(SkinConfig config, float chance)
        {
            Config = config;
            Chance = chance;
        }
    }
}

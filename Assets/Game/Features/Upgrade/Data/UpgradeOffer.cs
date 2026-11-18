using Features.Skins.Data;

namespace Features.Upgrade.Data
{
    public class UpgradeOffer
    {
        public Skin Source { get; private set; }
        public Skin Target { get; private set; }
        public float Multiplier { get; private set; }
        public float Chance { get; private set; }

        public UpgradeOffer(Skin source, Skin target, float multiplier, float chance)
        {
            Source = source;
            Target = target;
            Multiplier = multiplier;
            Chance = chance;
        }
    }
}

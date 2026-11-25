using Features.Skins.Data;

namespace Features.Upgrade.Data
{
    public class UpgradeOffer
    {
        public Skin Source { get; private set; }
        public Skin Target { get; private set; }
        public float Extra { get; private set; }
        public float Chance { get; private set; }

        public UpgradeOffer(Skin source, Skin target, float extra, float chance)
        {
            Source = source;
            Target = target;
            Extra = extra;
            Chance = chance;
        }
    }
}

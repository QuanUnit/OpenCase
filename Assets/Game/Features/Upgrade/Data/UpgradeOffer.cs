using System.Collections.Generic;
using Features.Skins.Data;

namespace Features.Upgrade.Data
{
    public class UpgradeOffer
    {
        public IReadOnlyList<Skin> Sources { get; private set; }
        public Skin Target { get; private set; }
        public float Extra { get; private set; }
        public float Chance { get; private set; }

        public UpgradeOffer(IReadOnlyList<Skin> sources, Skin target, float extra, float chance)
        {
            Sources = sources;
            Target = target;
            Extra = extra;
            Chance = chance;
        }
    }
}

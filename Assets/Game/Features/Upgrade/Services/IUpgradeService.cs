using System.Collections.Generic;
using Features.Skins.Data;
using Features.Upgrade.Data;

namespace Features.Upgrade.Services
{
    public interface IUpgradeService
    {
        public IReadOnlyList<float> Multipliers { get; }

        public bool TryCreateOffer(Skin source, float multiplier, out UpgradeOffer offer);
        public bool Upgrade(UpgradeOffer offer);
    }
}

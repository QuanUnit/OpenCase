using System.Collections.Generic;
using Features.Skins.Data;
using Features.Upgrade.Data;

namespace Features.Upgrade.Services
{
    public interface IUpgradeService
    {
        public IReadOnlyList<float> Multipliers { get; }

        public IReadOnlyList<Skin> CreateTargets(Skin source, float multiplier);
        public float GetChance(Skin source, Skin target, float extra);
        public float GetMaxExtra(Skin source, Skin target);
        public UpgradeOffer CreateOffer(Skin source, Skin target, float extra);
        public UpgradeResult Upgrade(UpgradeOffer offer);
    }
}

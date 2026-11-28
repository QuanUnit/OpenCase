using System.Collections.Generic;
using Features.Skins.Data;
using Features.Upgrade.Data;

namespace Features.Upgrade.Services
{
    public interface IUpgradeService
    {
        public IReadOnlyList<float> Multipliers { get; }

        public float GetSourcesPrice(IReadOnlyList<Skin> sources);
        public IReadOnlyList<Skin> CreateTargets(IReadOnlyList<Skin> sources, float multiplier);
        public float GetChance(IReadOnlyList<Skin> sources, Skin target, float extra);
        public float GetMaxExtra(IReadOnlyList<Skin> sources, Skin target);
        public UpgradeOffer CreateOffer(IReadOnlyList<Skin> sources, Skin target, float extra);
        public UpgradeResult Upgrade(UpgradeOffer offer);
    }
}

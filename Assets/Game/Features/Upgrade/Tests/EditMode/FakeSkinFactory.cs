using System.Collections.Generic;
using Features.Skins.Configs;
using Features.Skins.Data;
using Features.Skins.Services;
using QCore.Tools.Identifiers;

namespace Features.Upgrade.Tests.EditMode
{
    public class FakeSkinFactory : ISkinFactory
    {
        public int CreatedCount { get; private set; }

        private readonly Dictionary<SkinConfig, Queue<float>> _prices = new Dictionary<SkinConfig, Queue<float>>();
        private readonly Dictionary<SkinConfig, float> _lastPrices = new Dictionary<SkinConfig, float>();

        public void SetPrices(SkinConfig config, params float[] prices)
        {
            _prices[config] = new Queue<float>(prices);
        }

        public Skin CreateRandom(SkinConfig config)
        {
            CreatedCount++;

            var queue = _prices[config];

            if (queue.Count > 0)
            {
                _lastPrices[config] = queue.Dequeue();
            }

            return new Skin(config.Id, QGuid.CreateNew(), 0.1f, _lastPrices[config], false, false);
        }

        public Skin Create(SkinConfig config, float floatValue, bool isStatTrak)
        {
            return new Skin(config.Id, QGuid.CreateNew(), floatValue, 0f, isStatTrak, false);
        }
    }
}

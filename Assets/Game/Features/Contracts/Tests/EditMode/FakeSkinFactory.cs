using Features.Skins.Configs;
using Features.Skins.Data;
using Features.Skins.Services;
using QCore.Tools.Identifiers;

namespace Features.Contracts.Tests.EditMode
{
    public class FakeSkinFactory : ISkinFactory
    {
        public SkinConfig LastConfig { get; private set; }
        public float LastFloatValue { get; private set; }
        public bool LastIsStatTrak { get; private set; }

        public Skin CreateRandom(SkinConfig config)
        {
            return Create(config, 0.1f, false);
        }

        public Skin Create(SkinConfig config, float floatValue, bool isStatTrak)
        {
            LastConfig = config;
            LastFloatValue = floatValue;
            LastIsStatTrak = isStatTrak;

            return new Skin(config.Id, QGuid.CreateNew(), floatValue, 0f, isStatTrak, false);
        }
    }
}

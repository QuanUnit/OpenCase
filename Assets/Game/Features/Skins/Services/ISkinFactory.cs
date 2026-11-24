using Features.Skins.Configs;
using Features.Skins.Data;

namespace Features.Skins.Services
{
    public interface ISkinFactory
    {
        public Skin CreateRandom(SkinConfig config);

        public Skin Create(SkinConfig config, float floatValue, bool isStatTrak);
    }
}